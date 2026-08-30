using UnityEngine;

[RequireComponent(typeof(DamageReceiver))]
public sealed class SunExposureAgent : MonoBehaviour, ISunExposureTarget
{
    [SerializeField] private SunExposureCoordinator coordinator;
    [SerializeField] private SpriteRenderer visual;
    [SerializeField, Min(0.1f)] private float warningPulseSpeed = 8f;

    private DamageReceiver receiver;
    private bool warning;
    private bool started;
    private Color baseColor = Color.white;

    public float WorldX => transform.position.x;
    public IDamageable Damageable => receiver != null ? receiver.Damageable : null;

    private void Awake()
    {
        receiver = GetComponent<DamageReceiver>();
        if (visual != null)
            baseColor = visual.color;
    }

    private void OnEnable()
    {
        if (started)
            coordinator?.Register(this);
    }

    private void Start()
    {
        started = true;
        coordinator?.Register(this);
    }

    private void OnDisable()
    {
        coordinator?.Unregister(this);
        SetSunWarning(false);
    }

    private void Update()
    {
        if (!warning || visual == null)
            return;

        float pulse = 0.55f + Mathf.Sin(Time.time * warningPulseSpeed) * 0.25f;
        visual.color = new Color(baseColor.r, baseColor.g, baseColor.b, pulse);
    }

    public void SetSunWarning(bool active)
    {
        warning = active;
        if (!active && visual != null)
            visual.color = baseColor;
    }
}
