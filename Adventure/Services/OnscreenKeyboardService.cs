using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{ 
    interface IOnscreenKeyboardService
    {
        void ShowKeyboard(RequestedOnscreenKeyboardMode mode, int x, int y, int width, int height);
    }

    class NullOnscreenKeyboardService : IOnscreenKeyboardService
    {
        public void ShowKeyboard(RequestedOnscreenKeyboardMode mode, int x, int y, int width, int height)
        {

        }
    }

    static class OnscreenKeyboardServiceExt
    {
        public static void AddOnscreenKeyboard(this IServiceCollection services)
        {
            services.TryAddSingleton<IOnscreenKeyboardService, NullOnscreenKeyboardService>();
        }
    }
}
