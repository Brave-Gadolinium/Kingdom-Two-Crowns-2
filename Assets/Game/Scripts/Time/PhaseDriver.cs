using UnityEngine;

public sealed class PhaseDriver : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float debugRemainingTime = 10f;

    public PhaseService Service { get; private set; }

    public void Initialize(PhaseService service)
    {
        Service = service;
    }

    private void Update()
    {
        Service?.Tick(Time.deltaTime);
    }

    public void DebugNextPhase()
    {
        if (Debug.isDebugBuild || Application.isEditor)
            Service?.AdvancePhase();
    }

    public void DebugSetRemainingTime()
    {
        if (Debug.isDebugBuild || Application.isEditor)
            Service?.SetRemainingTime(debugRemainingTime);
    }
}
