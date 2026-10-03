using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.CustomAttributes
{
    public enum InjectionLifetime
    {
        Scoped,
        Singleton,
        Transient
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class InjectionControllerAttribute : Attribute
    {
        public Type[] ServiceTypes { get; }
        public InjectionLifetime Lifetime { get; }

        public InjectionControllerAttribute(InjectionLifetime lifetime, params Type[] serviceTypes)
        {
            if (serviceTypes == null || serviceTypes.Length == 0)
                throw new ArgumentException("At least one interface must be provided.", nameof(serviceTypes));

            foreach (var type in serviceTypes)
            {
                if (!type.IsInterface)
                    throw new ArgumentException($"Provided type {type.Name} must be an interface.", nameof(serviceTypes));
            }

            ServiceTypes = serviceTypes;
            Lifetime = lifetime;
        }
    }
}
