using System.Collections.Generic;
using UnityEngine;

public sealed class SunExposureService
{
    private sealed class ExposureState
    {
        public float ExposedTime;
        public float DamageRemainder;
        public bool Warning;
    }

    private readonly PhaseService phaseService;
    private readonly IInfectionQuery infection;
    private readonly float maximumDamagePerSecond;
    private readonly float graceDuration;
    private readonly Dictionary<ISunExposureTarget, ExposureState> targets = new();
    private readonly List<ISunExposureTarget> iterationBuffer = new();

    public SunExposureService(
        PhaseService phaseService,
        IInfectionQuery infection,
        float maximumDamagePerSecond,
        float graceDuration)
    {
        this.phaseService = phaseService;
        this.infection = infection;
        this.maximumDamagePerSecond = Mathf.Max(0f, maximumDamagePerSecond);
        this.graceDuration = Mathf.Max(0f, graceDuration);
    }

    public void Register(ISunExposureTarget target)
    {
        if (target != null && !targets.ContainsKey(target))
            targets.Add(target, new ExposureState());
    }

    public void Unregister(ISunExposureTarget target)
    {
        if (target == null)
            return;

        if (targets.Remove(target))
            target.SetSunWarning(false);
    }

    public void Tick(float deltaTime)
    {
        if (deltaTime <= 0f)
            return;

        iterationBuffer.Clear();
        iterationBuffer.AddRange(targets.Keys);

        foreach (ISunExposureTarget target in iterationBuffer)
        {
            if (target == null || !targets.TryGetValue(target, out ExposureState state))
                continue;

            bool exposed = phaseService.Phase != DayPhase.Night
                && !infection.Contains(target.WorldX)
                && target.Damageable != null
                && target.Damageable.IsAlive;

            SetWarning(target, state, exposed);
            if (!exposed)
            {
                state.ExposedTime = 0f;
                state.DamageRemainder = 0f;
                continue;
            }

            float previousExposedTime = state.ExposedTime;
            state.ExposedTime += deltaTime;
            float damagingDelta = Mathf.Max(0f, state.ExposedTime - graceDuration)
                - Mathf.Max(0f, previousExposedTime - graceDuration);
            if (damagingDelta <= 0f)
                continue;

            state.DamageRemainder += GetDamageRate() * damagingDelta;
            int damage = Mathf.FloorToInt(state.DamageRemainder);
            if (damage <= 0)
                continue;

            state.DamageRemainder -= damage;
            target.Damageable.ApplyDamage(
                new DamageInfo(damage, DamageKind.Sun, new Vector2(target.WorldX, 0f)));
        }
    }

    private float GetDamageRate()
    {
        return phaseService.Phase switch
        {
            DayPhase.Dawn => maximumDamagePerSecond * phaseService.PhaseProgress01,
            DayPhase.Day => maximumDamagePerSecond,
            DayPhase.Dusk => maximumDamagePerSecond * (1f - phaseService.PhaseProgress01),
            _ => 0f
        };
    }

    private static void SetWarning(
        ISunExposureTarget target,
        ExposureState state,
        bool active)
    {
        if (state.Warning == active)
            return;

        state.Warning = active;
        target.SetSunWarning(active);
    }
}
