public sealed class GameSession
{
    private readonly IGameClock clock;
    private readonly IEconomy economy;
    private readonly ITerritoryService territory;

    public bool IsRunning { get; private set; }
    public SessionEndReason EndReason { get; private set; }

    public int Day => clock.Day;

    public GameSession(
        IGameClock clock,
        IEconomy economy,
        ITerritoryService territory)
    {
        this.clock = clock;
        this.economy = economy;
        this.territory = territory;
    }

    public void Start()
    {
        IsRunning = true;
        EndReason = SessionEndReason.None;
    }

    public void AdvanceDay()
    {
        if (!IsRunning)
            return;

        clock.AdvanceDay();
    }

    public void End(SessionEndReason reason)
    {
        if (!IsRunning)
            return;

        IsRunning = false;
        EndReason = reason;
    }
}