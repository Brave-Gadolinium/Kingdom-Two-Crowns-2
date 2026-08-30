using UnityEngine;

public abstract class CurrencyPickup : MonoBehaviour
{
    [SerializeField, Min(1)] protected int amount = 1;
    private bool consumed;
    protected bool IsConsumed => consumed;
    protected bool TryConsume() { if (consumed) return false; consumed = true; gameObject.SetActive(false); return true; }
}

public sealed class GoldPickup : CurrencyPickup
{
    public int CollectGold() => TryConsume() ? amount : 0;
}

public sealed class GreedCrystalPickup : CurrencyPickup
{
    public int Collect(IGreedWallet wallet)
    {
        if (IsConsumed || wallet == null || wallet.AddCarried(amount) != amount) return 0;
        return TryConsume() ? amount : 0;
    }
}
