using DiligentEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Adventure.Services
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(GameOptions))]
    internal partial class GameOptionsSourceGenerationContext : JsonSerializerContext
    {
    }

    class GameOptions
    {
        public GraphicsEngine.RenderApi RenderApi { get; set; } = GraphicsEngine.RenderApi.D3D12;

        public bool Fullscreen { get; set; } = true;

        public String CurrentSave { get; set; }

        public float MasterVolume { get; set; } = 1.0f;

        public float MusicVolume { get; set; } = 0.35f;

        public float SfxVolume { get; set; } = 1.0f;

        public uint? DeviceId { get; set; }

        public UpsamplingMethod UpsamplingMethod { get; set; } = UpsamplingMethod.None;

        public float FSR1RenderPercentage { get; set; } = 0.75f;

        public uint PresentInterval { get; set; } = 1;
    }
}
