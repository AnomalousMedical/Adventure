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

    public Task<CC0TextureResult> GenerateTexture(int seed, int width, int height, FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.Perlin)
    {
        return Task.Run(() =>
        {
            //This does work, but its hacked
            //You can set the perlin as the floor and you can see it, but its not a good texture
            //need to use fp16 textures
            //convert this to its own class and have it load and manage its own textures in the right format

            var Barriers = new List<StateTransitionDesc>(1);

            // Create and configure FastNoise object
            FastNoiseLite noise = new FastNoiseLite(seed);
            noise.SetNoiseType(noiseType);

            using var bmp = new FreeImageBitmap(width, height, PixelFormat.Format32bppArgb);

            // Gather noise data
            unsafe
            {
                int index = 0;
                var firstPixel = ((uint*)bmp.Scan0.ToPointer()) - ((bmp.Height - 1) * bmp.Width);
                var size = bmp.Width * bmp.Height;
                var span = new Span<UInt32>(firstPixel, size);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var pixelValue = (uint)(noise.GetNoise(x, y) * 0xFFu);
                        span[index++] = pixelValue + (pixelValue << 8) + (pixelValue << 16) + 0xFF000000u;
                    }
                }
            }

            var perlinNoise = textureLoader.CreateTextureFromImage(bmp, 0, "Perlin Noise Texture", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, true);
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
