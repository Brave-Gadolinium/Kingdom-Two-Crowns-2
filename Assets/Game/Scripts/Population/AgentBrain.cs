using UnityEngine;

public sealed class AgentBrain : MonoBehaviour
{
    [SerializeField] private GreedRole role;
    public GreedRole Role => role;
    public void SetRole(GreedRole value) => role = value;
}
