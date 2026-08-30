using NUnit.Framework;
using UnityEngine;

public sealed class SunExposureServiceTests
{
    private sealed class FakeDamageable : IDamageable
    {
        public ActorId Id => new ActorId(1);
        public int CurrentHealth { get; private set; } = 100;
        public int MaxHealth => 100;
        public bool IsAlive => CurrentHealth > 0;

        public bool ApplyDamage(DamageInfo damage)
        {
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage.Amount);
            return true;
        }
    }

    private sealed class FakeTarget : ISunExposureTarget
    {
        public float WorldX { get; set; }
        public IDamageable Damageable { get; } = new FakeDamageable();
        public bool Warning { get; private set; }

        public void SetSunWarning(bool active) => Warning = active;
    }

    [Test]
    public void TargetInsideInfection_IsSafeDuringDay()
    {
        PhaseService phase = CreateDayPhase();
        var service = new SunExposureService(phase, new IntervalInfectionQuery(-10f, 10f), 8f, 2f);
        var target = new FakeTarget { WorldX = 0f };
        service.Register(target);

        service.Tick(10f);

        Assert.That(target.Damageable.CurrentHealth, Is.EqualTo(100));
        Assert.That(target.Warning, Is.False);
    }

    [Test]
    public void TargetOutside_WarnsBeforeFirstDamage_ThenReceivesExpectedDamage()
    {
        PhaseService phase = CreateDayPhase();
        var service = new SunExposureService(phase, new IntervalInfectionQuery(-10f, 10f), 8f, 2f);
        var target = new FakeTarget { WorldX = 20f };
        service.Register(target);

        service.Tick(1f);
        Assert.That(target.Warning, Is.True);
        Assert.That(target.Damageable.CurrentHealth, Is.EqualTo(100));

        service.Tick(2f);
        Assert.That(target.Damageable.CurrentHealth, Is.EqualTo(92));
    }

    private static PhaseService CreateDayPhase()
    {
        var phase = new PhaseService(1f, 1f, 100f, 1f);
        phase.AdvancePhase();
        phase.AdvancePhase();
        return phase;
    }
}
