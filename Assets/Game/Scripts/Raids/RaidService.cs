using System;
using System.Collections.Generic;

public enum RaidState { Idle, Deploying, Attacking, Retreating }
public enum RaidTargetKind { GreedWall, EyeTower, OtherBuilding, Heart, MainGreed, Retreat }
public enum RaidUnitLifecycle { Inactive, Spawning, Alive, Dying, Despawned }

public sealed class RaidParticipant
{
    public int RuntimeId { get; }
    public HumanUnitType Type { get; }
    public RaidUnitLifecycle Lifecycle { get; internal set; }
    public RaidTargetKind Target { get; internal set; }
    public RaidParticipant(int id,HumanUnitType type){RuntimeId=id;Type=type;Lifecycle=RaidUnitLifecycle.Spawning;Target=RaidTargetKind.GreedWall;}
}

public interface IRaidUnitPool
{
    RaidParticipant Rent(HumanUnitType type);
    void Return(RaidParticipant participant);
}

public sealed class RaidService
{
    private readonly IRaidUnitPool pool;
    private readonly List<RaidParticipant> active=new();
    private readonly Queue<HumanUnitType> pending=new();
    private float spawnTimer;
    private readonly float spawnInterval;
    public RaidState State { get; private set; }
    public int ActiveCount=>active.Count;
    public int PendingCount=>pending.Count;
    public IReadOnlyList<RaidParticipant> ActiveParticipants=>active;
    public event Action Completed;

    public RaidService(IRaidUnitPool source,float interval=.35f){pool=source??throw new ArgumentNullException(nameof(source));spawnInterval=Math.Max(.01f,interval);}
    public bool Start(RaidPlan plan)
    {
        if(State!=RaidState.Idle||plan==null)return false;
        foreach(HumanUnitType unit in plan.Units)pending.Enqueue(unit);
        State=RaidState.Deploying;spawnTimer=0;return true;
    }
    public void Tick(float deltaTime)
    {
        if(State==RaidState.Idle||deltaTime<=0)return;
        if(State==RaidState.Deploying||State==RaidState.Attacking)
        {
            spawnTimer-=deltaTime;
            while(spawnTimer<=0&&pending.Count>0&&active.Count<RaidPlanner.AbsoluteUnitLimit)
            {RaidParticipant unit=pool.Rent(pending.Dequeue());unit.Lifecycle=RaidUnitLifecycle.Alive;active.Add(unit);spawnTimer+=spawnInterval;}
            if(pending.Count==0)State=RaidState.Attacking;
        }
        TryComplete();
    }
    public void OnWallDestroyed(){foreach(RaidParticipant unit in active)unit.Target=RaidTargetKind.EyeTower;}
    public void BeginRetreat(){if(State==RaidState.Idle)return;State=RaidState.Retreating;pending.Clear();foreach(RaidParticipant unit in active)unit.Target=RaidTargetKind.Retreat;TryComplete();}
    public bool RemoveParticipant(int runtimeId)
    {
        int index=active.FindIndex(unit=>unit.RuntimeId==runtimeId);if(index<0)return false;
        RaidParticipant participant=active[index];active.RemoveAt(index);participant.Lifecycle=RaidUnitLifecycle.Despawned;pool.Return(participant);TryComplete();return true;
    }
    private void TryComplete()
    {
        if(active.Count>0||pending.Count>0||State==RaidState.Idle)return;
        State=RaidState.Idle;Completed?.Invoke();
    }
}
