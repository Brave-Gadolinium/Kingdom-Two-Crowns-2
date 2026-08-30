using UnityEngine;

public sealed class GreedCrownController : MonoBehaviour
{
    [SerializeField] private GameObject crownVisual;

    private ICrownService crownService;

    public void Initialize(ICrownService service)
    {
        ReleaseService();
        crownService = service;

        if (crownService == null)
            return;

        crownService.GreedCrownChanged += OnCrownChanged;
        OnCrownChanged(crownService.GreedCrown);
    }

    private void OnCrownChanged(CrownSnapshot snapshot)
    {
        if (crownVisual != null)
            crownVisual.SetActive(snapshot.Owner == CrownOwner.Greed);
    }

    private void OnDestroy() => ReleaseService();

    private void ReleaseService()
    {
        if (crownService != null)
            crownService.GreedCrownChanged -= OnCrownChanged;

        crownService = null;
    }
}
