using Engine;
using Engine.Platform;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OSPlatformServiceCollectionExtensions
    {
        public static IServiceCollection AddSharpGui(this IServiceCollection services, Action<IServiceProvider, SharpGuiOptions> configure = null)
        {
            services.AddSingleton<SharpGuiOptions>(s =>
            {
                var options = new SharpGuiOptions();
                configure?.Invoke(s, options);
                return options;
            });
            services.TryAddSingleton<ISharpGui, SharpGuiImpl>();
            services.TryAddSingleton<SharpGuiBuffer>();
            services.TryAddSingleton<SharpGuiRenderer>();
            services.TryAddSingleton<IScreenPositioner, ScreenPositioner>();
            services.TryAddSingleton<IFontManager, FontManager>();
            services.TryAddSingleton<IImageManager, ImageManager>();

            return services;
        }
    }
}
