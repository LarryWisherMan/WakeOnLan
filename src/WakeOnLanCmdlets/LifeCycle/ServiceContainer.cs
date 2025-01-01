using Microsoft.Extensions.DependencyInjection;
using System;

namespace WakeOnLanCmdlets.LifeCycle
{
    public static class ServiceContainer
    {
        private static IServiceProvider _serviceProvider;

        public static IServiceProvider Instance => _serviceProvider ?? throw new InvalidOperationException("Service provider is not initialized.");

        public static void Initialize(IServiceCollection services)
        {
            _serviceProvider = services.BuildServiceProvider();
        }
    }
}
