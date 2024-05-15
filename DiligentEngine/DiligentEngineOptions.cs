using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine
{
    public class DiligentEngineOptions
    {
        public GraphicsEngine.FeatureFlags Features { get; set; }

        public GraphicsEngine.RenderApi RenderApi { get; set; } = GraphicsEngine.RenderApi.Vulkan;

        /// <summary>
        /// Set the device id to use. Leave null to autodetect the discrete graphics card.
        /// </summary>
        public UInt32? DeviceId { get; set; }
    }
}
