using BepuPlugin;
using Engine;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBepuPlugin<T>(this IServiceCollection services, Action<BepuScene<T>.Description> configure = null)
        {
            services.TryAddSingleton<IBepuScene<T>, BepuScene<T>>();
            services.TryAddSingleton<BepuScene<T>.Description>(s =>
            {
                var desc = new BepuScene<T>.Description();
                configure?.Invoke(desc);
                return desc;
            });

            return services;
        }

        public static IServiceCollection AddBepuSceneType<T>(this IServiceCollection services, Action<BepuScene<T>.Description> configure = null)
        {
            services.TryAddSingleton<IBepuScene<T>, BepuScene<T>>();
            services.TryAddSingleton<BepuScene<T>.Description>(s =>
            {
                var desc = new BepuScene<T>.Description();
                configure?.Invoke(desc);
                return desc;
            });

            return services;
        }
    }
}
