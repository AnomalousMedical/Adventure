namespace DiligentEngine.RT.Resources;

public class TerrainNoise
{
    public void CreateTerrainNoise(int seed, out FastNoiseLite noise, out FastNoiseLite distanceNoise)
    {
        noise = CreateCommonNoise(seed);
        noise.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);
        distanceNoise = CreateCommonNoise(seed);
        distanceNoise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Div);
    }

    private static FastNoiseLite CreateCommonNoise(int seed)
    {
        var noise = new FastNoiseLite(seed);
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
        return noise;
    }
}
