// Ignore Spelling: Ltag env

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SRT.Core.ENV;
using SRT.Core.SystemEnums;

namespace SRT.Core.Domain
{
    public abstract class AppSetting
    {
        #region Constructors
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public AppSetting(List<string>? lstDatabaseNamespace = null)
        {
            AddResponseResult(StatusCodes.Status500InternalServerError, typeof(void));
            this.lstDatabaseNamespace = lstDatabaseNamespace ?? new List<string>();
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public AppSetting(IWebHostEnvironment _env, AppSetting _base)
        {
            CoreSystemENV = new SystemENV(_env);

            lstResponseResult = _base.lstResponseResult;
            ReturnHttpNotAcceptable = _base.ReturnHttpNotAcceptable;
            UseCashStructure = _base.UseCashStructure;
            ConnectionStrings = _base.ConnectionStrings;
            lstBuilderEnvironments = _base.lstBuilderEnvironments;
            lstAppEnvironments = _base.lstAppEnvironments;
            lstControllersOptions = _base.lstControllersOptions;
            lstDatabaseNamespace = _base.lstDatabaseNamespace;
            lstRabbitMQNamespace = _base.lstRabbitMQNamespace;
        }
        #endregion

        public void AddResponseResult(int statusCode, Type type)
        {
            lstResponseResult.Add(new Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute(statusCode: statusCode, type: type));
        }

        public List<Action<Microsoft.AspNetCore.Mvc.MvcOptions>> lstControllersOptions { get; set; } = new();
        public List<Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute> lstResponseResult { get; set; } = new();
        public bool ReturnHttpNotAcceptable { get; set; } = false;
        public bool UseErrorLogSQL { get; set; } = true;
        public bool UseSerilog { get; set; } = true;
        public CashType UseCashStructure { get; set; } = CashType.CashWithSQL;

        public string BaseURLtagName { get; set; } = "urlData";

        public AppConnectionString ConnectionStrings { get; set; }
        public SystemENV CoreSystemENV { get; set; }

        public List<Action<WebApplicationBuilder, SystemENV.EnvironmentType>> lstBuilderEnvironments { get; set; } = new();
        public List<Action<WebApplication, SystemENV.EnvironmentType>> lstAppEnvironments { get; set; } = new();

        public List<Func<IServiceCollection, URLData, IServiceCollection>> lstInjectService { get; set; } = new();

        public List<string> lstDatabaseNamespace { get; set; } = new();
        public List<string> lstRabbitMQNamespace { get; set; } = new();

        public List<string> lstInjection_WithInjectionAttribute { get; set; } = new();
    }
}
