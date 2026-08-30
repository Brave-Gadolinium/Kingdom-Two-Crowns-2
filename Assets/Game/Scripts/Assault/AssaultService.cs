using System;
using System.Collections.Generic;

public enum AssaultStartFailure { None, NotNight, NotEnoughTime, NotEnoughFighters, AlreadyActive, NotEnoughGreed }
public enum AssaultState { Idle, Forming, Advancing, Attacking, Returning }
public enum RouteSafety { Safe, Risky, Impossible }

public readonly struct AssaultStartResult
{
    public bool Success { get; }
    public AssaultStartFailure Failure { get; }
    public AssaultStartResult(bool success,AssaultStartFailure failure){Success=success;Failure=failure;}
}

public sealed class AssaultService
{
    public const int Price=6;
    public const int RequiredFighters=4;
    public const float MinimumNightTime=75f;
    public const float SquadSpeed=3.2f;
    private readonly IGreedWallet wallet;
    private readonly PopulationService population;
    private readonly List<ActorId> fighters=new();
    public AssaultState State { get; private set; }
    public IReadOnlyList<ActorId> ReservedFighters=>fighters;
    public bool CommanderExists { get; private set; }
    public event Action Completed;
    public event Action Started;

    public AssaultService(IGreedWallet source,PopulationService populationSource){wallet=source;population=populationSource;}
    public AssaultStartResult TryStart(DayPhase phase,float remainingNight)
    {
        if(State!=AssaultState.Idle)return Fail(AssaultStartFailure.AlreadyActive);
        if(phase!=DayPhase.Night)return Fail(AssaultStartFailure.NotNight);
        if(remainingNight<MinimumNightTime)return Fail(AssaultStartFailure.NotEnoughTime);
        if(!population.TryReserveFighters(RequiredFighters,out ActorId[] reserved))return Fail(AssaultStartFailure.NotEnoughFighters);
        SpendResult spend=wallet.TrySpend(Price,SpendReason.Assault);
        if(!spend.Success){population.ReleaseFighters(reserved);return Fail(AssaultStartFailure.NotEnoughGreed);}
        fighters.AddRange(reserved);CommanderExists=true;State=AssaultState.Forming;Started?.Invoke();return new AssaultStartResult(true,AssaultStartFailure.None);
    }
    public void FormationReady(){if(State==AssaultState.Forming)State=AssaultState.Advancing;}
    public void ReachedTarget(){if(State==AssaultState.Advancing)State=AssaultState.Attacking;}
    public void OnDawn(){if(State!=AssaultState.Idle)State=AssaultState.Returning;}
    public void RegisterFighterDeath(ActorId actor){fighters.Remove(actor);population.RegisterDeath(actor);TryFinishReturn();}
    public void RegisterCommanderDeath(){CommanderExists=false;TryFinishReturn();}
    public void RegisterCommanderReturn(){CommanderExists=false;TryFinishReturn();}
    public void RegisterReturn(ActorId actor){if(State!=AssaultState.Returning||!fighters.Remove(actor))return;population.ReleaseFighters(new[]{actor});TryFinishReturn();}
    public RouteSafety EstimateRoute(float distance,float remainingNight)
    {
        if(remainingNight<MinimumNightTime)return RouteSafety.Impossible;
        float required=Math.Max(0,distance)*2f/SquadSpeed+20f;
        if(required>remainingNight)return RouteSafety.Impossible;
        return required>remainingNight*.75f?RouteSafety.Risky:RouteSafety.Safe;
    }
    private void Finish(){CommanderExists=false;State=AssaultState.Idle;Completed?.Invoke();}
    private void TryFinishReturn(){if(State==AssaultState.Returning&&fighters.Count==0&&!CommanderExists)Finish();}
    private static AssaultStartResult Fail(AssaultStartFailure failure)=>new(false,failure);
}
