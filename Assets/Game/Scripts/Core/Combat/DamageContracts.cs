using UnityEngine;

public enum DamageKind
{
    Light,
    Normal,
    Heavy,
    Sun
}

public readonly struct DamageInfo
{
    public int Amount { get; }
    public DamageKind Kind { get; }
    public Vector2 HitPoint { get; }

    public DamageInfo(int amount, DamageKind kind, Vector2 hitPoint)
    {
        Amount = Mathf.Max(0, amount);
        Kind = kind;
        HitPoint = hitPoint;
    }
}

public interface IDamageable
{
    ActorId Id { get; }
    int CurrentHealth { get; }
    int MaxHealth { get; }
    bool IsAlive { get; }
    bool ApplyDamage(DamageInfo damage);
}
