namespace DiligentEngine.RT.Resources;

public class TerrainNoise
{
    public FastNoiseLite CreateBlendTerrainNoise(int seed)
    {
        var noise = new FastNoiseLite(seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetRotationType3D(FastNoiseLite.RotationType3D.None);
        noise.SetFrequency(0.01f);
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFractalOctaves(5);
        noise.SetFractalLacunarity(2.0f);
        noise.SetFractalGain(0.5f);
        noise.SetFractalWeightedStrength(0.0f);
        noise.SetFractalPingPongStrength(2.0f);
        noise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.EuclideanSq);
        noise.SetCellularJitter(1.0f);
        noise.SetRotationType3D(FastNoiseLite.RotationType3D.None);
        noise.SetFractalType(FastNoiseLite.FractalType.None);
        return noise;
    }

    public void CreateOffsetTerrainNoise(int seed, out FastNoiseLite noise, out FastNoiseLite distanceNoise)
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
