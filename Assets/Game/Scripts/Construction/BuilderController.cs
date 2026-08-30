using UnityEngine;

public sealed class BuilderController : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float workRate = 1f;
    private InfectionNodeTarget nodeTarget;
    public void Assign(InfectionNodeTarget target) => nodeTarget = target;
    private void Update()
    {
        if (nodeTarget != null && !nodeTarget.ApplyBuilderWork(workRate * Time.deltaTime)) nodeTarget = null;
    }
}
