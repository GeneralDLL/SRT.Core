// Ignore Spelling: DTO SRT CoreMatch

using SRT.Core.ENV;
using SRT.Core.SRTExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace SRT.Core.GeneralFunctions
{
    public class SSO
    {
        public static string GetHeaderData(HttpContext httpContext, string nameHeader)
        {
            httpContext.Request.Headers.TryGetValue(nameHeader, out var v);
            return v.Any() ? v.ToString() : string.Empty;
        }

        /// <summary>
        /// Sets the HttpOnly session cookie. Prefer SessionCookieService.SetSession for new code.
        /// When HttpOnly is false / name is TOKEN — legacy path that now clears TOKEN instead of writing secrets.
        /// </summary>
        public static void SetHttpOnlyToken(HttpContext context, string token, bool HttpOnly = true, string name = "AuthToken")
        {
            if (string.Equals(name, SessionCookieService.LegacyTokenCookieName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, SessionCookieService.LegacyRoleCookieName, StringComparison.OrdinalIgnoreCase))
            {
                // Do not re-issue JS-readable session cookies.
                if (token.SRT_String_IsNullOrEmpty())
                    SessionCookieService.ClearSession(context);
                return;
            }

            if (string.Equals(name, SessionCookieService.RegistrationTicketCookieName, StringComparison.OrdinalIgnoreCase))
            {
                if (token.SRT_String_IsNullOrEmpty())
                    SessionCookieService.ClearRegistrationTicket(context);
                else
                    SessionCookieService.SetRegistrationTicket(context, token);
                return;
            }

            // Default AuthToken session cookie
            if (token.SRT_String_IsNullOrEmpty())
                SessionCookieService.ClearSession(context);
            else if (Guid.TryParse(token, out _))
                SessionCookieService.SetRegistrationTicket(context, token);
            else
                SessionCookieService.SetSession(context, token);
        }

        /// <summary>
        /// http://sub.domain.com:8080/path?query=string  => domain.com
        /// </summary>
        public static string GetDomains(string url)
        {
            var domain = GetDomainsBase(url);

            if (domain.Count(q => q == '.') != 1)
                return domain.Substring(domain.IndexOf('.') + 1);

            return domain;
        }

        /// <summary>
        /// http://sub.domain.com:8080/path?query=string  => sub.domain.com
        /// </summary>
        public static string GetDomainsBase(string url)
        {
            string pattern = @"^(?:https?:\/\/)?([^\/?:]+)(?::\d+)?(?:[\/?].*)?$";
            Match match = Regex.Match(url, pattern);

            if (match.Success)
                return match.Groups[1].Value;

            pattern = @"^(?:http?:\/\/)?([^\/?:]+)(?::\d+)?(?:[\/?].*)?$";
            match = Regex.Match(url, pattern);

            if (match.Success)
                return match.Groups[1].Value;

            return string.Empty;
        }

        public static string ReadToken(
            HttpContext context,
            bool isInternal = false,
            bool justFromCookie = false)
        {
            if (isInternal)
            {
                if (context.Request.Headers.TryGetValue("Authorization", out var headerValue))
                {
                    var parts = headerValue.ToString().Split(' ');
                    return parts.Length == 2 ? parts[1] : "";
                }
                return "";
            }

            // User/Admin APIs: cookie-only when justFromCookie (default middleware path).
            // Bearer is only consulted when justFromCookie is false (legacy callers).
            var preferBearer = !justFromCookie;
            return SessionCookieService.ReadSession(context, preferBearer: preferBearer) ?? "";
        }

        public static string GetHostFromContext(HttpRequest request)
        {
            var v = request.Headers["X-Forwarded-Host"].FirstOrDefault()
                ?? request.Host.ToString();

            if (v.Split('.').Length == 1)
                v = request.Headers["requestFrom"].FirstOrDefault();

            if (v.SRT_String_IsNullOrEmpty() || v!.Contains("localhost"))
                v = "https://CoreMatch-test.servicebarkhat.ir";

            return v;
        }
    }
}
