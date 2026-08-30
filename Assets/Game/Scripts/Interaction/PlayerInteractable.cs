using UnityEngine;

public abstract class PlayerInteractable : MonoBehaviour, IPlayerInteractable
{
    public Transform InteractionPoint => transform;
    public virtual bool IsAvailable => isActiveAndEnabled;

    protected virtual void OnEnable() => InteractableRegistry.Register(this);
    protected virtual void OnDisable() => InteractableRegistry.Unregister(this);
}
