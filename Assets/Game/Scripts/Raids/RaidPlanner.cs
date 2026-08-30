using System;
using System.Collections.Generic;

public enum HumanUnitType { Peasant, Archer, Soldier, Knight, Worker }

public readonly struct RaidUnitOrder
{
    public HumanUnitType Type { get; }
    public int Count { get; }
    public RaidUnitOrder(HumanUnitType type,int count){Type=type;Count=count;}
}

public sealed class RaidPlan
{
    public int Seed { get; }
    public int Budget { get; }
    public IReadOnlyList<HumanUnitType> Units { get; }
    public RaidPlan(int seed,int budget,List<HumanUnitType> units){Seed=seed;Budget=budget;Units=units;}
}

public sealed class RaidPlanner
{
    public const int AbsoluteUnitLimit=20;
    private static readonly Dictionary<HumanUnitType,int> Costs=new()
    { [HumanUnitType.Peasant]=1,[HumanUnitType.Archer]=2,[HumanUnitType.Soldier]=3,[HumanUnitType.Knight]=6,[HumanUnitType.Worker]=2 };

    public RaidPlan CreatePlan(SettlementSnapshot settlement,CycleNumber cycle,int deterministicSeed)
    {
        int seed=unchecked(deterministicSeed*397^cycle.Value*31^(int)settlement.Tier);
        var random=new Random(seed);
        var units=Mandatory(settlement.Tier);
        int spent=0;foreach(HumanUnitType unit in units)spent+=Costs[unit];
        int effectiveBudget=Math.Max(settlement.RaidBudget,spent);
        HumanUnitType[] allowed=Allowed(settlement.Tier);
        while(units.Count<AbsoluteUnitLimit)
        {
            var candidates=new List<HumanUnitType>();
            foreach(HumanUnitType unit in allowed)if(spent+Costs[unit]<=effectiveBudget)candidates.Add(unit);
            if(candidates.Count==0)break;
            HumanUnitType selected=candidates[random.Next(candidates.Count)];
            units.Add(selected);spent+=Costs[selected];
        }
        return new RaidPlan(seed,effectiveBudget,units);
    }

    private static List<HumanUnitType> Mandatory(SettlementTier tier)
    {
        var result=new List<HumanUnitType>();
        void Add(HumanUnitType type,int count){for(int i=0;i<count;i++)result.Add(type);}
        if(tier==SettlementTier.Camp)Add(HumanUnitType.Peasant,3);
        else if(tier==SettlementTier.Village){Add(HumanUnitType.Peasant,3);Add(HumanUnitType.Archer,2);}
        else if(tier==SettlementTier.FortifiedVillage){Add(HumanUnitType.Archer,2);Add(HumanUnitType.Soldier,2);Add(HumanUnitType.Peasant,2);}
        else if(tier==SettlementTier.City){Add(HumanUnitType.Archer,4);Add(HumanUnitType.Soldier,2);Add(HumanUnitType.Knight,1);}
        else {Add(HumanUnitType.Archer,4);Add(HumanUnitType.Soldier,4);Add(HumanUnitType.Knight,2);}
        return result;
    }

    private static HumanUnitType[] Allowed(SettlementTier tier)
    {
        if(tier==SettlementTier.Camp)return new[]{HumanUnitType.Peasant};
        if(tier==SettlementTier.Village)return new[]{HumanUnitType.Peasant,HumanUnitType.Archer};
        if(tier==SettlementTier.FortifiedVillage)return new[]{HumanUnitType.Peasant,HumanUnitType.Archer,HumanUnitType.Soldier};
        return new[]{HumanUnitType.Peasant,HumanUnitType.Archer,HumanUnitType.Soldier,HumanUnitType.Knight};
    }
}
