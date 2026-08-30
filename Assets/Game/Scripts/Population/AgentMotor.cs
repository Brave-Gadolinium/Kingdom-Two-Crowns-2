using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class AgentMotor : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 4.5f;
    private Rigidbody2D body;
    private float direction;
    private void Awake() => body = GetComponent<Rigidbody2D>();
    public void Move(float normalizedDirection) => direction = Mathf.Clamp(normalizedDirection, -1f, 1f);
    private void FixedUpdate() { if (body != null) body.linearVelocity = new Vector2(direction * speed, body.linearVelocity.y); }
}
