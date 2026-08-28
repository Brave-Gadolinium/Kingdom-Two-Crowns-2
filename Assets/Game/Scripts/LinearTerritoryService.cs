public sealed class LinearTerritoryService : ITerritoryService
{
    public float Size { get; private set; }

    public LinearTerritoryService(float startingSize)
    {
        Size = startingSize;
    }

    public void Expand(float amount)
    {
        if (amount > 0)
            Size += amount;
    }
}