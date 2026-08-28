using UnityEngine;

public class GameRoot : MonoBehaviour
{
    [SerializeField]
    private GameBalanceConfig balance;

    public GameSession Session { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        CreateGame();
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