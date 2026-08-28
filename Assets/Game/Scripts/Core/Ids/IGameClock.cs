public interface IGameClock
{
    int Day { get; }
    DayPhase Phase { get; }

    void AdvanceDay();
}