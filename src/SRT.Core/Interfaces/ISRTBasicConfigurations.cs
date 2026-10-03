using Microsoft.AspNetCore.Builder;
using SRT.Core.Domain;

namespace SRT.Core.Interfaces
{
    public interface ISRTBasicConfigurations
    {
        int Order { get; }
        string FeatureName { get; }

        Task<WebApplicationBuilder> SRT_ConfigBuilder(WebApplicationBuilder builder, AppSetting configApp);
        Task<WebApplication> SRT_ConfigApp(WebApplication app, AppSetting configApp);
    }
}
