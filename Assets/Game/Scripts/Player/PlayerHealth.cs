using System;
using UnityEngine;

public sealed class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1)] private int maxHealth = 100;
    [SerializeField] private int actorId = 1;

    public ActorId Id => new ActorId(actorId);
    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool IsAlive => CurrentHealth > 0;

    public event Action<DamageInfo> Damaged;
    public event Action Defeated;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public bool ApplyDamage(DamageInfo damage)
    {
        if (!IsAlive || damage.Amount <= 0)
            return false;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage.Amount);
        Damaged?.Invoke(damage);

        if (!IsAlive)
            Defeated?.Invoke();

        return true;
    }

    public void Restore(int amount)
    {
        if (amount > 0 && IsAlive)
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
    }

    public void ResetHealth() => CurrentHealth = maxHealth;

    public void Configure(int id, int health)
    {
        actorId = id;
        maxHealth = Mathf.Max(1, health);
        CurrentHealth = maxHealth;
    }
}
