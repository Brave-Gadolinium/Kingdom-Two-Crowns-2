using UnityEngine;

[CreateAssetMenu(
    fileName = "MapConfig",
    menuName = "Game/Map/Map Config")]
public class MapConfig : ScriptableObject
{
    [Header("Map")]
    public float mapLength = 500f;
    public float groundY = -2f;

    [Header("Greed")]
    public float greedCaveX = -120f;
    public float heartX = 0f;

    [Header("Human")]
    public float humanOuterWallX = 150f;
    public float humanInnerWallX = 190f;
    public float humanStorageX = 215f;
    public float humanMonarchX = 235f;

    [Header("Zones")]
    public float corruptedZoneEndX = 80f;
    public float humanZoneStartX = 140f;

    [Header("Build sockets")]
    public float wallSocket01X = -40f;
    public float towerSocket01X = -20f;
    public float wallSocket02X = 35f;
    public float towerSocket02X = 55f;

    [Header("NPC points")]
    public float greedWaitingX = -10f;
    public float greedFormationX = 70f;
    public float humanWaitingX = 220f;
    public float humanFormationX = 160f;

    [Header("Visual lifecycle")]
    public float visualDespawnLeftX = -280f;
    public float visualSpawnLeftX = -260f;
    public float visualSpawnRightX = 260f;
    public float visualDespawnRightX = 280f;

    public float MapLeftX => -mapLength * 0.5f;
    public float MapRightX => mapLength * 0.5f;

    private void OnValidate()
    {
        mapLength = Mathf.Max(1f, mapLength);
        humanZoneStartX = Mathf.Max(corruptedZoneEndX, humanZoneStartX);
    }
}
