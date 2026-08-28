using UnityEngine;

[CreateAssetMenu(
    fileName = "EconomyBalance",
    menuName = "Game/Balance/Economy")]
public class EconomyBalanceConfig : ScriptableObject
{
    [Min(0)] public int startingGreed = 10;
    [Min(1)] public int recruitCost = 1;
    [Min(1)] public int wallCost = 5;
    [Min(1)] public int towerCost = 7;
}