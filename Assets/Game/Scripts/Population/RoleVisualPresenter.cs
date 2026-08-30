using UnityEngine;

public sealed class RoleVisualPresenter : MonoBehaviour
{
    [SerializeField] private GameObject formlessVisual;
    [SerializeField] private GameObject fighterVisual;
    [SerializeField] private GameObject gathererVisual;
    [SerializeField] private GameObject builderVisual;

    public void Present(GreedRole role)
    {
        Set(formlessVisual, role == GreedRole.Formless);
        Set(fighterVisual, role == GreedRole.Fighter);
        Set(gathererVisual, role == GreedRole.Gatherer);
        Set(builderVisual, role == GreedRole.Builder);
    }
    private static void Set(GameObject target, bool active) { if (target != null) target.SetActive(active); }
}
