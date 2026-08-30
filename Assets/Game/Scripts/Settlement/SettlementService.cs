using System;

public readonly struct SettlementTierRules
{
    public SettlementTier Tier { get; }
    public int UpgradePrice { get; }
    public int Income { get; }
    public int StorageCapacity { get; }
    public int RaidBudget { get; }
    public SettlementTierRules(SettlementTier tier, int price, int income, int capacity, int raidBudget)
    { Tier=tier;UpgradePrice=price;Income=income;StorageCapacity=capacity;RaidBudget=raidBudget; }
}

public readonly struct SettlementSnapshot
{
    public SettlementTier Tier { get; }
    public int GoldStorage { get; }
    public int Capacity { get; }
    public int RaidBudget { get; }
    public SettlementSnapshot(SettlementTier tier,int gold,int capacity,int raidBudget)
    { Tier=tier;GoldStorage=gold;Capacity=capacity;RaidBudget=raidBudget; }
}

public readonly struct SettlementChanged
{
    public SettlementSnapshot Snapshot { get; }
    public bool TierChanged { get; }
    public SettlementChanged(SettlementSnapshot snapshot,bool tierChanged){Snapshot=snapshot;TierChanged=tierChanged;}
}

public sealed class SettlementService
{
    private static readonly SettlementTierRules[] Rules =
    {
        new(SettlementTier.Camp,0,3,30,3),
        new(SettlementTier.Village,21,5,55,7),
        new(SettlementTier.FortifiedVillage,45,7,90,12),
        new(SettlementTier.City,80,10,140,18),
        new(SettlementTier.Castle,130,14,220,28)
    };
    private readonly HumanTreasury treasury;
    public SettlementTier Tier { get; private set; }
    public HumanTreasury Treasury => treasury;
    public SettlementSnapshot Snapshot => new(Tier,treasury.StoredGold,treasury.Capacity,CurrentRules.RaidBudget);
    public event Action<SettlementChanged> Changed;
    private SettlementTierRules CurrentRules => Rules[(int)Tier];

    public SettlementService(HumanTreasury source, SettlementTier startingTier=SettlementTier.Camp)
    { treasury=source??throw new ArgumentNullException(nameof(source));Tier=startingTier;treasury.SetCapacity(CurrentRules.StorageCapacity); }

    public bool BeginNight()
    {
        treasury.Produce(CurrentRules.Income);
        bool upgraded=TryUpgradeOnce();
        Changed?.Invoke(new SettlementChanged(Snapshot,upgraded));
        return upgraded;
    }

    private bool TryUpgradeOnce()
    {
        if(Tier==SettlementTier.Castle)return false;
        SettlementTier next=(SettlementTier)((int)Tier+1);
        int price=Rules[(int)next].UpgradePrice;
        if(!treasury.TrySpendExact(price))return false;
        Tier=next;
        treasury.SetCapacity(CurrentRules.StorageCapacity);
        return true;
    }
}
