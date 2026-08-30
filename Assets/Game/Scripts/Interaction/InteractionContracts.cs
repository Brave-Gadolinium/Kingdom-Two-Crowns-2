using UnityEngine;

public interface IPlayerInteractable
{
    Transform InteractionPoint { get; }
    bool IsAvailable { get; }
}

public interface IPaymentTarget : IPlayerInteractable
{
    bool CanAcceptPayment { get; }
    bool TryAcceptUnit();
}

public interface IAtomicPaymentTarget : IPlayerInteractable
{
    bool CanExecute { get; }
    bool TryExecuteAtomic();
}
