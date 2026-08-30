using System;
using UnityEngine;

public sealed class HoldPaymentController : MonoBehaviour
{
    [SerializeField] private InputService inputService;
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField, Min(0.01f)] private float secondsPerUnit = 0.35f;

    private IEconomy economy;
    private IPaymentTarget activeTarget;
    private IAtomicPaymentTarget activeAtomicTarget;
    private float elapsed;

    public float Progress01 => Mathf.Clamp01(elapsed / secondsPerUnit);
    public event Action<float> ProgressChanged;
    public event Action<EconomyError> PaymentFailed;

    public void Initialize(IEconomy economyService)
    {
        economy = economyService;
    }

    private void Update()
    {
        IAtomicPaymentTarget atomicTarget = interactor != null ? interactor.Current as IAtomicPaymentTarget : null;
        if (atomicTarget != null)
        {
            UpdateAtomicPayment(atomicTarget);
            return;
        }

        IPaymentTarget target = interactor != null ? interactor.Current as IPaymentTarget : null;
        bool canContinue = inputService != null
            && inputService.InteractHeld
            && target != null
            && target.IsAvailable
            && target.CanAcceptPayment
            && economy != null;

        if (!canContinue)
        {
            CancelChannel();
            return;
        }

        if (!ReferenceEquals(activeTarget, target))
        {
            activeTarget = target;
            elapsed = 0f;
        }

        elapsed += Time.deltaTime;
        ProgressChanged?.Invoke(Progress01);
        while (elapsed >= secondsPerUnit)
        {
            elapsed -= secondsPerUnit;
            if (!TryPayOneUnit(target))
            {
                CancelChannel();
                break;
            }
        }
    }

    private void UpdateAtomicPayment(IAtomicPaymentTarget target)
    {
        bool canContinue = inputService != null && inputService.InteractHeld && target.IsAvailable && target.CanExecute;
        if (!canContinue) { CancelChannel(); return; }
        if (!ReferenceEquals(activeAtomicTarget, target)) { activeAtomicTarget = target; elapsed = 0f; }
        elapsed += Time.deltaTime;
        ProgressChanged?.Invoke(Progress01);
        if (elapsed < secondsPerUnit) return;
        target.TryExecuteAtomic();
        CancelChannel();
    }

    private bool TryPayOneUnit(IPaymentTarget target)
    {
        OperationResult<EconomyError> spend = economy.TrySpend(1);
        if (!spend.Success)
        {
            PaymentFailed?.Invoke(spend.Error);
            return false;
        }

        if (target.TryAcceptUnit())
            return true;

        economy.Add(1);
        return false;
    }

    private void CancelChannel()
    {
        activeTarget = null;
        activeAtomicTarget = null;
        elapsed = 0f;
        ProgressChanged?.Invoke(0f);
    }

    private void OnDisable() => CancelChannel();
}
