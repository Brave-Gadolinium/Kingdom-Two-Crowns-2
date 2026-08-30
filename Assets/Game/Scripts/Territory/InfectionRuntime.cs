using UnityEngine;

public sealed class TerritoryInfectionQuery : IInfectionQuery
{
    private readonly IInfectionTerritory territory;
    public TerritoryInfectionQuery(IInfectionTerritory source) => territory = source;
    public bool Contains(float worldX) => territory != null && territory.Contains(worldX);
}

public sealed class InfectionNodeTarget : PlayerInteractable, IPaymentTarget
{
    [SerializeField] private TerritorySide side;
    [SerializeField, Min(1)] private int cost = 5;
    [SerializeField, Min(0.1f)] private float buildDuration = 12f;
    [SerializeField, Min(1)] private int hitPoints = 80;
    private int paid;
    private BuildingModel order;
    private IInfectionTerritory territory;
    private BuildingService buildings;

    public bool CanAcceptPayment => order == null && paid < cost;
    public float PaymentProgress => cost > 0 ? (float)paid / cost : 1f;

    public void Initialize(IInfectionTerritory territoryService, BuildingService buildingService)
    { territory = territoryService; buildings = buildingService; }

    public bool TryAcceptUnit()
    {
        if (!CanAcceptPayment || territory == null || buildings == null) return false;
        float boundary = side == TerritorySide.Left ? territory.LeftBoundary : territory.RightBoundary;
        if (Mathf.Abs(transform.position.x - boundary) > 0.5f) return false;
        paid++;
        if (paid == cost) order = buildings.CreatePaidOrder(BuildingType.InfectionNode, boundary, hitPoints, buildDuration);
        return true;
    }

    public bool ApplyBuilderWork(float seconds)
    {
        if (order == null || order.State == BuildingState.Destroyed || !order.ApplyWork(seconds)) return false;
        if (order.State == BuildingState.Active)
        {
            territory.CompleteNode(order.Id, side);
            enabled = false;
        }
        return true;
    }
}
