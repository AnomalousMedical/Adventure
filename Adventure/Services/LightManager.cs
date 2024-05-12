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
            while (i < count && cameraAndLight.CheckoutLight(out lightIndex))
            {
                var light = lights[i];
                cameraAndLight.LightLength[lightIndex] = light.Length;
                cameraAndLight.LightPos[lightIndex] = light.Position;
                cameraAndLight.LightColor[lightIndex] = light.Color;
                ++i;
            }
        }
    }

    class TypedLightManager<T>
    {
        private readonly LightManager lightManager;
        private readonly List<Light> lights = new List<Light>();
        private bool active;

        public TypedLightManager(LightManager lightManager)
        {
            this.lightManager = lightManager;
        }

        public void AddLight(Light light)
        {
            lights.Add(light);
            if (active)
            {
                lightManager.AddLight(light);
            }
        }

        public void RemoveLight(Light light)
        {
            lights.Remove(light);
            if (active)
            {
                lightManager.RemoveLight(light);
            }
        }

        public void SetActive(bool active)
        {
            if (this.active != active)
            {
                this.active = active;
                if (active)
                {
                    foreach (var light in lights)
                    {
                        lightManager.AddLight(light);
                    }
                }
                else
                {
                    foreach (var light in lights)
                    {
                        lightManager.RemoveLight(light);
                    }
                }
            }
        }
    }
}
