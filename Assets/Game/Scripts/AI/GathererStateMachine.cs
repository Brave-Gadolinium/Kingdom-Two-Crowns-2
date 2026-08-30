using System;

public enum GathererState { Idle, EvaluateTrip, GoToTreasury, Steal, Return, Deposit }

public sealed class GathererStateMachine
{
    private readonly ActorId actor;
    private readonly HumanTreasury treasury;
    private readonly IGreedWallet wallet;
    private readonly float speed;
    private readonly float safetySeconds;
    private readonly float stealSeconds;
    private readonly int capacity;
    private float heartX;
    private float treasuryX;
    private float actionRemaining;

    public GathererState State { get; private set; } = GathererState.Idle;
    public int CarriedGold { get; private set; }
    public float PositionX { get; private set; }

    public GathererStateMachine(ActorId actorId, HumanTreasury source, IGreedWallet targetWallet,
        float startX, float targetX, float moveSpeed = 4.8f, float safety = 20f, float stealTime = 1.5f, int carryCapacity = 2)
    { actor=actorId;treasury=source;wallet=targetWallet;heartX=startX;treasuryX=targetX;PositionX=startX;speed=Math.Max(.1f,moveSpeed);safetySeconds=Math.Max(0,safety);stealSeconds=Math.Max(.01f,stealTime);capacity=Math.Max(1,carryCapacity); }

    public bool Evaluate(float nightRemaining)
    {
        State=GathererState.EvaluateTrip;
        float travel=(Math.Abs(treasuryX-heartX)*2f)/speed;
        if(nightRemaining<travel+stealSeconds*capacity+safetySeconds){State=GathererState.Idle;return false;}
        GoldReservation reservation=treasury.TryReserveGold(actor,capacity);
        if(!reservation.Success){State=GathererState.Idle;return false;}
        actionRemaining=stealSeconds*reservation.Amount;State=GathererState.GoToTreasury;return true;
    }

    public void Tick(float deltaTime,bool dawnOrThreat,bool targetExists=true)
    {
        if(deltaTime<=0)return;
        if(!targetExists&&(State==GathererState.GoToTreasury||State==GathererState.Steal)){treasury.CancelReservation(actor);State=GathererState.Return;}
        if(dawnOrThreat&&(State==GathererState.GoToTreasury||State==GathererState.Steal)){treasury.CancelReservation(actor);State=GathererState.Return;}
        if(State==GathererState.GoToTreasury){PositionX=Move(PositionX,treasuryX,speed*deltaTime);if(PositionX==treasuryX)State=GathererState.Steal;}
        else if(State==GathererState.Steal){actionRemaining-=deltaTime;if(actionRemaining<=0){CarriedGold+=treasury.ConfirmTaken(actor);State=GathererState.Return;}}
        else if(State==GathererState.Return){PositionX=Move(PositionX,heartX,speed*deltaTime);if(PositionX==heartX)State=GathererState.Deposit;}
        else if(State==GathererState.Deposit){wallet.DepositGold(CarriedGold,actor);CarriedGold=0;State=GathererState.Idle;}
    }

    public int Die()
    {
        treasury.CancelReservation(actor);int dropped=CarriedGold;CarriedGold=0;State=GathererState.Idle;return dropped;
    }

    private static float Move(float from,float to,float distance)
    { if(Math.Abs(to-from)<=distance)return to;return from+Math.Sign(to-from)*distance; }
}
