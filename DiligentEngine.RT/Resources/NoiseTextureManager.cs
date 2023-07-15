using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

    public async Task<CC0TextureResult> GenerateTexture(FastNoiseLite noise, int width, int height, int numThreads = 8)
    {
        var Barriers = new List<StateTransitionDesc>(1);

        var perlinNoise = await Task.Run(async () =>
        {
            var sw = new Stopwatch();
            sw.Start();

            var size = width * height;
            var pixels = new float[size];

            var partialHeight = height / numThreads;

            List<Task<MinMaxResult>> generatorTasks = new List<Task<MinMaxResult>>();
            var kickoff = numThreads - 1;
            for (int i = 0; i < kickoff; ++i)
            {
                generatorTasks.Add(CreateNoise(noise, pixels, i * partialHeight, width, (i + 1) * partialHeight));
            }
            generatorTasks.Add(CreateNoise(noise, pixels, kickoff * partialHeight, width, height));

            await Task.WhenAll(generatorTasks);

            var minMax = GetMinMax(generatorTasks.Select(i => i.Result));
            var min = minMax.Min;
            var max = minMax.Max;

            var range = max - min;
            var ratio = (1.0f) / range;
            var offset = -min;

            //normalize values
            for (int i = 0; i < size; ++i)
            {
                pixels[i] = (pixels[i] + offset) * ratio;
            }

            var perlinNoise = textureLoader.CreateTextureFromFloatSpan(pixels, "Perlin Noise Texture", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, (uint)width, (uint)height);
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

    private Task<MinMaxResult> CreateNoise(FastNoiseLite noise, float[] pixels, int yStart, int width, int height)
    {
        return Task.Run(() =>
        {
            int index = yStart * width;
            var span = pixels;

            var min = float.MaxValue;
            var max = float.MinValue;

            for (int y = yStart; y < height; y++)
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

            return new MinMaxResult(min, max);
        });
    }

    public async Task<CC0TextureResult> GenerateDoubleNoiseTexture(FastNoiseLite noise1, FastNoiseLite noise2, int width, int height, int numThreads = 8)
    {
        var Barriers = new List<StateTransitionDesc>(1);

        var perlinNoise = await Task.Run(async () =>
        {
            var sw = new Stopwatch();
            sw.Start();

            var size = width * height;
            var pixels = new HalfRgTexturePixel[size];

            var partialHeight = height / numThreads;

            List<Task<MinMaxResult>> redGeneratorTasks = new List<Task<MinMaxResult>>();
            var kickoff = numThreads - 1;
            for(int i = 0; i < kickoff; ++i)
            {
                redGeneratorTasks.Add(CreateRedNoise(noise1, pixels, i * partialHeight, width, (i + 1) * partialHeight));
            }
            redGeneratorTasks.Add(CreateRedNoise(noise1, pixels, kickoff * partialHeight, width, height));

            List<Task<MinMaxResult>> greenGeneratorTasks = new List<Task<MinMaxResult>>();
            kickoff = numThreads - 1;
            for (int i = 0; i < kickoff; ++i)
            {
                greenGeneratorTasks.Add(CreateGreenNoise(noise2, pixels, i * partialHeight, width, (i + 1) * partialHeight));
            }
            greenGeneratorTasks.Add(CreateGreenNoise(noise2, pixels, kickoff * partialHeight, width, height));

            await Task.WhenAll(redGeneratorTasks.Concat(greenGeneratorTasks));

            var redMinMax = GetMinMax(redGeneratorTasks.Select(i => i.Result));
            var greenMinMax = GetMinMax(greenGeneratorTasks.Select(i => i.Result));

            await Task.WhenAll
            (
                NormalizeRed(pixels, (Half)redMinMax.Min, (Half)redMinMax.Max),
                NormalizeGreen(pixels, (Half)greenMinMax.Min, (Half)greenMinMax.Max)
            );

            var perlinNoise = textureLoader.CreateTextureFromFloatSpan(new Span<HalfRgTexturePixel>(pixels), "Perlin Noise Texture", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, (uint)width, (uint)height);
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

    record MinMaxResult(float Min, float Max);

    private MinMaxResult GetMinMax(IEnumerable<MinMaxResult> results)
    {
        var min = float.MaxValue;
        var max = float.MinValue;

        foreach(var i in results)
        {
            if(i.Min < min)
            {
                min = i.Min;
            }

            if(i.Max > max)
            {
                max = i.Max;
            }
        }

        return new MinMaxResult(min, max);
    }

    private Task NormalizeRed(HalfRgTexturePixel[] pixels, Half min, Half max)
    {
        return Task.Run(() =>
        {
            var range = (float)max - (float)min;
            var ratio = (1.0f) / range;
            var offset = -(float)min;
            //normalize values
            //var span = new Span<HalfRgTexturePixel>(pixels);
            var span = pixels;
            var size = pixels.Length;
            for (int i = 0; i < size; ++i)
            {
                span[i].r = (Half)(((float)span[i].r + offset) * ratio);
            }
        });
    }

    private Task NormalizeGreen(HalfRgTexturePixel[] pixels, Half min, Half max)
    {
        return Task.Run(() =>
        {
            var range = (float)max - (float)min;
            var ratio = (1.0f) / range;
            var offset = -(float)min;
            //normalize values
            //var span = new Span<HalfRgTexturePixel>(pixels);
            var span = pixels;
            var size = pixels.Length;
            for (int i = 0; i < size; ++i)
            {
                span[i].g = (Half)(((float)span[i].g + offset) * ratio);
            }
        });
    }

    private Task<MinMaxResult> CreateRedNoise(FastNoiseLite noise, HalfRgTexturePixel[] pixels, int yStart, int width, int height)
    {
        return Task.Run(() =>
        {
            int index = yStart * width;
            //var span = new Span<HalfRgTexturePixel>(pixels);
            var span = pixels;

            var min = Half.MaxValue;
            var max = Half.MinValue;

            for (int y = yStart; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    span[index].r = (Half)noise.GetNoise(x, y);

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

            return new MinMaxResult((float)min, (float)max);
        });
    }

    private Task<MinMaxResult> CreateGreenNoise(FastNoiseLite noise, HalfRgTexturePixel[] pixels, int yStart, int width, int height)
    {
        return Task.Run(() =>
        {
            int index = yStart * width;
            //var span = new Span<HalfRgTexturePixel>(pixels);
            var span = pixels;

            var min = Half.MaxValue;
            var max = Half.MinValue;

            for (int y = yStart; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    span[index].g = (Half)noise.GetNoise(x, y);

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

            return new MinMaxResult((float)min, (float)max);
        });
    }

    public void ReturnTexture(CC0TextureResult texture)
    {
        texture.Dispose();
    }
}
