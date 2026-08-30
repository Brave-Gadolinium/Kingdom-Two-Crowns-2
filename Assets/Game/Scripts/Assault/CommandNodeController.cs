using UnityEngine;

public sealed class CommandNodeController : PlayerInteractable, IAtomicPaymentTarget
{
    [SerializeField] private CommandNodePresenter presenter;
    [SerializeField] private float targetDistance=165f;
    private GameRoot gameRoot;
    public bool CanExecute => gameRoot != null && gameRoot.Assault.State == AssaultState.Idle;
    public void Initialize(GameRoot root){gameRoot=root;Refresh();}
    public AssaultStartResult TryLaunch()
    {
        if(gameRoot==null)return new AssaultStartResult(false,AssaultStartFailure.NotNight);
        AssaultStartResult result=gameRoot.Assault.TryStart(gameRoot.Phase.Phase,gameRoot.Phase.RemainingTime);
        Refresh(result.Failure);return result;
    }
    public bool TryExecuteAtomic() => TryLaunch().Success;
    public void Refresh(AssaultStartFailure failure=AssaultStartFailure.None)
    {
        if(gameRoot==null||presenter==null)return;
        presenter.Present(gameRoot.Assault.EstimateRoute(targetDistance,gameRoot.Phase.RemainingTime),failure);
    }
}
