public interface ITerritoryService
{
    float Size { get; }

    bool Contains(float worldX);

    void Expand(float amount);
}
