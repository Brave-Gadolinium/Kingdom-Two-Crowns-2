using UnityEngine;

public class WorldCompositionRoot : MonoBehaviour
{
    [SerializeField] private PhaseDriver phaseDriver;
    [SerializeField] private HoldPaymentController paymentController;
    [SerializeField] private GreedCrownController crownController;
    [SerializeField] private SunExposureCoordinator sunExposureCoordinator;
    [SerializeField] private InfectionVisualPresenter infectionVisualPresenter;
    [SerializeField] private InfectionNodeTarget[] infectionNodeTargets;
    [SerializeField] private EconomyHudPresenter economyHud;
    [SerializeField] private SettlementPresenter settlementPresenter;
    [SerializeField] private RaidRuntimeController raidRuntimeController;
    [SerializeField] private CommandNodeController commandNodeController;
    [SerializeField] private AssaultRuntimeController assaultRuntimeController;

    private bool initialized;

    public void Initialize(GameRoot gameRoot)
    {
        if (initialized || gameRoot == null)
            return;

        initialized = true;
        phaseDriver?.Initialize(gameRoot.Phase);
        paymentController?.Initialize(gameRoot.Economy);
        crownController?.Initialize(gameRoot.Crowns);
        sunExposureCoordinator?.Initialize(gameRoot.Infection);
        infectionVisualPresenter?.Initialize(gameRoot.Infection);
        if (infectionNodeTargets != null)
            foreach (InfectionNodeTarget node in infectionNodeTargets)
                node?.Initialize(gameRoot.Infection, gameRoot.Buildings);
        economyHud?.Initialize(gameRoot.Wallet, paymentController);
        settlementPresenter?.Initialize(gameRoot.Settlement);
        raidRuntimeController?.Initialize(gameRoot);
        commandNodeController?.Initialize(gameRoot);
        assaultRuntimeController?.Initialize(gameRoot);
    }
}
