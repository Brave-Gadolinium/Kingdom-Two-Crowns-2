using UnityEngine;

public sealed class GathererVisualPresenter : MonoBehaviour
{
    [SerializeField] private GameObject emptyBag;
    [SerializeField] private GameObject fullBag;
    [SerializeField] private Animator animator;

    public void Present(GathererState state, int carriedGold)
    {
        if (emptyBag != null) emptyBag.SetActive(carriedGold == 0);
        if (fullBag != null) fullBag.SetActive(carriedGold > 0);
        if (animator != null)
        {
            animator.SetBool("Stealing", state == GathererState.Steal);
            animator.SetBool("Fleeing", state == GathererState.Return);
            animator.SetBool("Depositing", state == GathererState.Deposit);
        }
    }
}
