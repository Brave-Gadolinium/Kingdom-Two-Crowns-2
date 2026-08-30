using System;
using System.Collections.Generic;

public enum BuildingState { Construction, Active, Damaged, Disabled, Destroyed }

public sealed class BuildingModel
{
    public BuildingId Id { get; }
    public BuildingType Type { get; }
    public float WorldX { get; }
    public int MaxHealth { get; }
    public int Health { get; private set; }
    public BuildingState State { get; private set; }
    public float WorkRemaining { get; private set; }
    public BuildingModel(BuildingId id, BuildingType type, float worldX, int health, float work)
    { Id=id; Type=type; WorldX=worldX; MaxHealth=Math.Max(1,health); Health=1; WorkRemaining=Math.Max(0.01f,work); State=BuildingState.Construction; }
    public bool ApplyWork(float amount)
    {
        if (State == BuildingState.Destroyed || State == BuildingState.Disabled || amount <= 0) return false;
        if (State == BuildingState.Construction) { WorkRemaining=Math.Max(0,WorkRemaining-amount); if(WorkRemaining<=0){Health=MaxHealth;State=BuildingState.Active;} return true; }
        if (Health >= MaxHealth) return false;
        Health=Math.Min(MaxHealth,Health+(int)Math.Ceiling(amount));State=Health==MaxHealth?BuildingState.Active:BuildingState.Damaged;return true;
    }
    public void Damage(int amount) { if(amount<=0||State==BuildingState.Destroyed)return;Health=Math.Max(0,Health-amount);State=Health==0?BuildingState.Destroyed:BuildingState.Damaged; }
    public void SetTerritoryActive(bool active) { if(State==BuildingState.Destroyed)return;if(!active)State=BuildingState.Disabled;else if(State==BuildingState.Disabled)State=Health>=MaxHealth?BuildingState.Active:BuildingState.Damaged; }
}

public sealed class BuildingService
{
    private readonly List<BuildingModel> buildings=new();
    private readonly IInfectionTerritory territory;
    private int nextId=1;
    public IReadOnlyList<BuildingModel> Buildings=>buildings;
    public BuildingService(IInfectionTerritory territorySource){territory=territorySource;if(territory!=null)territory.Changed+=OnTerritoryChanged;}
    public BuildingModel CreatePaidOrder(BuildingType type,float worldX,int hp,float buildSeconds)
    { if(territory==null||!territory.CanPlace(type,worldX,out _))return null;var b=new BuildingModel(new BuildingId(nextId++),type,worldX,hp,buildSeconds);buildings.Add(b);return b; }
    public BuildingModel SelectBuilderTask(bool isDay)
    {
        BuildingModel best=null;int bestPriority=int.MaxValue;
        foreach(var b in buildings){if(b.State==BuildingState.Destroyed||b.State==BuildingState.Disabled)continue;if(isDay&&!territory.Contains(b.WorldX))continue;int p=b.State==BuildingState.Construction?4:b.State==BuildingState.Damaged?5:int.MaxValue;if(p<bestPriority||(p==bestPriority&&best!=null&&b.Id.Value<best.Id.Value)){best=b;bestPriority=p;}}
        return best;
    }
    private void OnTerritoryChanged(TerritoryChanged change){foreach(var b in buildings)b.SetTerritoryActive(territory.Contains(b.WorldX));}
}
