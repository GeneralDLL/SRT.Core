// Ignore Spelling: SRT

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SRT.Core.Authentication;
using SRT.Core.CustomAttributes;
using SRT.Core.Domain;
using SRT.Core.ENV;
using SRT.Core.Interfaces;
using SRT.Core.Json;
using SRT.Core.ServiceStatus;
using SRT.Core.SystemEnums;
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
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environment });

            config.CoreSystemENV = new SystemENV(builder.Environment);

            var dmConnectionString = builder.SRT_GetConnectionString();
            config.ConnectionStrings = dmConnectionString;
            BindServiceIdentity(builder.Configuration, config);

            builder.Services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
            });

            config.lstInjectService.Add(AddAuthentication);

            return (builder, config);
        }

        public static async Task<WebApplication> SRT_ConfigBuilderAndBuildApp(WebApplicationBuilder builder, AppSetting configApp)
        {
            var loggerBuilder = builder.SRT_GetLogger();
            loggerBuilder.LogWarning("Environment -> {Environment}", configApp.CoreSystemENV.environment);

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

            builder.Services.AddEndpointsApiExplorer();

            if (configApp.lstInjectService != null && configApp.lstInjectService.Count > 0)
            {
                var _urlData = new URLData();
                builder.Configuration.GetSection(configApp.BaseURLtagName).Bind(_urlData);
                foreach (var item in configApp.lstInjectService)
                    item.Invoke(builder.Services, _urlData);
            }

            var plugins = SrtPluginLoader.Load(loggerBuilder);
            ValidateRequestedFeatures(configApp, plugins, loggerBuilder);

            foreach (var plugin in plugins)
            {
                builder = await plugin.SRT_ConfigBuilder(builder, configApp);
                loggerBuilder.LogWarning("Builder - External Config {Module} ({Feature}) - OK", plugin.GetType().Name, plugin.FeatureName);
            }

            configApp.lstBuilderEnvironments.ForEach(q => q.Invoke(builder, configApp.CoreSystemENV.environment));

            builder.AddInjection_WithInjectionAttribute(configApp);

            builder.Services.AddSingleton(configApp);
            builder.Services.AddSingleton<ISystemENV>(configApp.CoreSystemENV);
            builder.Services.AddSingleton(configApp.ConnectionStrings);
            builder.Services.AddServiceStatus();
            loggerBuilder.LogWarning("Builder - Add Injection SQLConnection | CoreSystemENV - OK");

            var app = builder.Build();

            app.UseServiceStatus();
            app.UseMiddleware<HttpOnlyMiddleWareAttribute>();

            var loggerApp = app.SRT_GetLogger();

            configApp.lstAppEnvironments.ForEach(q => q.Invoke(app, configApp.CoreSystemENV.environment));

            foreach (var plugin in plugins)
            {
                app = await plugin.SRT_ConfigApp(app, configApp);
                loggerApp.LogWarning("APP - External Config {Module} ({Feature}) - OK", plugin.GetType().Name, plugin.FeatureName);
            }

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

            logger.LogWarning("Plugin validation OK. Discovered: {Plugins}", string.Join(", ", discovered));
        }

        private static void ThrowIfIncomplete(AppSetting configApp, string feature, Func<string?> reason)
        {
            if (!configApp.GetRequestedFeatures().Contains(feature))
                return;
            var message = reason();
            if (!string.IsNullOrWhiteSpace(message))
                throw new InvalidOperationException($"SRT feature '{feature}' is enabled with incomplete settings: {message}");
        }

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

            Console.WriteLine("\n\n******************************* Connection Strings ****************************");
            LogConnection(builder, sqlConnection.DefaultConnection);
            LogConnection(builder, sqlConnection.DefaultConnection2);
            LogConnection(builder, sqlConnection.SRTCore_ErrorSQL);
            LogConnection(builder, sqlConnection.SRTCore_RabbitMQ);
            LogConnection(builder, sqlConnection.SRTCore_CacheSQL);
            LogConnection(builder, sqlConnection.SRTCore_CacheRedis);
            Console.WriteLine("*******************************************************************************\n\n");

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
            logger.LogWarning("Connection {Name}: {Summary}", name, connection.GetSafeSummary());
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
