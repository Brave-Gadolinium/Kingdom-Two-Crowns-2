using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public sealed class DamageReceiver : MonoBehaviour
{
    private PlayerHealth health;

    public IDamageable Damageable => health;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
    }

    public bool Receive(DamageInfo damage)
    {
        return health != null && health.ApplyDamage(damage);
    }
}
