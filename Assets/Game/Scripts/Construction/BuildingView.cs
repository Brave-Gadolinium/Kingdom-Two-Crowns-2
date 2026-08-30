using UnityEngine;

public sealed class BuildingView : MonoBehaviour
{
    [SerializeField] private GameObject constructionVisual;
    [SerializeField] private GameObject activeVisual;
    [SerializeField] private GameObject damagedVisual;
    [SerializeField] private GameObject destroyedVisual;
    [SerializeField] private Collider2D blockingCollider;

    public void Present(BuildingState state)
    {
        Set(constructionVisual, state == BuildingState.Construction);
        Set(activeVisual, state == BuildingState.Active);
        Set(damagedVisual, state == BuildingState.Damaged || state == BuildingState.Disabled);
        Set(destroyedVisual, state == BuildingState.Destroyed);
        if (blockingCollider != null)
            blockingCollider.enabled = state == BuildingState.Active || state == BuildingState.Damaged;
    }
    private static void Set(GameObject target, bool active) { if (target != null) target.SetActive(active); }
}
