using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static DiligentEngine.TextureLoader;

namespace DiligentEngine.RT.Resources;

public class NoiseTextureManager
{
    private readonly ILogger<NoiseTextureManager> logger;
    private readonly TextureLoader textureLoader;
    private readonly GraphicsEngine graphicsEngine;

    public NoiseTextureManager
    (
        ILogger<NoiseTextureManager> logger,
        TextureLoader textureLoader,
        GraphicsEngine graphicsEngine
    )
    {
        this.logger = logger;
        this.textureLoader = textureLoader;
        this.graphicsEngine = graphicsEngine;
    }

    public async Task<CC0TextureResult> GenerateTexture(FastNoiseLite noise, uint width, uint height)
    {
        var Barriers = new List<StateTransitionDesc>(1);

        var perlinNoise = await Task.Run(() =>
        {
            var sw = new Stopwatch();
            sw.Start();

            var size = width * height;
            var span = new Span<float>(new float[size]);

            var min = float.MaxValue;
            var max = float.MinValue;

            unsafe
            {
                int index = 0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        span[index] = noise.GetNoise(x, y);

                        if (span[index] > max)
                        {
                            max = span[index];
                        }

                        if (span[index] < min)
                        {
                            min = span[index];
                        }

                        ++index;
                    }
                }
            }

            var range = max - min;
            var ratio = (1.0f) / range;
            var offset = -min;

            //normalize values
            for (int i = 0; i < size; ++i)
            {
                span[i] = (span[i] + offset) * ratio;
            }

            var perlinNoise = textureLoader.CreateTextureFromFloatSpan(span, "Perlin Noise Texture", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, width, height);
            Barriers.Add(new StateTransitionDesc { pResource = perlinNoise.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });

            sw.Stop();
            logger.LogInformation($"Noise texture generated in {sw.ElapsedMilliseconds}ms");

            return perlinNoise;
        });

        graphicsEngine.ImmediateContext.TransitionResourceStates(Barriers);

        var result = new CC0TextureResult();
        result.SetBaseColorMap(perlinNoise, false, false);
        return result;
    }

    public async Task<CC0TextureResult> GenerateDoubleNoiseTexture(FastNoiseLite noise1, FastNoiseLite noise2, uint width, uint height)
    {
        var Barriers = new List<StateTransitionDesc>(1);

        var perlinNoise = await Task.Run(async () =>
        {
            var sw = new Stopwatch();
            sw.Start();

            var size = width * height;
            var pixels = new HalfRgTexturePixel[size];

            await Task.WhenAll
            (
                Task.Run(() =>
                {
                    int index = 0;
                    var span = new Span<HalfRgTexturePixel>(pixels);

                    var min = Half.MaxValue;
                    var max = Half.MinValue;

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            span[index].r = (Half)noise1.GetNoise(x, y);

                            if (span[index].r > max)
                            {
                                max = span[index].r;
                            }

                            if (span[index].r < min)
                            {
                                min = span[index].r;
                            }

                            ++index;
                        }
                    }

                    var range = (float)max - (float)min;
                    var ratio = (1.0f) / range;
                    var offset = -(float)min;

                    //normalize values
                    for (int i = 0; i < size; ++i)
                    {
                        span[i].r = (Half)(((float)span[i].r + offset) * ratio);
                    }
                }),
                Task.Run(() =>
                {
                    int index = 0;
                    var span = new Span<HalfRgTexturePixel>(pixels);

                    var min = Half.MaxValue;
                    var max = Half.MinValue;

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            span[index].g = (Half)noise2.GetNoise(x, y);

                            if (span[index].g > max)
                            {
                                max = span[index].g;
                            }

                            if (span[index].g < min)
                            {
                                min = span[index].g;
                            }

                            ++index;
                        }
                    }

                    var range = (float)max - (float)min;
                    var ratio = (1.0f) / range;
                    var offset = -(float)min;

                    //normalize values
                    for (int i = 0; i < size; ++i)
                    {
                        span[i].g = (Half)(((float)span[i].g + offset) * ratio);
                    }
                })
            );

            var perlinNoise = textureLoader.CreateTextureFromFloatSpan(new Span<HalfRgTexturePixel>(pixels), "Perlin Noise Texture", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, width, height);
            Barriers.Add(new StateTransitionDesc { pResource = perlinNoise.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });

            sw.Stop();
            logger.LogInformation($"GenerateDoubleNoiseTexture in {sw.ElapsedMilliseconds}ms");

            return perlinNoise;
        });

        graphicsEngine.ImmediateContext.TransitionResourceStates(Barriers);

        var result = new CC0TextureResult();
        result.SetBaseColorMap(perlinNoise, false, false);
        return result;
    }

    public void ReturnTexture(CC0TextureResult texture)
    {
        texture.Dispose();
    }
}
