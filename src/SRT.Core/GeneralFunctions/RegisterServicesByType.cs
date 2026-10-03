using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace SRT.Core
{
    public static class RegisterServicesByType
    {
        public static IServiceCollection SRT_RegisterServices<TInterface, TInterfaceSearchInChild>(this IServiceCollection services)
        {
            var types = SRT_GetTypes<TInterface, TInterfaceSearchInChild>();
            var interfaceType = typeof(TInterface);
            foreach (var type in types)
                services.AddScoped(interfaceType, type);

            return services;
        }

        public static List<Type> SRT_GetTypes<TInterface, TInterfaceSearchInChild>()
        {
            var namespacePrefixSearchInChild = typeof(TInterfaceSearchInChild).Namespace!;
            return SRT_GetTypes<TInterface>(namespacePrefixSearchInChild);
        }

        public static List<Type> SRT_GetTypes<TInterface>(string namespacePrefixSearchInChild)
        {
            var interfaceType = typeof(TInterface);

            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a =>
                {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        return ex.Types.Where(t => t != null).Cast<Type>().ToArray();
                    }
                })
                .Where(t => t is { IsClass: true, IsAbstract: false }
                            && t.GetInterfaces().Contains(interfaceType)
                            && t.Namespace != null
                            && t.Namespace.StartsWith(namespacePrefixSearchInChild))
                .ToList();
        }
    }
}
