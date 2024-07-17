using DiligentEngine;
using Engine.Platform;
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

    class KeyboardMouseBinding
    {
        public KeyboardMouseBinding() { }

        public KeyboardMouseBinding(KeyboardButtonCode code)
        {
            this.KeyboardButton = code;
        }

        public KeyboardMouseBinding(MouseButtonCode code)
        {
            this.MouseButton = code;
        }

        public KeyboardButtonCode? KeyboardButton { get; set; }

        public MouseButtonCode? MouseButton { get; set; }
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

        public Dictionary<KeyBindings, KeyboardMouseBinding> KeyboardBindings { get; set; } = new Dictionary<KeyBindings, KeyboardMouseBinding>();

        public Dictionary<KeyBindings, GamepadButtonCode>[] GamepadBindings { get; set; } =
        [
            new Dictionary<KeyBindings, GamepadButtonCode>(),
            new Dictionary<KeyBindings, GamepadButtonCode>(),
            new Dictionary<KeyBindings, GamepadButtonCode>(),
            new Dictionary<KeyBindings, GamepadButtonCode>(),
        ];

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool Debug { get; set; } =
            #if RELEASE
                false;
            #else
                true;

        internal void Update()
        {
            if(KeyboardBindings == null)
            {
                KeyboardBindings = new Dictionary<KeyBindings, KeyboardMouseBinding>();
            }
            if(GamepadBindings == null)
            {
                GamepadBindings =
                [
                    new Dictionary<KeyBindings, GamepadButtonCode>(),
                    new Dictionary<KeyBindings, GamepadButtonCode>(),
                    new Dictionary<KeyBindings, GamepadButtonCode>(),
                    new Dictionary<KeyBindings, GamepadButtonCode>(),
                ];
            }
        }
#endif
    }
}
