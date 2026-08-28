using NUnit.Framework;

public class GameSessionTests
{
    [Test]
    public void Session_CanAdvanceFullCycle_AndFinish()
    {
        var clock = new ManualGameClock();

        var economy = new InMemoryEconomy(10);

        var territory =
            new LinearTerritoryService(25f);

        var session =
            new GameSession(
                clock,
                economy,
                territory);

        session.Start();

        Assert.AreEqual(1, session.Day);
        Assert.AreEqual(DayPhase.Night, session.Phase);
        Assert.IsTrue(session.IsRunning);

        session.AdvancePhase();
        Assert.AreEqual(DayPhase.Dawn, session.Phase);

        session.AdvancePhase();
        Assert.AreEqual(DayPhase.Day, session.Phase);

        session.AdvancePhase();
        Assert.AreEqual(DayPhase.Dusk, session.Phase);

        session.AdvancePhase();

        Assert.AreEqual(2, session.Day);
        Assert.AreEqual(DayPhase.Night, session.Phase);

        session.End(SessionEndReason.Victory);

        Assert.IsFalse(session.IsRunning);

        Assert.AreEqual(
            SessionEndReason.Victory,
            session.EndReason);

        session.AdvancePhase();

        Assert.AreEqual(2, session.Day);
        Assert.AreEqual(DayPhase.Night, session.Phase);
    }

    [Test]
    public void Economy_RejectsInvalidOrUnaffordableSpend()
    {
        var economy = new InMemoryEconomy(12);

        var invalid = economy.TrySpend(0);
        var unaffordable = economy.TrySpend(13);
        var valid = economy.TrySpend(6);

        Assert.IsFalse(invalid.Success);
        Assert.AreEqual(EconomyError.InvalidAmount, invalid.Error);
        Assert.IsFalse(unaffordable.Success);
        Assert.AreEqual(EconomyError.NotEnoughGreed, unaffordable.Error);
        Assert.IsTrue(valid.Success);
        Assert.AreEqual(6, economy.Greed);
    }

    [Test]
    public void GameIds_CompareByValueAndType()
    {
        Assert.AreEqual(new ActorId(7), new ActorId(7));
        Assert.AreNotEqual(new ActorId(7), new ActorId(8));
        Assert.AreNotEqual((object)new ActorId(7), new BuildingId(7));
    }
}
