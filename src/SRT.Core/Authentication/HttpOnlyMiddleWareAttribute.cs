// Ignore Spelling: SRT Auth sys

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SRT.Core.Authentication
{
    public class HttpOnlyMiddleWareAttribute
    {
        private readonly RequestDelegate _next;

        #region Constructors
        public HttpOnlyMiddleWareAttribute(RequestDelegate next)
        {
            _next = next;
        }
        #endregion

        #region Handler
        public async Task InvokeAsync(HttpContext context)
        {
            var token = ReadToken(context);

            await _next(context);

            ControlHTTPOnlyToken(context, token);
        }
        #endregion

        #region Control HTTP Only Token

        private void ControlHTTPOnlyToken(
            HttpContext context,
            string tokenForCheck)
        {
            try
            {
                var action = context.Request.RouteValues["action"]?.ToString();

                // Login / registration already set cookies via SessionCookieService.
                if (context.Response.Headers.Any(q => q.Key == "SetAuthToken" && q.Value.ToString() == "true"))
                    return;

                var isLogout = string.Equals(action, "logout", StringComparison.OrdinalIgnoreCase);

                if (isLogout)
                {
                    GeneralFunctions.SessionCookieService.ClearSession(context);
                    GeneralFunctions.SessionCookieService.ClearRegistrationTicket(context);
                    return;
                }

                // Never ClearSession on 401. A parallel GetInfo/CheckPlatformId with a leftover
                // parent-domain AuthToken would wipe the host-only cookie just set by login.
                // Stale cookies are replaced on the next SetSession / logout.

                // Do not blindly re-append cookies on every response (was causing races / overwrites).
            }
            catch
            { }
        }

        #endregion

        #region Create Auth Token Model
        [NonAction]
        public static string ReadToken(HttpContext context)
        {
            return GeneralFunctions.SessionCookieService.ReadSession(context, preferBearer: false)
                ?? GeneralFunctions.SessionCookieService.ReadRegistrationTicket(context)
                ?? "";
        }
        #endregion
    }
}
