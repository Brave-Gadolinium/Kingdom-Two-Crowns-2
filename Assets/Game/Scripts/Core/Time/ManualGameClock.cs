public sealed class ManualGameClock : IGameClock
{
    public int Day { get; private set; } = 1;

    public DayPhase Phase { get; private set; } = DayPhase.Night;

    public void AdvanceDay()
    {
        Day++;
        Phase = DayPhase.Night;
    }
}