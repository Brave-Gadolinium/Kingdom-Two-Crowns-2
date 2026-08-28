using UnityEngine;

[CreateAssetMenu(
    fileName = "TerritoryBalance",
    menuName = "Game/Balance/Territory")]
public class TerritoryBalanceConfig : ScriptableObject
{
    [Min(1f)] public float startingSize = 25f;
    [Min(1f)] public float expansionSize = 20f;
    [Min(1)] public int expansionCost = 5;
}