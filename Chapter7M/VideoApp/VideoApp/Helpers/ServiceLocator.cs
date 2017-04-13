using System;
using System.Collections.Generic;
using System.Text;

namespace VideoApp.Helpers
{
    public sealed class ServiceLocator
    {
        static readonly Lazy<ServiceLocator> instance = new Lazy<ServiceLocator>(() => new ServiceLocator());
        readonly Dictionary<Type, Lazy<object>> registeredServices = new Dictionary<Type, Lazy<object>>();

        /// <summary>
        /// Singleton instance for the default service locator
        /// </summary>
        public static ServiceLocator Instance => instance.Value;

        /// <summary>
        /// Add a new service implementation
        /// </summary>
        /// <typeparam name="TContract">The type of service</typeparam>
        /// <typeparam name="TService">The service implementation</typeparam>
        public void AddInner<TContract, TService>() where TService : new()
        {
            registeredServices[typeof(TContract)] = new Lazy<object>(() => Activator.CreateInstance(typeof(TService)));
        }

        public static void Add<TContract, TService>() where TService : new()
        {
            instance.Value.AddInner<TContract, TService>();
        }

        /// <summary>
        /// Resolve the service type into the implementation.  This assumes the key used to register the
        /// object is of the appropriate type - throws InvalidCastException if you get this wrong (at runtime)
        /// </summary>
        /// <typeparam name="T">The type of service</typeparam>
        /// <returns>The service implementation</returns>
        public T ResolveInner<T>() where T : class
        {
            Lazy<object> service;
            if (registeredServices.TryGetValue(typeof(T), out service))
            {
                return (T)service.Value;
            }
            return null;
        }

        public static T Resolve<T>() where T : class
        {
            return instance.Value.ResolveInner<T>();
        }
    }
}
