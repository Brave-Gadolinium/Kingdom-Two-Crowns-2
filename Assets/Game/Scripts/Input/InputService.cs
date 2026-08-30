using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputService : MonoBehaviour, IInputService
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference interactAction;

    public float MoveIntent { get; private set; }
    public bool InteractHeld { get; private set; }

    private bool IsConfigured => moveAction != null && interactAction != null;

    private void Awake()
    {
        if (IsConfigured)
            return;

        Debug.LogError("InputService: не назначены действия Move и Interact.", this);
        enabled = false;
    }

    private void OnEnable()
    {
        if (!IsConfigured)
            return;

        moveAction.action.Enable();
        interactAction.action.Enable();
    }

    private void OnDisable()
    {
        if (IsConfigured)
        {
            moveAction.action.Disable();
            interactAction.action.Disable();
        }
        MoveIntent = 0f;
        InteractHeld = false;
    }

    private void Update()
    {
        if (!IsConfigured)
            return;

        MoveIntent = Mathf.Clamp(moveAction.action.ReadValue<float>(), -1f, 1f);
        InteractHeld = interactAction.action.IsPressed();
    }
}
