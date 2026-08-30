using System;
using System.Collections.Generic;
using UnityEngine;

public enum SpendReason { Recruit, ProfessionTool, InfectionNode, GreedWall, EyeTower, CommandNode, Assault }
public enum SpendFailure { None, InvalidAmount, NotEnoughCarried, CapacityReached }

public readonly struct SpendResult
{
    public bool Success { get; }
    public int Spent { get; }
    public SpendFailure Failure { get; }
    public SpendResult(bool success, int spent, SpendFailure failure)
    { Success = success; Spent = spent; Failure = failure; }
}

public readonly struct WalletChanged
{
    public int Carried { get; }
    public int Reserve { get; }
    public WalletChanged(int carried, int reserve) { Carried = carried; Reserve = reserve; }
}

public interface IGreedWallet
{
    int Carried { get; }
    int Reserve { get; }
    int Capacity { get; }
    bool CanSpend(int amount);
    SpendResult TrySpend(int amount, SpendReason reason);
    int AddCarried(int amount);
    int DepositGold(int amount, ActorId source);
    int WithdrawReserve(int amount);
    event Action<WalletChanged> Changed;
}

public sealed class GreedWallet : IGreedWallet, IEconomy
{
    private readonly Dictionary<SpendReason, int> expenses = new();
    public int Carried { get; private set; }
    public int Reserve { get; private set; }
    public int Capacity { get; }
    public int Greed => Carried;
    public IReadOnlyDictionary<SpendReason, int> Expenses => expenses;
    public event Action<WalletChanged> Changed;

    public GreedWallet(int startingCarried, int capacity = 99, int startingReserve = 0)
    {
        Capacity = Math.Max(1, capacity);
        Carried = Math.Clamp(startingCarried, 0, Capacity);
        Reserve = Math.Max(0, startingReserve);
    }

    public bool CanSpend(int amount) => amount > 0 && Carried >= amount;
    public SpendResult TrySpend(int amount, SpendReason reason)
    {
        if (amount <= 0) return new SpendResult(false, 0, SpendFailure.InvalidAmount);
        if (Carried < amount) return new SpendResult(false, 0, SpendFailure.NotEnoughCarried);
        Carried -= amount;
        expenses[reason] = expenses.TryGetValue(reason, out int total) ? total + amount : amount;
        Notify();
        return new SpendResult(true, amount, SpendFailure.None);
    }

    public int AddCarried(int amount)
    {
        if (amount <= 0) return 0;
        int accepted = Math.Min(amount, Capacity - Carried);
        Carried += accepted;
        if (accepted > 0) Notify();
        return accepted;
    }

    public int DepositGold(int amount, ActorId source)
    {
        if (amount <= 0) return 0;
        Reserve += amount;
        Notify();
        return amount;
    }

    public int WithdrawReserve(int amount)
    {
        if (amount <= 0) return 0;
        int moved = Math.Min(Math.Min(amount, Reserve), Capacity - Carried);
        Reserve -= moved;
        Carried += moved;
        if (moved > 0) Notify();
        return moved;
    }

    public void Add(int amount) => AddCarried(amount);
    public OperationResult<EconomyError> TrySpend(int amount)
    {
        SpendResult result = TrySpend(amount, SpendReason.Assault);
        return result.Success ? OperationResult<EconomyError>.Ok() :
            OperationResult<EconomyError>.Fail(result.Failure == SpendFailure.InvalidAmount ? EconomyError.InvalidAmount : EconomyError.NotEnoughGreed);
    }
    private void Notify() => Changed?.Invoke(new WalletChanged(Carried, Reserve));
}

public sealed class EconomyHudPresenter : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text carriedLabel;
    [SerializeField] private TMPro.TMP_Text reserveLabel;
    [SerializeField] private TMPro.TMP_Text statusLabel;
    [SerializeField] private UnityEngine.UI.Image paymentProgress;
    private IGreedWallet wallet;
    private HoldPaymentController payment;

    public void Initialize(IGreedWallet source, HoldPaymentController paymentSource)
    {
        Release();
        wallet = source;
        payment = paymentSource;
        if (wallet != null) { wallet.Changed += ShowWallet; ShowWallet(new WalletChanged(wallet.Carried, wallet.Reserve)); }
        if (payment != null) { payment.ProgressChanged += ShowPaymentProgress; payment.PaymentFailed += ShowPaymentFailure; }
    }

    public void ShowWallet(WalletChanged value)
    {
        if (carriedLabel != null) carriedLabel.text = $"Жадность: {value.Carried}";
        if (reserveLabel != null) reserveLabel.text = $"Сердце: {value.Reserve}";
    }
    public void ShowFailure(SpendFailure failure) { if (statusLabel != null) statusLabel.text = failure == SpendFailure.NotEnoughCarried ? "Недостаточно Жадности" : string.Empty; }
    public void ShowPaymentProgress(float progress) { if (paymentProgress != null) paymentProgress.fillAmount = Mathf.Clamp01(progress); }
    private void ShowPaymentFailure(EconomyError failure) { if (statusLabel != null) statusLabel.text = failure == EconomyError.NotEnoughGreed ? "Недостаточно Жадности" : string.Empty; }
    private void OnDestroy() => Release();
    private void Release()
    {
        if (wallet != null) wallet.Changed -= ShowWallet;
        if (payment != null) { payment.ProgressChanged -= ShowPaymentProgress; payment.PaymentFailed -= ShowPaymentFailure; }
        wallet = null; payment = null;
    }
}
