using Microsoft.AspNetCore.Mvc.Filters;
using SRT.Core.Domain.DomainInterfaces;
using SRT.Core.Domain.RequestsDomqins;
using SRT.Core.Domain.RequestsDomqins.HttpClientServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace SRT.Core.Interfaces
{
    public partial interface IAuthClient
    {
        Task<TResponse> CheckToken<TResponse>(GetStringFromBody token)
            where TResponse : BaseHttpClientService;

        Task<TResponse> RegisterNew<TResponse>(object data)
            where TResponse : BaseHttpClientService;

        Task<TResponse> Logout<TResponse>(GetStringFromBody tokenCheck)
            where TResponse : BaseHttpClientService;

        // اجبار تغییر رمز: فقط GetInfo / ChangePassword / logout مجاز است
        bool IsForceChangePassword(ActionExecutingContext context);
        //        && context.HttpContext.Items["UserData"] is UserInfo lockedUser
        //&& lockedUser.mustChangePassword
        //&& IsForcedPasswordChangeRole(lockedUser.userRole)
        //&& !IsAllowedDuringForcedPasswordChange(context.HttpContext.Request))
    }
}
