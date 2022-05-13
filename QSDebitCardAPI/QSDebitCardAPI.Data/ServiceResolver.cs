using System;
namespace QSDataUpdateAPI.Domain.Services.Helpers
{
    public class ServiceResolver
    {
        public static ServiceResolver Instance { get; private set; }
        public IServiceProvider _serviceProvider { get; }

        private ServiceResolver(IServiceProvider provider)
        {
            _serviceProvider = provider;
        }

        public static ServiceResolver Register(IServiceProvider provider)
        {
            Instance = Instance ?? new ServiceResolver(provider);
            return Instance;
        }
            
        public static T Resolve<T>()
        {
            return (T)Instance._serviceProvider.GetService(typeof(T));
        }
    }
}
