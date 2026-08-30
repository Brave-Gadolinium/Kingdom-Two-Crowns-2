using NUnit.Framework;

public sealed class PhaseServiceTests
{
    [Test]
    public void Tick_AdvancesThroughAllPhases_AndStartsNextDay()
    {
        var service = new PhaseService(4f, 2f, 3f, 1f);
        service.Tick(10f);

        Assert.That(service.Phase, Is.EqualTo(DayPhase.Night));
        Assert.That(service.Day, Is.EqualTo(2));
        Assert.That(service.RemainingTime, Is.EqualTo(4f).Within(0.001f));
    }

    [Test]
    public void Tick_IsIndependentFromFrameStep()
    {
        var lowFps = new PhaseService(4f, 2f, 3f, 1f);
        var highFps = new PhaseService(4f, 2f, 3f, 1f);

        for (int i = 0; i < 30; i++)
            lowFps.Tick(1f / 30f);

        for (int i = 0; i < 144; i++)
            highFps.Tick(1f / 144f);

        Assert.That(lowFps.Phase, Is.EqualTo(highFps.Phase));
        Assert.That(lowFps.RemainingTime, Is.EqualTo(highFps.RemainingTime).Within(0.001f));
    }
}
