using FreeImageAPI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiligentEngine.RT.Resources;

public class NoiseTextureManager
{
    private readonly TextureLoader textureLoader;
    private readonly GraphicsEngine graphicsEngine;

    public NoiseTextureManager
    (
        TextureLoader textureLoader,
        GraphicsEngine graphicsEngine
    )
    {
        this.textureLoader = textureLoader;
        this.graphicsEngine = graphicsEngine;
    }

    public Task<CC0TextureResult> GenerateTexture(int seed, uint width, uint height, FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.Perlin)
    {
        return Task.Run(() =>
        {
            var Barriers = new List<StateTransitionDesc>(1);

            // Create and configure FastNoise object
            FastNoiseLite noise = new FastNoiseLite(seed);
            noise.SetNoiseType(noiseType);

            // Gather noise data

            var size = width * height;
            var span = new Span<float>(new float[size]);

            unsafe
            {
                int index = 0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        span[index++] = noise.GetNoise(x, y);
                    }
                }
            }

            var perlinNoise = textureLoader.CreateTextureFromFloatSpan(span, "Perlin Noise Texture", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, width, height);
            Barriers.Add(new StateTransitionDesc { pResource = perlinNoise.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });

            graphicsEngine.ImmediateContext.TransitionResourceStates(Barriers);

            var result = new CC0TextureResult();
            result.SetBaseColorMap(perlinNoise, false, false);
            return result;
        });
    }

    public void ReturnTexture(CC0TextureResult texture)
    {
        texture.Dispose();
    }
}
