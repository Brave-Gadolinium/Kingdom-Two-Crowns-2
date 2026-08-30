using UnityEngine;

[CreateAssetMenu(
    fileName = "TimeBalance",
    menuName = "Game/Balance/Time")]
public class TimeBalanceConfig : ScriptableObject
{
    [Min(1f)] public float nightDuration = 240f;
    [Min(1f)] public float dawnDuration = 20f;
    [Min(1f)] public float dayDuration = 120f;
    [Min(1f)] public float duskDuration = 20f;

    [Header("Sun")]
    [Min(0f)] public float sunDamagePerSecond = 8f;
    [Min(0f)] public float sunGraceDuration = 2f;
}
