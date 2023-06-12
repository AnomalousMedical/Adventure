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

            //Temp 1
            //noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
            //noise.SetFrequency(0.01f);
            //noise.SetFractalOctaves(4);
            //noise.SetFractalLacunarity(2);
            //noise.SetFractalGain(0.9f);
            //noise.SetFractalWeightedStrength(0.7f);
            //noise.SetFractalPingPongStrength(3.0f);
            //noise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Euclidean);
            //noise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Add);
            //noise.SetCellularJitter(1.0f);

            //Distance to edge
            noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
            noise.SetRotationType3D(FastNoiseLite.RotationType3D.ImproveXYPlanes);
            noise.SetFrequency(0.01f);
            noise.SetFractalType(FastNoiseLite.FractalType.None);
            noise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.EuclideanSq);
            noise.SetCellularJitter(1.0f);
            noise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
            noise.SetRotationType3D(FastNoiseLite.RotationType3D.None);
            noise.SetDomainWarpAmp(160.0f);
            noise.SetFrequency(0.005f);
            noise.SetFractalType(FastNoiseLite.FractalType.None);
            //Change this between distance and cell value to get both textures
            //noise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Div);
            noise.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);

            // Gather noise data

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
            var offset = -min;

            //normalize values
            for (int i =  0; i < size; ++i)
            {
                span[i] = (span[i] + offset) / range;
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
