using DiligentEngine.RT;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    class Light
    {
        public Color Color { get; set; }

        public Vector4 Position { get; set; }

        public float Length { get; set; }
    }

    class LightManager
    {
        private readonly RTCameraAndLight cameraAndLight;
        private readonly List<Light> lights = new List<Light>();

        public LightManager(RTCameraAndLight cameraAndLight)
        {
            this.cameraAndLight = cameraAndLight;
        }

        public void AddLight(Light light)
        {
            lights.Add(light);
        }

        public void RemoveLight(Light light)
        {
            lights.Remove(light);
        }

        public void UpdateLights()
        {
            int i = 0;
            int lightIndex;
            int count = lights.Count;
            while (cameraAndLight.CheckoutLight(out lightIndex) && i < count)
            {
                var light = lights[i];
                cameraAndLight.LightLength[i] = light.Length;
                cameraAndLight.LightPos[i] = light.Position;
                cameraAndLight.LightColor[i] = light.Color;
            }
        }
    }
}
