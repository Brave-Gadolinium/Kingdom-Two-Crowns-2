public interface ITerritoryService
{
    float Size { get; }

    void Expand(float amount);
}