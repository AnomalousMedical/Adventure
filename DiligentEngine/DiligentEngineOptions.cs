using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DiligentEngine
{
    [JsonConverter(typeof(JsonStringEnumConverter<UpsamplingMethod>))]
    public enum UpsamplingMethod
    {
        None,
        FSR1
    }

    public class DiligentEngineOptions
    {
        public GraphicsEngine.FeatureFlags Features { get; set; }

        public GraphicsEngine.RenderApi RenderApi { get; set; } = GraphicsEngine.RenderApi.Vulkan;

        /// <summary>
        /// Set the device id to use. Leave null to autodetect the discrete graphics card.
        /// </summary>
        public UInt32? DeviceId { get; set; }

        public UpsamplingMethod UpsamplingMethod { get; set; } = UpsamplingMethod.None;

        public float FSR1RenderPercentage { get; set; } = 0.75f;
    }
}
