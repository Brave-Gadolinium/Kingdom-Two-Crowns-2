using System.Collections.Generic;
using NUnit.Framework;

public sealed class SettlementRaidAssaultTests
{
    [Test]
    public void Settlement_ThirtyCycles_FollowsPricesWithoutSkippingOrRegression()
    {
        var settlement=new SettlementService(new HumanTreasury(12,30));
        var reached=new List<SettlementTier>{settlement.Tier};
        for(int cycle=1;cycle<=30;cycle++)
        {
            SettlementTier before=settlement.Tier;
            if(settlement.BeginNight())reached.Add(settlement.Tier);
            Assert.That(settlement.Tier,Is.GreaterThanOrEqualTo(before));
        }
        Assert.That(reached,Is.EqualTo(new[]{SettlementTier.Camp,SettlementTier.Village,SettlementTier.FortifiedVillage,SettlementTier.City}));
        Assert.That(settlement.Tier,Is.EqualTo(SettlementTier.City));
    }

    [Test]
    public void Settlement_ReachesCastleStrictlyByTable_AndRobberyOnlyDelaysProgress()
    {
        var untouched=new SettlementService(new HumanTreasury(12,30));
        var robbed=new SettlementService(new HumanTreasury(12,30));
        for(int cycle=1;cycle<=45;cycle++)
        {
            untouched.BeginNight();
            SettlementTier before=robbed.Tier;
            robbed.BeginNight();
            if(cycle%3==0)
            {
                var actor=new ActorId(cycle);
                robbed.Treasury.TryReserveGold(actor,2);
                robbed.Treasury.ConfirmTaken(actor);
            }
            Assert.That(robbed.Tier,Is.GreaterThanOrEqualTo(before));
        }
        Assert.That(untouched.Tier,Is.EqualTo(SettlementTier.Castle));
        Assert.That(robbed.Tier,Is.LessThanOrEqualTo(untouched.Tier));
    }

    [TestCase(SettlementTier.Camp,3)]
    [TestCase(SettlementTier.Village,5)]
    [TestCase(SettlementTier.FortifiedVillage,6)]
    [TestCase(SettlementTier.City,7)]
    [TestCase(SettlementTier.Castle,10)]
    public void RaidPlanner_ContainsMandatoryThreat_AndNeverExceedsLimit(SettlementTier tier,int mandatoryCount)
    {
        var planner=new RaidPlanner();
        var snapshot=new SettlementSnapshot(tier,0,220,tier==SettlementTier.Castle?28:18);
        RaidPlan first=planner.CreatePlan(snapshot,new CycleNumber(7),12345);
        RaidPlan second=planner.CreatePlan(snapshot,new CycleNumber(7),12345);
        Assert.That(first.Units.Count,Is.GreaterThanOrEqualTo(mandatoryCount));
        Assert.That(first.Units.Count,Is.LessThanOrEqualTo(20));
        Assert.That(second.Units,Is.EqualTo(first.Units));
    }

    [Test]
    public void RaidService_ThirtyRaids_ReturnEveryParticipantAndSubscription()
    {
        var pool=new FakeRaidPool();
        var service=new RaidService(pool,.01f);
        var planner=new RaidPlanner();
        int completions=0;service.Completed+=()=>completions++;
        var snapshot=new SettlementSnapshot(SettlementTier.Castle,0,220,28);
        for(int raid=1;raid<=30;raid++)
        {
            Assert.That(service.Start(planner.CreatePlan(snapshot,new CycleNumber(raid),99)),Is.True);
            for(int i=0;i<100&&service.PendingCount>0;i++)service.Tick(.1f);
            var ids=new List<int>();foreach(RaidParticipant unit in service.ActiveParticipants)ids.Add(unit.RuntimeId);
            service.BeginRetreat();foreach(int id in ids)Assert.That(service.RemoveParticipant(id),Is.True);
            Assert.That(service.State,Is.EqualTo(RaidState.Idle));
        }
        Assert.That(completions,Is.EqualTo(30));
        Assert.That(pool.Rented,Is.EqualTo(pool.Returned));
    }

    [Test]
    public void Assault_StartIsAtomic_AndReturnedFightersBecomeAvailable()
    {
        var population=new PopulationService();
        for(int i=1;i<=4;i++){population.RegisterRecruit(new ActorId(i));Assign(population,i,GreedRole.Fighter);}
        var wallet=new GreedWallet(6,20);
        var assault=new AssaultService(wallet,population);
        Assert.That(assault.TryStart(DayPhase.Night,74).Failure,Is.EqualTo(AssaultStartFailure.NotEnoughTime));
        Assert.That(wallet.Carried,Is.EqualTo(6));
        Assert.That(assault.TryStart(DayPhase.Night,75).Success,Is.True);
        Assert.That(wallet.Carried,Is.Zero);
        Assert.That(population.AvailableFighters,Is.Zero);
        var reserved=new List<ActorId>(assault.ReservedFighters);
        assault.OnDawn();foreach(ActorId fighter in reserved)assault.RegisterReturn(fighter);assault.RegisterCommanderReturn();
        Assert.That(assault.State,Is.EqualTo(AssaultState.Idle));
        Assert.That(assault.CommanderExists,Is.False);
        Assert.That(population.AvailableFighters,Is.EqualTo(4));
    }

    private static void Assign(PopulationService population,int actor,GreedRole role)
    {var tool=population.OrderTool(role);population.TryReserveNearest(tool.Id,new ActorId(actor));population.TryCollectTool(tool.Id,new ActorId(actor));}

    private sealed class FakeRaidPool:IRaidUnitPool
    {
        private int next=1;public int Rented{get;private set;}public int Returned{get;private set;}
        public RaidParticipant Rent(HumanUnitType type){Rented++;return new RaidParticipant(next++,type);}
        public void Return(RaidParticipant participant){Returned++;}
    }
}
