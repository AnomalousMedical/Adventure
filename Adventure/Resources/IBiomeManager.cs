namespace Adventure
{
    interface IBiomeManager
    {
        int Count { get; }

        IBiome GetBiome(int index);
    }
}