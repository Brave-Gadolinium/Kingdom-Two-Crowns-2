using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerMotor2D : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 6f;

    private Rigidbody2D body;
    private float moveIntent;

    public float MoveIntent => moveIntent;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void SetMoveIntent(float value)
    {
        moveIntent = Mathf.Clamp(value, -1f, 1f);
    }

    private void FixedUpdate()
    {
        if (body == null)
            return;

        body.linearVelocity = new Vector2(moveIntent * speed, body.linearVelocity.y);
    }
}
