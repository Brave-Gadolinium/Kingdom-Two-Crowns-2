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

    public bool Contains(float worldX)
    {
        return worldX >= -Size * 0.5f && worldX <= Size * 0.5f;
    }
}
