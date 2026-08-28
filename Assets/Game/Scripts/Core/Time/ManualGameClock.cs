public sealed class ManualGameClock : IGameClock
{
    public int Day { get; private set; } = 1;

    public DayPhase Phase { get; private set; } = DayPhase.Night;

    public void AdvancePhase()
    {
        switch (Phase)
        {
            case DayPhase.Night:
                Phase = DayPhase.Dawn;
                break;
            case DayPhase.Dawn:
                Phase = DayPhase.Day;
                break;
            case DayPhase.Day:
                Phase = DayPhase.Dusk;
                break;
            case DayPhase.Dusk:
                Day++;
                Phase = DayPhase.Night;
                break;
        }
    }

    public void AdvanceDay()
    {
        Day++;
        Phase = DayPhase.Night;
    }
}
