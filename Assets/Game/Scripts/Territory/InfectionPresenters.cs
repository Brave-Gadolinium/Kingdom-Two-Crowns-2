using UnityEngine;

public sealed class InfectionBoundObject : MonoBehaviour
{
    [SerializeField] private float decayPercentPerSecond = 0.05f;
    public bool IsOperational { get; private set; } = true;
    public float DecayPercentPerSecond => decayPercentPerSecond;

    public void Refresh(IInfectionTerritory territory)
    {
        IsOperational = territory != null && territory.Contains(transform.position.x);
    }
}

public sealed class InfectionVisualPresenter : MonoBehaviour
{
    [SerializeField] private Transform groundLayer;
    [SerializeField] private Transform decorLayer;
    [SerializeField] private Transform fogLayer;
    private IInfectionTerritory territory;

    public void Initialize(IInfectionTerritory source)
    {
        if (territory != null) territory.Changed -= OnChanged;
        territory = source;
        if (territory == null) return;
        territory.Changed += OnChanged;
        Apply(territory.LeftBoundary, territory.RightBoundary);
    }

    private void OnDestroy() { if (territory != null) territory.Changed -= OnChanged; }
    private void OnChanged(TerritoryChanged change) => Apply(change.Left, change.Right);
    private void Apply(float left, float right)
    {
        float width = right - left;
        float center = (left + right) * 0.5f;
        SetLayer(groundLayer, center, width);
        SetLayer(decorLayer, center, width);
        SetLayer(fogLayer, center, width);
    }
    private static void SetLayer(Transform layer, float center, float width)
    {
        if (layer == null) return;
        layer.position = new Vector3(center, layer.position.y, layer.position.z);
        layer.localScale = new Vector3(width, layer.localScale.y, layer.localScale.z);
    }
}
