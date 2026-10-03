using Microsoft.Extensions.Logging;
using SRT.Core.Interfaces;
using System.Reflection;
using System.Runtime.Loader;

namespace SRT.Core
{
    internal static class SrtPluginLoader
    {
        public static List<ISRTBasicConfigurations> Load(ILogger logger)
        {
            LoadCandidateAssemblies(logger);

            var plugins = new List<ISRTBasicConfigurations>();
            var seenTypes = new HashSet<string>(StringComparer.Ordinal);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    foreach (var loader in ex.LoaderExceptions)
                    {
                        if (loader != null)
                            logger.LogError(loader, "Plugin type load failed in assembly {Assembly}", assembly.GetName().Name);
                    }

                    types = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
                }

                foreach (var type in types)
                {
                    if (type is not { IsClass: true, IsAbstract: false })
                        continue;
                    if (!typeof(ISRTBasicConfigurations).IsAssignableFrom(type))
                        continue;
                    if (type.GetConstructor(Type.EmptyTypes) is null)
                    {
                        logger.LogError("Plugin {Type} skipped: parameterless constructor is required", type.FullName);
                        continue;
                    }

                    var key = type.FullName ?? type.Name;
                    if (!seenTypes.Add(key))
                        continue;

                    if (Activator.CreateInstance(type) is ISRTBasicConfigurations plugin)
                        plugins.Add(plugin);
                }
            }

            return plugins
                .OrderBy(p => p.Order)
                .ThenBy(p => p.FeatureName, StringComparer.Ordinal)
                .ToList();
        }

        private static void LoadCandidateAssemblies(ILogger logger)
        {
            var baseDir = AppContext.BaseDirectory;
            if (!Directory.Exists(baseDir))
                return;

            var loaded = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetName().Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)!;

            foreach (var path in Directory.GetFiles(baseDir, "SRT.Core*.dll"))
            {
                var name = Path.GetFileNameWithoutExtension(path);
                if (loaded.Contains(name))
                    continue;

                try
                {
                    AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                    loaded.Add(name);
                    logger.LogWarning("Loaded SRT plugin assembly {Name}", name);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to load SRT plugin assembly {Path}", path);
                }
            }
        }
    }
}
