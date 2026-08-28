using NUnit.Framework;

public class GameSessionTests
{
    [Test]
    public void Session_CanAdvanceDay_AndFinish()
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
        Assert.IsTrue(session.IsRunning);

        session.AdvanceDay();

        Assert.AreEqual(2, session.Day);

        session.End(SessionEndReason.Victory);

        Assert.IsFalse(session.IsRunning);

        Assert.AreEqual(
            SessionEndReason.Victory,
            session.EndReason);
    }
}