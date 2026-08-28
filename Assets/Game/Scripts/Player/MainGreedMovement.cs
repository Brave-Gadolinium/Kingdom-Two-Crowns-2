using UnityEngine;
using UnityEngine.InputSystem;

public class MainGreedMovement : MonoBehaviour
{
    [SerializeField]
    private InputActionReference moveAction;

    [SerializeField]
    private float speed = 8f;

    private Rigidbody2D body;
    private float input;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
    }

    private void Update()
    {
        input = moveAction.action.ReadValue<float>();
    }

    private void FixedUpdate()
    {
        body.linearVelocity = new Vector2(
            input * speed,
            body.linearVelocity.y
        );
    }
}