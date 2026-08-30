using UnityEngine;

public sealed class PlayerInteractor : MonoBehaviour
{
    [SerializeField, Min(0f)] private float interactionRange = 3f;

    public IPlayerInteractable Current { get; private set; }

    private void Update()
    {
        Current = FindNearest();
    }

    private IPlayerInteractable FindNearest()
    {
        IPlayerInteractable nearest = null;
        float bestDistanceSquared = interactionRange * interactionRange;
        Vector3 position = transform.position;

        foreach (IPlayerInteractable candidate in InteractableRegistry.All)
        {
            if (candidate == null || !candidate.IsAvailable || candidate.InteractionPoint == null)
                continue;

            float distanceSquared = (candidate.InteractionPoint.position - position).sqrMagnitude;
            if (distanceSquared > bestDistanceSquared)
                continue;

            bestDistanceSquared = distanceSquared;
            nearest = candidate;
        }

        return nearest;
    }
}
