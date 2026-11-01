// Ignore Spelling: SRT

using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SRT.Core.Authentication;
using SRT.Core.CustomAttributes;
using SRT.Core.Domain;
using SRT.Core.Domain.Diagnostics;
using SRT.Core.Domain.RequestsDomqins;
using SRT.Core.ENV;
using SRT.Core.Interfaces;
using SRT.Core.Json;
using SRT.Core.ServiceStatus;
using SRT.Core.SYS_Controllers;
using SRT.Core.SystemEnums;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SRT.Core
{
    public static class BaseConfigurations
    {
        public static (WebApplicationBuilder, TAppSetting) SRT_ConfigController<TAppSetting>(
            string[] args,
            TAppSetting config,
            Func<IServiceCollection, URLData, IServiceCollection> AddAuthentication)
            where TAppSetting : AppSetting
        {
            Console.WriteLine("Start Configurations (SRT.Core) ..................... ");
            Console.WriteLine("");

            string environment = SystemENV.FixEnvironmentName(args.Length == 0 ? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development" : args[0]);

            Console.Title = config.ServiceName + "-" + environment;

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environment });

            config.CoreSystemENV = new SystemENV(builder.Environment);

            var dmConnectionString = builder.SRT_GetConnectionString();
            config.ConnectionStrings = dmConnectionString;
            BindServiceIdentity(builder.Configuration, config);

            // Controllers without [ApiVersion] stay on the default version, so callers that send no api-version keep working.
            builder.Services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
            })
            .AddMvc()
            .AddApiExplorer();

            config.lstInjectService.Add(AddAuthentication);

            return (builder, config);
        }

        public static async Task<WebApplication> SRT_ConfigBuilderAndBuildApp(WebApplicationBuilder builder, AppSetting configApp)
        {
            var loggerBuilder = builder.SRT_GetLogger();
            var report = configApp.StartupReport;
            var environmentName = configApp.CoreSystemENV.environment.ToString();
            loggerBuilder.LogDebug("Environment -> {Environment}", environmentName);

            builder.Services.AddControllers(option =>
            {
                option.ReturnHttpNotAcceptable = configApp.ReturnHttpNotAcceptable;
                configApp.lstResponseResult.ForEach(filter => option.Filters.Add(filter));
                configApp.lstControllersOptions.ForEach(item => item.Invoke(option));
            })
            .AddJsonOptions(json =>
            {
                json.JsonSerializerOptions.Converters.Add(new LocalDateTimeJsonConverter());
            });

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var message = string.Join(" ", context.ModelState
                        .Where(entry => entry.Value is not null && entry.Value.Errors.Count > 0)
                        .SelectMany(entry => entry.Value!.Errors.Select(error =>
                            string.IsNullOrWhiteSpace(error.ErrorMessage)
                                ? error.Exception?.Message
                                : error.ErrorMessage))
                        .Where(text => !string.IsNullOrWhiteSpace(text)));

                    return SRTBaseControllerSystem.CreateApiResponse(
                        HttpStatusCode.BadRequest,
                        string.IsNullOrWhiteSpace(message) ? "Invalid model state." : message);
                };
            });

            builder.Services.AddEndpointsApiExplorer();

            var _urlData = new URLData();
            builder.Configuration.GetSection(configApp.BaseURLtagName).Bind(_urlData);
            builder.Services.AddSingleton(_urlData);
            if (configApp.lstInjectService != null && configApp.lstInjectService.Count > 0)
            {
                foreach (var item in configApp.lstInjectService)
                    item.Invoke(builder.Services, _urlData);
            }

            builder.Services.Configure<ServiceAuthOptions>(builder.Configuration.GetSection(ServiceAuthOptions.SectionName));
            builder.Services.TryAddSingleton<IServiceTokenClock, SystemServiceTokenClock>();
            builder.Services.TryAddSingleton<IServiceJwtKeySource, HttpServiceJwtKeySource>();
            builder.Services.AddSingleton<ServiceJwtKeyCache>();
            builder.Services.AddHostedService<ServiceJwtKeyWarmup>();
            builder.Services.TryAddSingleton<ServiceTokenMiddleWare>();

            List<ISRTBasicConfigurations> plugins;
            try
            {
                plugins = SrtPluginLoader.Load(loggerBuilder);
                report.Info("plugins", string.Join(", ", plugins.Select(p => p.FeatureName)));
                ValidateRequestedFeatures(configApp, plugins, loggerBuilder);
            }
            catch (Exception ex)
            {
                report.Failed("plugins", ex.Message);
                report.Write(builder.SRT_GetLogger(SRTStartupReport.LoggerCategory), configApp.ServiceName, environmentName, failure: ex);
                throw;
            }

            foreach (var plugin in plugins)
            {
                try
                {
                    builder = await plugin.SRT_ConfigBuilder(builder, configApp);
                }
                catch (Exception ex)
                {
                    report.Failed(plugin.FeatureName.ToLowerInvariant(), "builder config failed: " + ex.Message);
                    report.Write(builder.SRT_GetLogger(SRTStartupReport.LoggerCategory), configApp.ServiceName, environmentName, failure: ex);
                    throw;
                }

                loggerBuilder.LogDebug("Builder - External Config {Module} ({Feature}) - OK", plugin.GetType().Name, plugin.FeatureName);
            }

            report.Set("grpc", SRTStartupReport.Status.Off, IsGrpcLoaded() ? "Grpc.AspNetCore.Server is loaded but not reported by any SRT plugin" : "not used");

            configApp.lstBuilderEnvironments.ForEach(q => q.Invoke(builder, configApp.CoreSystemENV.environment));

            builder.AddInjection_WithInjectionAttribute(configApp);

            builder.Services.AddSingleton(configApp);
            builder.Services.AddSingleton<ISystemENV>(configApp.CoreSystemENV);
            builder.Services.AddSingleton(configApp.ConnectionStrings);
            builder.Services.AddServiceStatus();
            builder.Services.AddSingleton(report);
            loggerBuilder.LogDebug("Builder - Add Injection SQLConnection | CoreSystemENV - OK");

            var app = builder.Build();

            app.UseServiceStatus();
            app.UseMiddleware<HttpOnlyMiddleWareAttribute>();

            var loggerApp = app.SRT_GetLogger();

            configApp.lstAppEnvironments.ForEach(q => q.Invoke(app, configApp.CoreSystemENV.environment));

            var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(SRTStartupReport.LoggerCategory);
            foreach (var plugin in plugins)
            {
                try
                {
                    app = await plugin.SRT_ConfigApp(app, configApp);
                }
                catch (Exception ex)
                {
                    var section = plugin.FeatureName.ToLowerInvariant();
                    if (!report.Entries.Any(e => e.Status == SRTStartupReport.Status.Failed))
                        report.Failed(section, "app config failed: " + ex.Message);
                    report.Write(startupLogger, configApp.ServiceName, environmentName, failure: ex);
                    throw;
                }

                loggerApp.LogDebug("APP - External Config {Module} ({Feature}) - OK", plugin.GetType().Name, plugin.FeatureName);
            }

            // One summary line per start. ApplicationStarted fires after Kestrel binds, so the real URLs are known.
            var lifetimeApp = app;
            app.Lifetime.ApplicationStarted.Register(() =>
            {
                try
                {
                    // Replaces the Microsoft.Hosting.Lifetime lines (listening on / environment / content root), now filtered to Warning.
                    report.Info("host", $"contentRoot={lifetimeApp.Environment.ContentRootPath} pid={Environment.ProcessId}");
                    report.Write(startupLogger, configApp.ServiceName, environmentName, string.Join(", ", lifetimeApp.Urls));
                }
                catch
                {
                    // The summary must never stop the service.
                }
            });

            //app.UseAuthorization();
            app.MapControllers();
            return app;
        }

        private static void ValidateRequestedFeatures(
            AppSetting configApp,
            List<ISRTBasicConfigurations> plugins,
            ILogger logger)
        {
            var discovered = plugins
                .Select(p => p.FeatureName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var feature in configApp.GetRequestedFeatures())
            {
                if (!discovered.Contains(feature))
                {
                    throw new InvalidOperationException(
                        $"SRT feature '{feature}' is enabled but its plugin was not loaded. " +
                        $"Add a ProjectReference/PackageReference to the matching SRT.Core package and ensure the assembly is copied to the output directory.");
                }
            }

            ThrowIfIncomplete(configApp, SRTFeatureNames.SQLServer, () =>
            {
                if (configApp.IsErrorLogSqlStore() && !HasHost(configApp.ConnectionStrings?.SRTCore_ErrorSQL))
                    return "UseErrorLogSQL is true but ConnectionDB:SRTCore_ErrorSQL.host is empty.";
                if (configApp.IsCacheSqlStore()
                    && !configApp.AllowCacheSqlFallback
                    && !HasHost(configApp.ConnectionStrings?.SRTCore_CacheSQL))
                    return "UseCashStructure is CashWithSQL but ConnectionDB:SRTCore_CacheSQL.host is empty.";
                if (configApp.IsSqlDatabaseRequested() && !HasHost(configApp.ConnectionStrings?.DefaultConnection))
                    return "SQL database namespaces are set but ConnectionDB:DefaultConnection.host is empty.";
                return null;
            });

            ThrowIfIncomplete(configApp, SRTFeatureNames.PostgreSQL, () =>
            {
                if (configApp.IsErrorLogPostgresStore() && !HasHost(configApp.ConnectionStrings?.SRTCore_ErrorSQL))
                    return "UseErrorLogSQL is true but ConnectionDB:SRTCore_ErrorSQL.host is empty.";
                if (configApp.IsCachePostgresStore()
                    && !configApp.AllowCacheSqlFallback
                    && !HasHost(configApp.ConnectionStrings?.SRTCore_CacheSQL))
                    return "UseCashStructure is CashWithSQL but ConnectionDB:SRTCore_CacheSQL.host is empty.";
                if (configApp.IsPostgresDatabaseRequested() && !HasHost(configApp.ConnectionStrings?.DefaultConnection))
                    return "PostgreSQL database namespaces are set but ConnectionDB:DefaultConnection.host is empty.";
                return null;
            });

            ThrowIfIncomplete(configApp, SRTFeatureNames.Redis, () =>
            {
                if (configApp.UseCashStructure != CashType.None && !HasHost(configApp.ConnectionStrings?.SRTCore_CacheRedis))
                    return "Cache is enabled but ConnectionDB:SRTCore_CacheRedis.host is empty.";
                return null;
            });

            ThrowIfIncomplete(configApp, SRTFeatureNames.RabbitMQ, () =>
            {
                if (configApp.lstRabbitMQNamespace.Count > 0 && !HasHost(configApp.ConnectionStrings?.SRTCore_RabbitMQ))
                    return "RabbitMQ namespaces are set but ConnectionDB:SRTCore_RabbitMQ.host is empty.";
                return null;
            });

            logger.LogDebug("Plugin validation OK. Discovered: {Plugins}", string.Join(", ", discovered));
        }

        private static void ThrowIfIncomplete(AppSetting configApp, string feature, Func<string?> reason)
        {
            if (!configApp.GetRequestedFeatures().Contains(feature))
                return;
            var message = reason();
            if (!string.IsNullOrWhiteSpace(message))
                throw new InvalidOperationException($"SRT feature '{feature}' is enabled with incomplete settings: {message}");
        }

        private static bool IsGrpcLoaded() =>
            AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "Grpc.AspNetCore.Server");

        internal static bool HasHost(GeneralConnectionString? connection) =>
            connection != null && !string.IsNullOrWhiteSpace(connection.host);

        private static void AddInjection_WithInjectionAttribute(
            this WebApplicationBuilder builder,
            AppSetting appSetting)
        {
            foreach (var targetNamespace in appSetting.lstInjection_WithInjectionAttribute)
            {
                var types = AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); }
                        catch (ReflectionTypeLoadException ex)
                        { return ex.Types.Where(t => t != null)!; }
                    })
                    .Where(t => t is { IsClass: true, IsAbstract: false }
                                && t.Namespace?.Contains(targetNamespace) == true
                                && t.GetCustomAttribute<InjectionControllerAttribute>() != null)
                    .ToList();

                foreach (var implementation in types)
                {
                    if (implementation is null)
                        continue;

                    var attrs = implementation.GetCustomAttributes<InjectionControllerAttribute>();
                    foreach (var attr in attrs)
                    {
                        foreach (var serviceType in attr.ServiceTypes)
                        {
                            if (!serviceType.IsAssignableFrom(implementation))
                                throw new InvalidOperationException(
                                    $"Class {implementation.Name} does not implement {serviceType.Name}");

                            switch (attr.Lifetime)
                            {
                                case InjectionLifetime.Scoped:
                                    builder.Services.AddScoped(serviceType, implementation);
                                    break;
                                case InjectionLifetime.Singleton:
                                    builder.Services.AddSingleton(serviceType, implementation);
                                    break;
                                case InjectionLifetime.Transient:
                                    builder.Services.AddTransient(serviceType, implementation);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private static AppConnectionString SRT_GetConnectionString(this WebApplicationBuilder builder)
        {
            var sqlConnection = new AppConnectionString();
            builder.Configuration.GetSection("ConnectionDB").Bind(sqlConnection);
            sqlConnection.SRTCore_ErrorSQL.InheritConnectionTypeFrom(sqlConnection.DefaultConnection);
            sqlConnection.SRTCore_CacheSQL.InheritConnectionTypeFrom(sqlConnection.DefaultConnection);

            LogConnection(builder, sqlConnection.DefaultConnection);
            LogConnection(builder, sqlConnection.DefaultConnection2);
            LogConnection(builder, sqlConnection.SRTCore_ErrorSQL);
            LogConnection(builder, sqlConnection.SRTCore_RabbitMQ);
            LogConnection(builder, sqlConnection.SRTCore_CacheSQL);
            LogConnection(builder, sqlConnection.SRTCore_CacheRedis);

            return sqlConnection;
        }

        private static void BindServiceIdentity(IConfiguration configuration, AppSetting config)
        {
            config.ServiceId = configuration.GetValue<int?>("Service:Id")
                ?? configuration.GetValue<int?>("ServiceId")
                ?? config.ServiceId;
            config.ServiceName = configuration["Service:Name"]
                ?? configuration["ServiceName"]
                ?? config.ServiceName;
        }

        private static void LogConnection(
            WebApplicationBuilder builder,
            GeneralConnectionString connection,
            [CallerArgumentExpression("connection")] string instanceName = "")
        {
            if (connection == null) return;
            var name = instanceName.Contains('.') ? instanceName.Split('.')[1] : instanceName;
            var logger = builder.SRT_GetLogger(name);
            logger.LogDebug("Connection {Name}: {Summary}", name, connection.GetSafeSummary());
        }

        public static ILogger SRT_GetLogger(this WebApplicationBuilder builder, string title = "configure Application")
        {
            var loggerFactory = builder.Services.BuildServiceProvider()
                                                .GetRequiredService<ILoggerFactory>();
            return loggerFactory.CreateLogger(title);
        }

        public static ILogger SRT_GetLogger(this WebApplication app, string title = "configure Application")
        {
            var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            return loggerFactory.CreateLogger(title);
        }
    }
}
