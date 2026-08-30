public interface IInfectionQuery
{
    bool Contains(float worldX);
}

public interface ISunExposureTarget
{
    float WorldX { get; }
    IDamageable Damageable { get; }
    void SetSunWarning(bool active);
}

public sealed class IntervalInfectionQuery : IInfectionQuery
{
    private readonly float left;
    private readonly float right;

    public IntervalInfectionQuery(float left, float right)
    {
        this.left = System.Math.Min(left, right);
        this.right = System.Math.Max(left, right);
    }

    public bool Contains(float worldX) => worldX >= left && worldX <= right;
}
