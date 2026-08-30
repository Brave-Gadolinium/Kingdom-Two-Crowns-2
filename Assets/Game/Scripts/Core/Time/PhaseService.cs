using System;

public readonly struct PhaseChanged
{
    public DayPhase Previous { get; }
    public DayPhase Current { get; }
    public CycleNumber Cycle { get; }

    public PhaseChanged(DayPhase previous, DayPhase current, CycleNumber cycle)
    {
        Previous = previous;
        Current = current;
        Cycle = cycle;
    }
}

public sealed class PhaseService : IGameClock
{
    private readonly float[] durations;

    public int Day { get; private set; } = 1;
    public DayPhase Phase { get; private set; } = DayPhase.Night;
    public float RemainingTime { get; private set; }
    public float PhaseProgress01 => 1f - RemainingTime / GetDuration(Phase);

    public event Action<PhaseChanged> PhaseChanged;

    public PhaseService(float night, float dawn, float day, float dusk)
    {
        durations = new[]
        {
            RequirePositive(night, nameof(night)),
            RequirePositive(dawn, nameof(dawn)),
            RequirePositive(day, nameof(day)),
            RequirePositive(dusk, nameof(dusk))
        };
        RemainingTime = durations[(int)DayPhase.Night];
    }

    public void Tick(float deltaTime)
    {
        if (deltaTime <= 0f)
            return;

        RemainingTime -= deltaTime;
        while (RemainingTime <= 0f)
        {
            float overflow = -RemainingTime;
            AdvancePhase();
            RemainingTime -= overflow;
        }
    }

    public void AdvancePhase()
    {
        DayPhase previous = Phase;
        Phase = (DayPhase)(((int)Phase + 1) % 4);
        if (Phase == DayPhase.Night)
            Day++;

        RemainingTime = GetDuration(Phase);
        PhaseChanged?.Invoke(new PhaseChanged(previous, Phase, new CycleNumber(Day)));
    }

    public void AdvanceDay()
    {
        Day++;
        SetPhase(DayPhase.Night);
    }

    public void SetRemainingTime(float seconds)
    {
        RemainingTime = Math.Clamp(seconds, 0.01f, GetDuration(Phase));
    }

    private void SetPhase(DayPhase phase)
    {
        DayPhase previous = Phase;
        Phase = phase;
        RemainingTime = GetDuration(Phase);
        PhaseChanged?.Invoke(new PhaseChanged(previous, Phase, new CycleNumber(Day)));
    }

    private float GetDuration(DayPhase phase) => durations[(int)phase];

    private static float RequirePositive(float value, string name)
    {
        if (value <= 0f)
            throw new ArgumentOutOfRangeException(name);

        return value;
    }
}
