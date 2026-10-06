using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SRT.Core.Diagnostics;
using SRT.Core.Domain;
using SRT.Core.Domain.DomainInterfaces;
using SRT.Core.Domain.RequestsDomqins;
using SRT.Core.ENV;
using SRT.Core.GeneralFunctions;

namespace SRT.Core.SYS_Controllers
{
    [ApiController]
    public abstract class SRTBaseControllerSystem : ControllerBase, IActionFilter
    {
        public const string UserDataItemKey = "UserData";
        public const string DeviceIdentityHeaderName = "X-Device-Identity";
        public const string RoleIdentityHeaderName = "X-Role-Identity";

        protected ILogger Logger =>
            HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());

        protected IConfiguration SRT_Base_AppConfiguration =>
            HttpContext.RequestServices.GetRequiredService<IConfiguration>();

        protected AppSetting SRT_Base_AppSetting =>
            HttpContext.RequestServices.GetRequiredService<AppSetting>();

        protected AppConnectionString SRT_Base_AppConnectionString =>
            HttpContext.RequestServices.GetRequiredService<AppConnectionString>();

        protected ISystemENV SRT_Base_SystemENV =>
            HttpContext.RequestServices.GetRequiredService<ISystemENV>();

        protected IErrorLogger? SRT_Base_ErrorLogger =>
            HttpContext.RequestServices.GetService<IErrorLogger>();

        protected string headerBrowser = string.Empty;
        protected string headerBrowserID = string.Empty;
        protected Guid headerUserRoleGUID = Guid.Empty;

        [NonAction]
        public void OnActionExecuting(ActionExecutingContext context)
        {
            ReadDeviceIdentityHeader();
            ReadRoleIdentityHeader();
        }

        [NonAction]
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        private void ReadDeviceIdentityHeader()
        {
            var header = SSO.GetHeaderData(HttpContext, DeviceIdentityHeaderName);
            if (string.IsNullOrWhiteSpace(header))
                return;

            try
            {
                using var document = JsonDocument.Parse(header);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    Logger.LogWarning("Header {Header} is not a JSON object.", DeviceIdentityHeaderName);
                    return;
                }

                if (document.RootElement.TryGetProperty("browser", out var browser))
                    headerBrowser = browser.ToString();
                if (document.RootElement.TryGetProperty("browserID", out var browserId))
                    headerBrowserID = browserId.ToString();
            }
            catch (JsonException ex)
            {
                Logger.LogWarning(ex, "Header {Header} is not valid JSON.", DeviceIdentityHeaderName);
            }
        }

        private void ReadRoleIdentityHeader()
        {
            if (!Request.Headers.TryGetValue(RoleIdentityHeaderName, out var values))
                return;

            var raw = values.ToString();
            if (string.IsNullOrWhiteSpace(raw))
                return;

            if (!Guid.TryParse(raw, out var roleId))
            {
                Logger.LogWarning("Header {Header} is not a valid GUID.", RoleIdentityHeaderName);
                return;
            }

            headerUserRoleGUID = roleId;
        }

        [NonAction]
        public virtual async Task<IActionResult> Response_GenerateResult(ResponseData data)
        {
            return data.status switch
            {
                HttpStatusCode.OK => Response_OK(data.message, data.data, data.redirectURL),
                HttpStatusCode.NotFound => Response_Error_NotFoundInputError(data.message, data.data, data.redirectURL),
                HttpStatusCode.BadRequest => await Response_Error_InputParameterError(data.message, data.data, url: data.redirectURL),
                HttpStatusCode.Unauthorized => Response_Error_Unauthorized(data.message, data.data),
                HttpStatusCode.Conflict => Response_Error_ExistedDataBefore(data.message, data.data),
                HttpStatusCode.TooManyRequests => Response_Error_TooManyRequests(data.message, data.data),
                HttpStatusCode.ServiceUnavailable => Response_Error_ServiceUnavailable(data.message, data.data),
                HttpStatusCode.GatewayTimeout => Response_Error_GatewayTimeout(data.message, data.data),
                HttpStatusCode.InternalServerError => await Response_Error_ControlInternalServerError(
                    data.message,
                    new InvalidOperationException(data.message ?? data.status.ToString())),
                _ => throw new InvalidOperationException($"Unsupported ResponseData.status: {data.status}"),
            };
        }

        [NonAction]
        public virtual IActionResult Response_Error_Unauthorized(string? message = null, object? data = null)
            => CreateResponse(HttpStatusCode.Unauthorized, message, data);

        [NonAction]
        public virtual IActionResult Response_Error_NotFoundInputError(string? message = null, object? data = null, string? url = null)
            => CreateResponse(HttpStatusCode.NotFound, message, data, redirectUrl: url);

        [NonAction]
        public virtual IActionResult Response_Error_BadRequest(string? message = null, object? data = null)
            => CreateResponse(HttpStatusCode.BadRequest, message, data);

        [NonAction]
        public virtual async Task<IActionResult> Response_Error_InputParameterError(
            string? message = null,
            object? data = null,
            Exception? error = null,
            string? url = null)
        {
            var code = error is null ? "" : await SaveLogError(error);
            return CreateResponse(HttpStatusCode.BadRequest, message, data, errorCode: code, redirectUrl: url);
        }

        [NonAction]
        public virtual IActionResult Response_OK(string? message = null, object? data = null, string? url = null)
            => CreateResponse(HttpStatusCode.OK, message, data, redirectUrl: url);

        [NonAction]
        public virtual IActionResult Response_NoContent()
            => NoContent();

        [NonAction]
        public virtual IActionResult Response_Error_TooManyRequests(string? message = null, object? data = null)
            => CreateResponse(HttpStatusCode.TooManyRequests, message, data);

        [NonAction]
        public virtual IActionResult Response_Error_ServiceUnavailable(string? message = null, object? data = null)
            => CreateResponse(HttpStatusCode.ServiceUnavailable, message, data);

        [NonAction]
        public virtual IActionResult Response_Error_GatewayTimeout(string? message = null, object? data = null)
            => CreateResponse(HttpStatusCode.GatewayTimeout, message, data);

        [NonAction]
        public virtual IActionResult Response_Error_FinishedProcessBefore(string? message = null, object? data = null)
            => Response_Error_ExistedDataBefore(message, data);

        [NonAction]
        public virtual IActionResult Response_Error_FoundDataBefore(string? message = null, object? data = null)
            => Response_Error_ExistedDataBefore(message, data);

        [NonAction]
        public virtual IActionResult Response_Error_ExistedDataBefore(string? message = null, object? data = null)
            => CreateResponse(HttpStatusCode.Conflict, message, data);

        [NonAction]
        public virtual IActionResult Response_Error_NotResponseFromChild(string? message = null, object? data = null)
            => CreateResponse(HttpStatusCode.BadRequest, message, data);

        [NonAction]
        public virtual IActionResult Response_Error_NotResponseFromChild(string? message, ResponseData responseData)
            => CreateResponse(HttpStatusCode.BadRequest, message, responseData.data, errorCode: responseData.errorCode);

        [NonAction]
        public async Task<IActionResult> Response_Error_ControlInternalServerError(string? message, Exception ex)
        {
            var codeError = await SaveLogError(ex);
            return CreateResponse(HttpStatusCode.InternalServerError, message, data: null, errorCode: codeError);
        }

        [NonAction]
        protected async Task<string> SaveLogError(Exception ex)
        {
            var errorLogger = SRT_Base_ErrorLogger;
            if (errorLogger is null)
            {
                Logger.LogError(ex, "Unhandled controller error (IErrorLogger is not registered).");
                return "Error Log => IErrorLogger not registered";
            }

            var id = await errorLogger.Save_Error_ToDB(ex);
            var text = "Error Log => ID: " + id;
            Logger.LogError(ex, "{ErrorLog}", text);
            return text;
        }

        [NonAction]
        public static ObjectResult CreateApiResponse(
            HttpStatusCode status,
            string? message = null,
            object? data = null,
            string? errorCode = null,
            string? redirectUrl = null)
        {
            return new ObjectResult(new ResponseData
            {
                status = status,
                data = data,
                message = message,
                errorCode = errorCode,
                redirectURL = redirectUrl,
            })
            {
                StatusCode = (int)status,
            };
        }

        [NonAction]
        protected IActionResult CreateResponse(
            HttpStatusCode status,
            string? message = null,
            object? data = null,
            string? errorCode = null,
            string? redirectUrl = null)
            => CreateApiResponse(status, message, data, errorCode, redirectUrl);

        [NonAction]
        protected virtual IUserInfo? GetUserInfo()
        {
            if (!HttpContext.Items.TryGetValue(UserDataItemKey, out var userData) || userData is null)
                return null;

            return userData as IUserInfo;
        }

        protected virtual IUserInfo? dmUser => GetUserInfo();
    }
}
