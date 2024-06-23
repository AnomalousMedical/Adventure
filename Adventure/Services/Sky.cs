using DiligentEngine.RT;
using DiligentEngine.RT.Resources;
using Engine;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    class Sky : IDisposable
    {
        const long OneHour = 60L * 60L * Clock.SecondsToMicro;
        private readonly ITimeClock timeClock;
        private readonly RTCameraAndLight cameraAndLight;
        private readonly TextureManager textureManager;
        private readonly RayTracingRenderer rayTracingRenderer;
        private readonly ActiveTextures activeTextures;

        //Light
        private Vector3 sunPosition;
        private Vector3 moonPosition;
        Vector4 lightColor = new Vector4(1, 1, 1, 1);
        float lightIntensity = 3;
        float averageLogLum = 0.3f;

        private bool texturesReady = false;
        private Vector2 uvOffset = new Vector2();

        record SkyTextureInfo(CC0TextureResult TextureSet, int TextureIndex);

        SkyTextureInfo DaySky;
        SkyTextureInfo NightSky;
        SkyTextureInfo DawnSky;
        SkyTextureInfo DuskSky;

        public Sky(ITimeClock timeClock, RTCameraAndLight cameraAndLight, TextureManager textureManager, IScopedCoroutine scopedCoroutine, RayTracingRenderer rayTracingRenderer, ActiveTextures activeTextures)
        {
            this.timeClock = timeClock;
            this.cameraAndLight = cameraAndLight;
            this.textureManager = textureManager;
            this.rayTracingRenderer = rayTracingRenderer;
            this.activeTextures = activeTextures;

            scopedCoroutine.RunTask(async () =>
            {
                var dayTextureTask = textureManager.Checkout(new CCOTextureBindingDescription("Graphics/Textures/PolyHaven/kloofendal_48d_partly_cloudy_puresky_8k_Adj", false, Ext: "webp", MipLevels: 1));
                var nightTextureTask = textureManager.Checkout(new CCOTextureBindingDescription("Graphics/Textures/NASA/starmap_2020_8k", false, Ext: "webp", MipLevels: 1));
                var dawnTextureTask = textureManager.Checkout(new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/EveningSkyHDRI017B_8K-TONEMAPPED", false, Ext: "webp", MipLevels: 1));
                var duskTextureTask = textureManager.Checkout(new CCOTextureBindingDescription("Graphics/Textures/PolyHaven/syferfontein_1d_clear_puresky_8k", false, Ext: "webp", MipLevels: 1));

                await Task.WhenAll
                (
                    dayTextureTask,
                    nightTextureTask,
                    dawnTextureTask,
                    duskTextureTask
                );

                var dayTexture = dayTextureTask.Result;
                var nightTexture = nightTextureTask.Result;
                var dawnTexture = dawnTextureTask.Result;
                var duskTexture = duskTextureTask.Result;

                DaySky = new SkyTextureInfo(dayTexture, activeTextures.AddActiveTexture2(dayTexture));
                NightSky = new SkyTextureInfo(nightTexture, activeTextures.AddActiveTexture2(nightTexture));
                DawnSky = new SkyTextureInfo(dawnTexture, activeTextures.AddActiveTexture2(dawnTexture));
                DuskSky = new SkyTextureInfo(duskTexture, activeTextures.AddActiveTexture2(duskTexture));

                texturesReady = true;
            });
        }

        public void Dispose()
        {
            activeTextures.RemoveActiveTexture(DaySky.TextureSet);
            activeTextures.RemoveActiveTexture(NightSky.TextureSet);
            activeTextures.RemoveActiveTexture(DawnSky.TextureSet);
            activeTextures.RemoveActiveTexture(DuskSky.TextureSet);

            textureManager.TryReturn(DaySky.TextureSet);
            textureManager.TryReturn(NightSky.TextureSet);
            textureManager.TryReturn(DawnSky.TextureSet);
            textureManager.TryReturn(DuskSky.TextureSet);
        }

        const float LightDistance = 10000.0f;

        public Vector3 CelestialOffset { get; set; }

        public unsafe void UpdateLight(Clock clock)
        {
            uvOffset.x += 0.0001f * clock.DeltaSeconds;
            if(uvOffset.x > 1f)
            {
                uvOffset.x -= 1f;
            }

            if (!texturesReady)
            {
                return;
            }

            var rotation = new Quaternion(Vector3.UnitZ, timeClock.TimeFactor * 2 * MathF.PI);
            sunPosition = Quaternion.quatRotate(rotation, Vector3.Down) * LightDistance;
            sunPosition += new Vector3(0f, 0f, -LightDistance);
            sunPosition += CelestialOffset;

            moonPosition = Quaternion.quatRotate(rotation, Vector3.Up) * LightDistance;
            moonPosition += new Vector3(0f, 0f, -LightDistance);
            moonPosition += CelestialOffset;

            if (timeClock.IsDay)
            {
                var dayFactor = (timeClock.DayFactor - 0.5f) * 2.0f;
                var noonFactor = 1.0f - Math.Abs(dayFactor);
                lightIntensity = 5f * noonFactor + 2.0f;

                averageLogLum = 0.3f;

                if (timeClock.CurrentTimeMicro < timeClock.DayStart + OneHour)
                {
                    float timeFactor = (timeClock.CurrentTimeMicro - timeClock.DayStart) / (float)OneHour;
                    BlendSetPallet(timeFactor, DawnSky, DaySky);
                }
                else if (timeClock.CurrentTimeMicro > timeClock.DayEnd - OneHour)
                {
                    float timeFactor = (timeClock.CurrentTimeMicro - (timeClock.DayEnd - OneHour)) / (float)OneHour;
                    BlendSetPallet(timeFactor, DaySky, DuskSky);
                }
                else
                {
                    SetPallet(DaySky);
                }
            }
            else
            {
                var nightFactor = (timeClock.NightFactor - 0.5f) * 2.0f;
                var midnightFactor = 1.0f - Math.Abs(nightFactor);

                lightIntensity = 0.7f * midnightFactor + 2.0f;

                averageLogLum = 0.8f;

                if (timeClock.CurrentTimeMicro > timeClock.DayStart - OneHour && timeClock.CurrentTimeMicro <= timeClock.DayStart)
                {
                    float timeFactor = (timeClock.CurrentTimeMicro - (timeClock.DayStart - OneHour)) / (float)OneHour;
                    BlendSetPallet(timeFactor, NightSky, DawnSky);
                }
                else if (timeClock.CurrentTimeMicro >= timeClock.DayEnd && timeClock.CurrentTimeMicro < timeClock.DayEnd + OneHour)
                {
                    float timeFactor = (timeClock.CurrentTimeMicro - timeClock.DayEnd) / (float)OneHour;
                    BlendSetPallet(timeFactor, DuskSky, NightSky);
                }
                else
                {
                    SetPallet(NightSky);
                }
            }

            int lightIndex;
            if(cameraAndLight.CheckoutLight(out lightIndex))
            {
                cameraAndLight.LightPos[lightIndex] = new Vector4(sunPosition.x, sunPosition.y, sunPosition.z, 0);
            }

            if (cameraAndLight.CheckoutLight(out lightIndex))
            {
                cameraAndLight.LightPos[lightIndex] = new Vector4(moonPosition.x, moonPosition.y, moonPosition.z, 0);
            }
        }

        private void SetPallet(SkyTextureInfo src)
        {
            rayTracingRenderer.SetMissTextureSet(src.TextureIndex, uvOffset: uvOffset);
        }

        private void BlendSetPallet(float factor, SkyTextureInfo tex1, SkyTextureInfo tex2)
        {
            rayTracingRenderer.SetMissTextureSet(tex1.TextureIndex, tex2.TextureIndex, factor, uvOffset: uvOffset);
        }
    }
}
