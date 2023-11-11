using Engine;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anomalous.OSPlatform
{
    public class OSPlatformOptions
    {
        /// <summary>
        /// The type of the class to use to specify event layers. Default: built in event layers.
        /// </summary>
        public Type EventLayersType { get; set; } = typeof(EventLayers);

        /// <summary>
        /// This is called when the event manager is created.
        /// </summary>
        public Action<EventManager> EventManagerCreated { get; set; }

        /// <summary>
        /// Set this to true to enable multitouch input. Default: true.
        /// </summary>
        public bool EnableMultitiouch { get; set; } = true;
    }
}
