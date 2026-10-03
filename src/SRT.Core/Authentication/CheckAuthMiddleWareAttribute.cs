// Ignore Spelling: SYS

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using SRT.Core.Domain.DomainInterfaces;
using SRT.Core.Domain.RequestsDomqins;
using SRT.Core.Domain.RequestsDomqins.HttpClientServices;
using SRT.Core.Interfaces;
using SRT.Core.SRTExtensions;
using SRT.Core.SystemEnums;
using System.ComponentModel;
using System.Net;
using SRT.Core.Diagnostics;

namespace SRT.Core.Authentication
{
    /// <summary>
    /// Middleware attribute that validates authentication (User, CoreMatch, or Internal) 
    /// and injects user data + dashboard context into HttpContext.Items.
    /// </summary>
    public class CheckAuthMiddleWareAttribute : ActionFilterAttribute
    {
        private string? baseToken { get; }
        private bool allowNotAuthenticated { get; }
        private TypeAuthenticationController ControllerType { get; }

        #region Constructors
        public CheckAuthMiddleWareAttribute(
            TypeAuthenticationController authType,
            bool allowNotAuthenticated = false,
            string? baseToken = null)
        {
            ControllerType = authType;
            this.allowNotAuthenticated = allowNotAuthenticated;
            this.baseToken = baseToken;
        }
        #endregion

        #region Handler
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthClient>();

                if (!context.HttpContext.Request.Path.StartsWithSegments("/registration/checkAuthentication") &&
                    !context.HttpContext.Request.Path.StartsWithSegments("/registration/identity") &&
                    !context.HttpContext.Request.Path.StartsWithSegments("/registration/SaveUsername") &&
                    !context.HttpContext.Request.Path.StartsWithSegments("/auth"))
                {
                    var token = GetTokenFromRequest(context);

                    await HandleAuthenticationAsync(context, authService, token);
                }

                // اجبار تغییر رمز: فقط GetInfo / ChangePassword / logout مجاز است
                if (context.Result is null && authService.IsForceChangePassword(context))
                {
                    context.Result = new ObjectResult(new ResponseData
                    {
                        status = HttpStatusCode.Forbidden,
                        message = "برای ادامه باید رمز عبور را تغییر دهید.",
                        errorCode = "MUST_CHANGE_PASSWORD"
                    })
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                    };
                    return;
                }

                await next();
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }
        #endregion

        #region Authentication Handler
        private async Task HandleAuthenticationAsync(
            ActionExecutingContext context,
            IAuthClient authService,
            string? token)
        {
            switch (ControllerType)
            {
                // `None` = any authenticated panel/user role (e.g. shared User/General).
                // Still validates the session cookie and injects UserData — it only skips
                // the role-equality check that Admin/Referee/Client endpoints enforce.
                case TypeAuthenticationController.None:
                case TypeAuthenticationController.Admin:
                case TypeAuthenticationController.RefereeAdmin:
                case TypeAuthenticationController.Referee:
                case TypeAuthenticationController.Client:
                case TypeAuthenticationController.Mci:
                    await HandleUserAuth(context, authService, token);
                    return;

                case TypeAuthenticationController.Internal:
                    HandleInternalAuth(token);
                    return;

                default:
                    throw new ArgumentOutOfRangeException(nameof(ControllerType), ControllerType, null);
            }
        }

        private async Task HandleUserAuth(ActionExecutingContext context, IAuthClient authService, string? token)
        {
            if (!token.SRT_String_IsNullOrEmpty())
            {
                try
                {
                    var result = await authService.CheckToken<BaseHttpClientService>(new GetStringFromBody() { data = token });
                    if (result.IsSuccess)
                        SetUserData(context, result);
                    else if (!allowNotAuthenticated)
                    {
                        var msg = result.APIResult_ResponseData?.message ?? "session missing";
                        throw new Exception(msg);
                    }
                }
                catch when (allowNotAuthenticated)
                {
                    // Guest-allowed endpoints: ignore invalid/expired tokens and continue unauthenticated.
                }
            }
            else if (!allowNotAuthenticated)
            {
                throw new Exception("Missing client authentication data");
            }
        }

        private void HandleInternalAuth(string? token)
        {
            var expected = ISecuritySystem.ResolveInternalToken(baseToken);
            if (token != expected)
            {
                throw new Exception("Invalid internal token");
            }
        }
        #endregion

        #region Token Creation
        private string? GetTokenFromRequest(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var host = GetHost(context);

            try
            {
                var token = GeneralFunctions.SSO.ReadToken(httpContext, ControllerType, justFromCookie: true);

                return token;
            }
            catch
            {
                // fall back with minimal token
                var token = GeneralFunctions.SSO.ReadToken(httpContext, ControllerType, justFromCookie: true);
                return token;
            }
        }
        #endregion

        #region User Injection
        private void SetUserData(ActionExecutingContext context, BaseHttpClientService apiResult)
        {
            var r = SetUserDataMain(context, apiResult);

            if (r.SRT_String_IsNullOrEmpty())
                return;

            if (!allowNotAuthenticated)
                throw new Exception(r);
        }
        private string? SetUserDataMain(ActionExecutingContext context, BaseHttpClientService apiResult)
        {
            if (!apiResult.IsSuccess)
            {
                if (!allowNotAuthenticated)
                    return apiResult.APIResult_ResponseData.message;

                return null; // success
            }

            if (apiResult.APIResult_ResponseData.data == null)
                return "Empty client data response";

            var dmUser = apiResult.APIResult_ResponseData.GetData<IUserInfo>();
            if (dmUser is null)
                return "Invalid client data response";

            // Role-specific controllers must match; `None` accepts any authenticated role.
            if (!allowNotAuthenticated
                && ControllerType != TypeAuthenticationController.None
                && !RoleMatchesController(dmUser.userRole, ControllerType))
                return InvalidRoleDataMessage;

            context.HttpContext.Items["UserData"] = dmUser;

            return null; // success
        }

        private static bool RoleMatchesController(
            UserRoleType userRole,
            TypeAuthenticationController controllerType)
        {
            return controllerType switch
            {
                TypeAuthenticationController.Admin => userRole == UserRoleType.Admin,
                TypeAuthenticationController.RefereeAdmin => userRole == UserRoleType.RefereeAdmin,
                TypeAuthenticationController.Referee => userRole == UserRoleType.Referee,
                TypeAuthenticationController.Client => userRole == UserRoleType.Client,
                TypeAuthenticationController.Mci => userRole == UserRoleType.Mci,
                _ => false,
            };
        }
        #endregion

        #region Exception Handler
        private const string InvalidRoleDataMessage = "Invalid Role Data";

        private static async Task HandleExceptionAsync(ActionExecutingContext context, Exception ex)
        {
            if (string.Equals(ex.Message, InvalidRoleDataMessage, StringComparison.Ordinal))
            {
                context.Result = new ObjectResult(new ResponseData
                {
                    status = HttpStatusCode.Forbidden,
                    message = "دسترسی به این سرویس برای نقش شما مجاز نیست.",
                    errorCode = "FORBIDDEN_ROLE"
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                };
                return;
            }

            var errorRepo = context.HttpContext.RequestServices.GetService<IErrorLogger>()!;
            var errorId = await errorRepo.Save_Error_ToDB(ex);

            // 🟢 اضافه کردن کوکی در صورت عدم احراز هویت
            context.HttpContext.Response.Cookies.Append(
                "authStatus",
                "not Authentication",
                new CookieOptions
                {
                    HttpOnly = false,
                    Secure = context.HttpContext.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddMinutes(5)
                });

            context.Result = new ObjectResult(new ResponseData
            {
                status = HttpStatusCode.Unauthorized,
                message = $"Error in Auth Middleware => ID: {errorId}"
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized,
            };
        }
        #endregion

        private static bool IsForcedPasswordChangeRole(UserRoleType role)
            => role == UserRoleType.Admin
                || role == UserRoleType.RefereeAdmin
                || role == UserRoleType.Referee;

        private static bool IsAllowedDuringForcedPasswordChange(HttpRequest request)
        {
            var path = request.Path.Value ?? string.Empty;
            if (path.EndsWith("/User/General/GetInfo", StringComparison.OrdinalIgnoreCase)
                && HttpMethods.IsGet(request.Method))
                return true;
            if (path.EndsWith("/User/General/ChangePassword", StringComparison.OrdinalIgnoreCase)
                && HttpMethods.IsPost(request.Method))
                return true;
            if (path.EndsWith("/User/General/logout", StringComparison.OrdinalIgnoreCase)
                && HttpMethods.IsDelete(request.Method))
                return true;
            return false;
        }

        #region Get Host
        private string GetHost(ActionExecutingContext context)
        {
            var v = GeneralFunctions.SSO.GetHostFromContext(context.HttpContext.Request);

#if DEBUG
            if (v.Contains("localhost"))
                v = "http://test.com";
#endif

            return v.Replace("http://", "").Replace("https://", "");
        }
        #endregion
    }
}
