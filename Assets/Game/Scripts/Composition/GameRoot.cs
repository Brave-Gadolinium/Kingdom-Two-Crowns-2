using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRoot : MonoBehaviour
{
    private static GameRoot activeInstance;
    [SerializeField] private GameBalanceConfig balance;

    public GameSession Session { get; private set; }
    public PhaseService Phase { get; private set; }
    public IEconomy Economy { get; private set; }
    public ITerritoryService Territory { get; private set; }
    public ICrownService Crowns { get; private set; }
    public GreedWallet Wallet { get; private set; }
    public InfectionTerritory Infection { get; private set; }
    public HumanTreasury HumanTreasury { get; private set; }
    public PopulationService Population { get; private set; }
    public BuildingService Buildings { get; private set; }
    public SettlementService Settlement { get; private set; }
    public RaidPlanner RaidPlanner { get; private set; }
    public AssaultService Assault { get; private set; }

    private void Awake()
    {
        if (activeInstance != null && activeInstance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (!TryValidateBalance())
        {
            enabled = false;
            return;
        }

        activeInstance = this;
        DontDestroyOnLoad(gameObject);
        CreateGame();
        SceneManager.sceneLoaded += OnSceneLoaded;
        InitializeActiveWorld();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Phase != null)
            Phase.PhaseChanged -= OnPhaseChanged;
        if (activeInstance == this)
            activeInstance = null;
    }

    private bool TryValidateBalance()
    {
        if (balance == null)
        {
            Debug.LogError("GameRoot: не назначен GameBalanceConfig.", this);
            return false;
        }

        var errors = balance.ValidateConfig();
        if (errors.Count == 0)
            return true;

        Debug.LogError(
            "GameRoot: запуск с некорректным балансом остановлен:\n" + string.Join("\n", errors),
            this);
        return false;
    }

    private void CreateGame()
    {
        Phase = new PhaseService(
            balance.time.nightDuration,
            balance.time.dawnDuration,
            balance.time.dayDuration,
            balance.time.duskDuration);
        Wallet = new GreedWallet(balance.economy.startingGreed);
        Infection = new InfectionTerritory(0f, balance.territory.startingSize, balance.territory.expansionSize);
        HumanTreasury = new HumanTreasury(25, 30);
        Population = new PopulationService();
        Buildings = new BuildingService(Infection);
        Settlement = new SettlementService(HumanTreasury);
        RaidPlanner = new RaidPlanner();
        Assault = new AssaultService(Wallet, Population);
        Economy = Wallet;
        Territory = Infection;
        Crowns = new CrownService();

        Session = new GameSession(Phase, Economy, Territory);
        Phase.PhaseChanged += OnPhaseChanged;
        Settlement.BeginNight();
        Session.Start();
    }

    private void OnPhaseChanged(PhaseChanged change)
    {
        if (change.Current == DayPhase.Night)
            Settlement.BeginNight();
        else if (change.Current == DayPhase.Dawn)
            Assault.OnDawn();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => InitializeActiveWorld();

    private void InitializeActiveWorld()
    {
        WorldCompositionRoot world = FindAnyObjectByType<WorldCompositionRoot>();
        if (world != null)
            world.Initialize(this);
    }
}
