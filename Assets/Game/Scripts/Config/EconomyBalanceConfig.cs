using UnityEngine;

[CreateAssetMenu(
    fileName = "EconomyBalance",
    menuName = "Game/Balance/Economy")]
public class EconomyBalanceConfig : ScriptableObject
{
    [Min(0)] public int startingGreed = 12;
    [Min(1)] public int recruitCost = 1;
    [Min(1)] public int wallCost = 6;
    [Min(1)] public int towerCost = 8;
}
