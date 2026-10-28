using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SRT.Core.ENV;
using SRT.Core.SRTExtensions;

namespace SRT.Core.GeneralFunctions
{
    /// <summary>
    /// Central cookie policy for auth sessions and registration tickets.
    /// Session cookie MaxAge uses AuthSettings:SessionLifetimeDays (default 10 days), the same value as the Authentication session.
    /// Cookies are host-only (no Domain attribute) so same-origin /api works behind CDN/Ocelot.
    /// </summary>
    public static class SessionCookieService
    {
        public const string SessionLifetimeDaysKey = "AuthSettings:SessionLifetimeDays";
        public const string RegistrationTicketHoursKey = "AuthSettings:RegistrationTicketHours";

        /// <summary>Fallback AuthToken cookie MaxAge when AuthSettings:SessionLifetimeDays is missing. Matches the Authentication session default.</summary>
        public const int DefaultSessionLifetimeDays = 10;

        /// <summary>Fallback RegTicket cookie MaxAge when AuthSettings:RegistrationTicketHours is missing.</summary>
        public const int DefaultRegistrationTicketHours = 12;

        public const string SessionCookieName = "AuthToken";
        public const string RegistrationTicketCookieName = "RegTicket";
        /// <summary>Legacy JS-readable cookie — cleared on write/clear during migration.</summary>
        public const string LegacyTokenCookieName = "TOKEN";
        public const string LegacyRoleCookieName = "ROLE_ID";

        public static readonly TimeSpan SessionMaxAge = TimeSpan.FromDays(DefaultSessionLifetimeDays);
        public static readonly TimeSpan RegistrationTicketMaxAge = TimeSpan.FromHours(DefaultRegistrationTicketHours);

        public static void SetSession(HttpContext context, string sessionId)
        {
            // Expire leftover parent-domain AuthToken first so the browser only keeps host-only.
            ExpireLeftoverParentCookie(context, SessionCookieName, httpOnly: true);
            AppendCookie(context, SessionCookieName, sessionId, httpOnly: true, SessionLifetime(context));
            // Stop issuing non-HttpOnly TOKEN; clear residue so XSS cannot read the session.
            ClearCookie(context, LegacyTokenCookieName);
            context.Response.Headers.TryAdd("SetAuthToken", "true");
        }

        public static void ClearSession(HttpContext context)
        {
            ClearCookie(context, SessionCookieName);
            ClearCookie(context, LegacyTokenCookieName);
            ClearCookie(context, LegacyRoleCookieName);
            context.Response.Headers.TryAdd("SetAuthToken", "true");
        }

        public static void SetRegistrationTicket(HttpContext context, string ticketGuid)
        {
            ExpireLeftoverParentCookie(context, RegistrationTicketCookieName, httpOnly: true);
            AppendCookie(context, RegistrationTicketCookieName, ticketGuid, httpOnly: true, RegistrationTicketLifetime(context));
            context.Response.Headers.TryAdd("SetAuthToken", "true");
        }

        public static void ClearRegistrationTicket(HttpContext context)
        {
            ClearCookie(context, RegistrationTicketCookieName);
        }

        public static string? ReadSession(HttpContext context, bool preferBearer = false)
        {
            if (preferBearer
                && context.Request.Headers.TryGetValue("Authorization", out var headerValue))
            {
                var parts = headerValue.ToString().Split(' ');
                if (parts.Length == 2 && !parts[1].SRT_String_IsNullOrEmpty())
                    return parts[1];
            }

            if (context.Request.Cookies.TryGetValue(SessionCookieName, out var cookieToken)
                && !cookieToken.SRT_String_IsNullOrEmpty())
                return cookieToken;

            // Migration: still accept legacy TOKEN until fronts stop sending it.
            if (context.Request.Cookies.TryGetValue(LegacyTokenCookieName, out var legacy)
                && !legacy.SRT_String_IsNullOrEmpty())
                return legacy;

            return null;
        }

        public static string? ReadRegistrationTicket(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue(RegistrationTicketCookieName, out var ticket)
                && !ticket.SRT_String_IsNullOrEmpty())
                return ticket;
            return null;
        }

        public static TimeSpan SessionLifetime(HttpContext context)
            => TimeSpan.FromDays(ReadPositive(context, SessionLifetimeDaysKey, DefaultSessionLifetimeDays));

        public static TimeSpan RegistrationTicketLifetime(HttpContext context)
            => TimeSpan.FromHours(ReadPositive(context, RegistrationTicketHoursKey, DefaultRegistrationTicketHours));

        private static int ReadPositive(HttpContext context, string key, int fallback)
        {
            var configured = context.RequestServices.GetService<IConfiguration>()?.GetValue<int?>(key);
            return configured is > 0 ? configured.Value : fallback;
        }

        private static void AppendCookie(
            HttpContext context,
            string name,
            string value,
            bool httpOnly,
            TimeSpan maxAge,
            string? domain = null)
        {
            var _systemEnv = context.RequestServices.GetRequiredService<ISystemENV>();
            var secureInProd =
                _systemEnv.environment is
                    SystemENV.EnvironmentType.Production or
                    SystemENV.EnvironmentType.ProductionIIS
                || context.Request.IsHttps;

            var options = new CookieOptions
            {
                HttpOnly = httpOnly,
                Secure = secureInProd,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                MaxAge = value.SRT_String_IsNullOrEmpty() ? TimeSpan.FromSeconds(0) : maxAge,
                Expires = DateTimeOffset.UtcNow.Add(value.SRT_String_IsNullOrEmpty() ? TimeSpan.FromDays(-2) : maxAge),
            };

            // New sessions are host-only (no Domain). Parent Domain is only used when
            // expiring leftover cookies from the previous Origin-based policy.
            if (!domain.SRT_String_IsNullOrEmpty())
                options.Domain = domain;

            context.Response.Cookies.Append(name, value ?? "", options);
        }

        private static void ClearCookie(HttpContext context, string name)
        {
            var httpOnly = name != LegacyTokenCookieName && name != LegacyRoleCookieName;
            AppendCookie(context, name, "", httpOnly, TimeSpan.FromSeconds(0));

            ExpireLeftoverParentCookie(context, name, httpOnly);
        }

        private static void ExpireLeftoverParentCookie(HttpContext context, string name, bool httpOnly)
        {
            var leftoverParent = ResolveLeftoverParentDomain(context);
            if (!leftoverParent.SRT_String_IsNullOrEmpty())
                AppendCookie(context, name, "", httpOnly, TimeSpan.FromSeconds(0), leftoverParent);
        }

        /// <summary>
        /// Parent domain of the public browser host (e.g. game-line.ir), only to expire
        /// pre-fix cookies. Never docker names like CoreMatch.
        /// </summary>
        private static string? ResolveLeftoverParentDomain(HttpContext context)
        {
            var publicHost = GetPublicHostName(context);
            if (publicHost.SRT_String_IsNullOrEmpty()
                || publicHost.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                || publicHost.IndexOf('.') < 0)
                return null;

            var parent = SSO.GetDomains(publicHost);
            if (parent.SRT_String_IsNullOrEmpty()
                || parent.IndexOf('.') < 0
                || parent.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                return null;

            if (!publicHost.Equals(parent, StringComparison.OrdinalIgnoreCase)
                && !publicHost.EndsWith("." + parent, StringComparison.OrdinalIgnoreCase))
                return null;

            return parent;
        }

        private static string GetPublicHostName(HttpContext context)
        {
            var origin = context.Request.Headers.Origin.ToString();
            if (!origin.SRT_String_IsNullOrEmpty())
            {
                var fromOrigin = SSO.GetDomainsBase(origin);
                if (!fromOrigin.SRT_String_IsNullOrEmpty() && fromOrigin.IndexOf('.') >= 0)
                    return fromOrigin;
            }

            var forwarded = context.Request.Headers["X-Forwarded-Host"].FirstOrDefault();
            if (!forwarded.SRT_String_IsNullOrEmpty())
            {
                var first = forwarded.Split(',')[0].Trim();
                var fromForwarded = SSO.GetDomainsBase(first);
                if (!fromForwarded.SRT_String_IsNullOrEmpty() && fromForwarded.IndexOf('.') >= 0)
                    return fromForwarded;
            }

            var host = context.Request.Host.Host;
            if (!host.SRT_String_IsNullOrEmpty() && host.IndexOf('.') >= 0)
                return host;

            return string.Empty;
        }
    }
}
