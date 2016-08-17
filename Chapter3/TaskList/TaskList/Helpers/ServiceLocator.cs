using System;
using System.Collections.Generic;

namespace TaskList.Helpers
{
    public sealed class ServiceLocator
    {
        /// <summary>
        /// Add a singleton service implementation
        /// </summary>
        /// <typeparam name="TContract">The type of service</typeparam>
        /// <typeparam name="TService">The concrete implementation of the service</typeparam>
        public static void Add<TContract, TService>() where TService : new()
        {
            Instance.InternalAdd<TContract, TService>();
        }

        /// <summary>
        /// Resolve the service type into the implementation.
        /// </summary>
        /// <typeparam name="T">The type of service</typeparam>
        /// <returns>The concrete implementation of the service</returns>
        /// <exception cref="InvalidCastException">If you mix and match services</exception>
        public static T Get<T>() where T : class
        {
            return Instance.Resolve<T>();
        }

        #region Internal Representation
        static readonly Lazy<ServiceLocator> instance = new Lazy<ServiceLocator>(() => new ServiceLocator());
        readonly Dictionary<Type, Lazy<object>> services = new Dictionary<Type, Lazy<object>>();

        static ServiceLocator Instance => instance.Value;

        void InternalAdd<TContract, TService>() where TService : new()
        {
            services[typeof(TContract)] = new Lazy<object>(() => Activator.CreateInstance(typeof(TService)));
        }

        T Resolve<T>() where T : class
        {
            Lazy<object> service;
            if (services.TryGetValue(typeof(T), out service))
            {
                return (T)service.Value;
            }
            return null;
        }
    }
    #endregion
}
