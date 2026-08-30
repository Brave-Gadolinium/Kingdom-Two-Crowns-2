using UnityEngine;

public sealed class RecruitTarget : PlayerInteractable, IPaymentTarget
{
    [SerializeField, Min(1)] private int cost = 1;
    [SerializeField] private int actorId = 1;
    private int paid;
    private PopulationService population;
    private FormlessCave cave;
    public bool CanAcceptPayment => paid < cost;
    public void Initialize(PopulationService service, FormlessCave source) { population = service; cave = source; }
    public bool TryAcceptUnit()
    {
        if (!CanAcceptPayment || population == null || cave == null) return false;
        paid++;
        if (paid < cost) return true;
        if (!cave.TryTake() || !population.RegisterRecruit(new ActorId(actorId))) { paid--; return false; }
        enabled = false; return true;
    }
}

public sealed class ProfessionToolOrderTarget : PlayerInteractable, IPaymentTarget
{
    [SerializeField] private GreedRole role = GreedRole.Fighter;
    [SerializeField, Min(1)] private int cost = 2;
    private int paid;
    private PopulationService population;
    public ProfessionTool LastOrderedTool { get; private set; }
    public bool CanAcceptPayment => population != null && paid < cost;
    public float PaymentProgress => (float)paid / cost;
    public void Initialize(PopulationService service) => population = service;
    public bool TryAcceptUnit()
    {
        if (!CanAcceptPayment) return false;
        paid++;
        if (paid == cost) { LastOrderedTool = population.OrderTool(role); paid = 0; }
        return true;
    }
}
