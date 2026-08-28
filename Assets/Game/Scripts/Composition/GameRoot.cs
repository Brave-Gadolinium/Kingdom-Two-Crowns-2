using UnityEngine;

public class GameRoot : MonoBehaviour
{
    private static GameRoot activeInstance;

    [SerializeField]
    private GameBalanceConfig balance;

    public GameSession Session { get; private set; }

    private void Awake()
    {
        if (activeInstance != null && activeInstance != this)
        {
            // В каждый момент времени существует только один корень игровой сессии.
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
    }

    private void OnDestroy()
    {
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
            "GameRoot: запуск с некорректным балансом остановлен:\n" +
            string.Join("\n", errors),
            this);

        return false;
    }

    private void CreateGame()
    {
        var clock = new ManualGameClock();

        var economy =
            new InMemoryEconomy(
                balance.economy.startingGreed);

        var territory =
            new LinearTerritoryService(
                balance.territory.startingSize);

        Session = new GameSession(
            clock,
            economy,
            territory);

        Session.Start();
    }
}
