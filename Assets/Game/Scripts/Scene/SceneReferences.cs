using UnityEngine;

public class SceneReferences : MonoBehaviour
{
    [Header("Config")]
    [SerializeField]
    private MapConfig mapConfig;

    [Header("Map")]
    public Transform ground;
    public Transform groundVisual;
    public Transform corruptedZone;
    public Transform neutralZone;
    public Transform humanZone;

    [Header("Greed")]
    public Transform greedCavePoint;
    public Transform heartPoint;

    [Header("Human")]
    public Transform humanOuterWallPoint;
    public Transform humanInnerWallPoint;
    public Transform humanStoragePoint;
    public Transform humanMonarchPoint;

    [Header("Build sockets")]
    public Transform wallSocket01;
    public Transform towerSocket01;
    public Transform wallSocket02;
    public Transform towerSocket02;

    [Header("NPC points")]
    public Transform greedWaitingPoint;
    public Transform greedFormationPoint;
    public Transform humanWaitingPoint;
    public Transform humanFormationPoint;

    [Header("Visual lifecycle")]
    public Transform visualDespawnLeft;
    public Transform visualSpawnLeft;
    public Transform visualSpawnRight;
    public Transform visualDespawnRight;

    [Header("Runtime")]
    public CameraFollow2D followCamera;

    private void Awake()
    {
        if (!HasRequiredReferences())
        {
            Debug.LogError("SceneReferences: не назначены обязательные ссылки сцены.", this);
            enabled = false;
            return;
        }

        ApplyConfig();
    }

    private void OnValidate()
    {
        if (mapConfig != null)
            ApplyConfig();
    }

    private void ApplyConfig()
    {
        float surfaceY = mapConfig.groundY;

        SetPosition(groundVisual, 0f, surfaceY - 1f);
        if (groundVisual != null)
            groundVisual.localScale = new Vector3(mapConfig.mapLength, 2f, 1f);

        ConfigureZone(corruptedZone, mapConfig.MapLeftX, mapConfig.corruptedZoneEndX, surfaceY);
        ConfigureZone(neutralZone, mapConfig.corruptedZoneEndX, mapConfig.humanZoneStartX, surfaceY);
        ConfigureZone(humanZone, mapConfig.humanZoneStartX, mapConfig.MapRightX, surfaceY);

        SetPosition(greedCavePoint, mapConfig.greedCaveX, surfaceY);
        SetPosition(heartPoint, mapConfig.heartX, surfaceY);
        SetPosition(humanOuterWallPoint, mapConfig.humanOuterWallX, surfaceY);
        SetPosition(humanInnerWallPoint, mapConfig.humanInnerWallX, surfaceY);
        SetPosition(humanStoragePoint, mapConfig.humanStorageX, surfaceY);
        SetPosition(humanMonarchPoint, mapConfig.humanMonarchX, surfaceY);

        SetPosition(wallSocket01, mapConfig.wallSocket01X, surfaceY);
        SetPosition(towerSocket01, mapConfig.towerSocket01X, surfaceY);
        SetPosition(wallSocket02, mapConfig.wallSocket02X, surfaceY);
        SetPosition(towerSocket02, mapConfig.towerSocket02X, surfaceY);

        SetPosition(greedWaitingPoint, mapConfig.greedWaitingX, surfaceY);
        SetPosition(greedFormationPoint, mapConfig.greedFormationX, surfaceY);
        SetPosition(humanWaitingPoint, mapConfig.humanWaitingX, surfaceY);
        SetPosition(humanFormationPoint, mapConfig.humanFormationX, surfaceY);

        SetPosition(visualDespawnLeft, mapConfig.visualDespawnLeftX, surfaceY);
        SetPosition(visualSpawnLeft, mapConfig.visualSpawnLeftX, surfaceY);
        SetPosition(visualSpawnRight, mapConfig.visualSpawnRightX, surfaceY);
        SetPosition(visualDespawnRight, mapConfig.visualDespawnRightX, surfaceY);

        if (followCamera != null)
            followCamera.ConfigureBounds(mapConfig.MapLeftX, mapConfig.MapRightX);
    }

    private bool HasRequiredReferences()
    {
        return mapConfig != null
            && ground != null
            && groundVisual != null
            && corruptedZone != null
            && neutralZone != null
            && humanZone != null
            && greedCavePoint != null
            && heartPoint != null
            && humanOuterWallPoint != null
            && humanInnerWallPoint != null
            && humanStoragePoint != null
            && humanMonarchPoint != null
            && wallSocket01 != null
            && towerSocket01 != null
            && wallSocket02 != null
            && towerSocket02 != null
            && greedWaitingPoint != null
            && greedFormationPoint != null
            && humanWaitingPoint != null
            && humanFormationPoint != null
            && visualDespawnLeft != null
            && visualSpawnLeft != null
            && visualSpawnRight != null
            && visualDespawnRight != null
            && followCamera != null;
    }

    private static void ConfigureZone(Transform zone, float left, float right, float surfaceY)
    {
        if (zone == null)
            return;

        const float zoneHeight = 10f;
        SetPosition(zone, (left + right) * 0.5f, surfaceY + zoneHeight * 0.5f, -1f);
        zone.localScale = new Vector3(right - left, zoneHeight, 1f);
    }

    private static void SetPosition(Transform target, float x, float y, float z = 0f)
    {
        if (target != null)
            target.position = new Vector3(x, y, z);
    }
}
