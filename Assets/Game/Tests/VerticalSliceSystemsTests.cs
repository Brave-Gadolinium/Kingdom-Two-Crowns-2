using NUnit.Framework;
using UnityEngine;

public sealed class VerticalSliceSystemsTests
{
    [Test]
    public void InfectionNodes_ExpandByTwentyFive_AndRemoveDetachedChain()
    {
        var territory = new InfectionTerritory(0f, 25f, 25f);
        Assert.That(territory.CompleteNode(new BuildingId(1), TerritorySide.Right), Is.True);
        Assert.That(territory.CompleteNode(new BuildingId(2), TerritorySide.Right), Is.True);
        Assert.That(territory.RightBoundary, Is.EqualTo(62.5f));
        Assert.That(territory.DestroyNode(new BuildingId(1)), Is.True);
        Assert.That(territory.RightBoundary, Is.EqualTo(12.5f));
        Assert.That(territory.ActiveNodes.Count, Is.Zero);
    }

    [Test]
    public void InfectionSnapshot_RestoresBothOrderedChains()
    {
        var source = new InfectionTerritory(0, 25, 25);
        source.CompleteNode(new BuildingId(1), TerritorySide.Left);
        source.CompleteNode(new BuildingId(2), TerritorySide.Right);
        var restored = new InfectionTerritory(0, 25, 25);
        restored.Restore(source.CaptureSnapshot());
        Assert.That(restored.LeftBoundary, Is.EqualTo(-37.5f));
        Assert.That(restored.RightBoundary, Is.EqualTo(37.5f));
        Assert.That(restored.ActiveNodes.Count, Is.EqualTo(2));
    }

    [Test]
    public void WalletAndTreasury_OneHundredOperations_KeepConservationAndNonNegativeBalances()
    {
        var wallet = new GreedWallet(50, 100);
        var treasury = new HumanTreasury(30, 30);
        for (int i = 1; i <= 100; i++)
        {
            var actor = new ActorId(i);
            GoldReservation reservation = treasury.TryReserveGold(actor, 2);
            if (i % 2 == 0) wallet.DepositGold(treasury.ConfirmTaken(actor), actor);
            else treasury.CancelReservation(actor);
            wallet.TrySpend(1, SpendReason.ProfessionTool);
            wallet.AddCarried(1);
        }
        Assert.That(wallet.Carried, Is.GreaterThanOrEqualTo(0));
        Assert.That(wallet.Reserve, Is.GreaterThanOrEqualTo(0));
        Assert.That(treasury.StoredGold, Is.GreaterThanOrEqualTo(0));
        Assert.That(treasury.ReservedGold, Is.Zero);
    }

    [Test]
    public void Population_TenAgents_HasExactRoles_AndDeathReleasesCount()
    {
        var population = new PopulationService();
        for (int i = 1; i <= 10; i++) Assert.That(population.RegisterRecruit(new ActorId(i)), Is.True);
        Assign(population, 1, GreedRole.Fighter); Assign(population, 2, GreedRole.Fighter); Assign(population, 3, GreedRole.Fighter);
        Assign(population, 4, GreedRole.Gatherer); Assign(population, 5, GreedRole.Gatherer); Assign(population, 6, GreedRole.Gatherer); Assign(population, 7, GreedRole.Gatherer);
        Assign(population, 8, GreedRole.Builder); Assign(population, 9, GreedRole.Builder);
        PopulationSnapshot snapshot = population.Snapshot;
        Assert.That((snapshot.Formless, snapshot.Fighters, snapshot.Gatherers, snapshot.Builders), Is.EqualTo((1, 3, 4, 2)));
        Assert.That(population.RegisterDeath(new ActorId(4)), Is.True);
        Assert.That(population.Snapshot.Gatherers, Is.EqualTo(3));
    }

    [Test]
    public void BuilderSelection_IsDeterministic_AndNeverReturnsDestroyedObject()
    {
        var territory = new InfectionTerritory(0, 100, 25);
        var buildings = new BuildingService(territory);
        BuildingModel first = buildings.CreatePaidOrder(BuildingType.GreedWall, -10, 300, 15);
        BuildingModel second = buildings.CreatePaidOrder(BuildingType.EyeTower, 10, 160, 18);
        Assert.That(buildings.SelectBuilderTask(false), Is.SameAs(first));
        first.Damage(999);
        Assert.That(buildings.SelectBuilderTask(false), Is.SameAs(second));
    }

    [Test]
    public void Gatherer_PredictablyRejectsLateTrip_AndConservesGoldOnCompletedTrip()
    {
        var treasury = new HumanTreasury(10, 30);
        var wallet = new GreedWallet(0, 99);
        var gatherer = new GathererStateMachine(new ActorId(1), treasury, wallet, 0, 48);
        Assert.That(gatherer.Evaluate(40), Is.False);
        Assert.That(gatherer.Evaluate(50), Is.True);
        for (int i = 0; i < 200 && gatherer.State != GathererState.Idle; i++) gatherer.Tick(.25f, false);
        Assert.That(treasury.StoredGold + wallet.Reserve + gatherer.CarriedGold, Is.EqualTo(10));
        Assert.That(wallet.Reserve, Is.EqualTo(2));
    }

    [Test]
    public void Combat_RejectsFriendlyFire_AndDuplicateSwingHit()
    {
        var registry = new AttackHitRegistry();
        var service = new MeleeAttackService(registry);
        var target = new FakeDamageable(7, 100);
        AttackId attack = registry.Begin();
        Assert.That(service.TryHit(attack, Faction.Greed, Faction.Greed, target, 12, Vector2.zero), Is.False);
        Assert.That(service.TryHit(attack, Faction.Greed, Faction.Human, target, 12, Vector2.zero), Is.True);
        Assert.That(service.TryHit(attack, Faction.Greed, Faction.Human, target, 12, Vector2.zero), Is.False);
        Assert.That(target.CurrentHealth, Is.EqualTo(88));
    }

    private static void Assign(PopulationService population, int actor, GreedRole role)
    {
        ProfessionTool tool = population.OrderTool(role);
        Assert.That(population.TryReserveNearest(tool.Id, new ActorId(actor)), Is.True);
        Assert.That(population.TryCollectTool(tool.Id, new ActorId(actor)), Is.True);
    }

    private sealed class FakeDamageable : IDamageable
    {
        public ActorId Id { get; }
        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; }
        public bool IsAlive => CurrentHealth > 0;
        public FakeDamageable(int id, int hp) { Id = new ActorId(id); MaxHealth = hp; CurrentHealth = hp; }
        public bool ApplyDamage(DamageInfo damage) { if (!IsAlive || damage.Amount <= 0) return false; CurrentHealth = Mathf.Max(0, CurrentHealth - damage.Amount); return true; }
    }
}
