using System;
using System.Collections.Generic;

public enum GreedRole { Formless, Fighter, Gatherer, Builder }
public enum ToolState { Available, Reserved, Carried, Dropped, Expired }

public readonly struct PopulationSnapshot
{
    public int Total { get; }
    public int Formless { get; }
    public int Fighters { get; }
    public int Gatherers { get; }
    public int Builders { get; }
    public PopulationSnapshot(int total, int formless, int fighters, int gatherers, int builders)
    { Total = total; Formless = formless; Fighters = fighters; Gatherers = gatherers; Builders = builders; }
}

public sealed class ProfessionTool
{
    public ItemId Id { get; }
    public GreedRole Role { get; }
    public ToolState State { get; internal set; }
    public ActorId ReservedBy { get; internal set; }
    public ProfessionTool(ItemId id, GreedRole role) { Id = id; Role = role; State = ToolState.Available; }
}

public sealed class PopulationService
{
    private readonly Dictionary<int, GreedRole> actors = new();
    private readonly Dictionary<int, ProfessionTool> tools = new();
    private readonly HashSet<int> assaultReserved = new();
    private int nextToolId = 1;
    public PopulationSnapshot Snapshot => BuildSnapshot();
    public event Action<PopulationSnapshot> Changed;
    public int AvailableFighters
    {
        get
        {
            int count=0;
            foreach(var pair in actors)
                if(pair.Value==GreedRole.Fighter&&!assaultReserved.Contains(pair.Key))count++;
            return count;
        }
    }

    public bool RegisterRecruit(ActorId actor)
    {
        if (actor.Value <= 0 || actors.ContainsKey(actor.Value)) return false;
        actors.Add(actor.Value, GreedRole.Formless); Notify(); return true;
    }

    public ProfessionTool OrderTool(GreedRole role)
    {
        if (role == GreedRole.Formless) return null;
        var tool = new ProfessionTool(new ItemId(nextToolId++), role);
        tools.Add(tool.Id.Value, tool); return tool;
    }

    public bool TryReserveNearest(ItemId toolId, ActorId actor)
    {
        if (!actors.TryGetValue(actor.Value, out GreedRole role) || role != GreedRole.Formless) return false;
        if (!tools.TryGetValue(toolId.Value, out ProfessionTool tool) || tool.State != ToolState.Available) return false;
        tool.State = ToolState.Reserved; tool.ReservedBy = actor; return true;
    }

    public bool TryCollectTool(ItemId toolId, ActorId actor)
    {
        if (!tools.TryGetValue(toolId.Value, out ProfessionTool tool) || tool.State != ToolState.Reserved || !tool.ReservedBy.Equals(actor)) return false;
        if (!actors.TryGetValue(actor.Value, out GreedRole role) || role != GreedRole.Formless) return false;
        actors[actor.Value] = tool.Role; tool.State = ToolState.Carried; Notify(); return true;
    }

    public bool RegisterDeath(ActorId actor)
    {
        if (!actors.Remove(actor.Value)) return false;
        assaultReserved.Remove(actor.Value);
        foreach (ProfessionTool tool in tools.Values)
        {
            if (tool.ReservedBy.Equals(actor) && tool.State == ToolState.Reserved)
            { tool.State = ToolState.Available; tool.ReservedBy = default; }
            else if (tool.ReservedBy.Equals(actor) && tool.State == ToolState.Carried)
            { tool.State = ToolState.Dropped; }
        }
        Notify(); return true;
    }

    public bool TryReserveFighters(int count,out ActorId[] fighters)
    {
        var result=new List<ActorId>(count);
        foreach(var pair in actors)
        {
            if(pair.Value!=GreedRole.Fighter||assaultReserved.Contains(pair.Key))continue;
            result.Add(new ActorId(pair.Key));
            if(result.Count==count)break;
        }
        if(result.Count<count){fighters=Array.Empty<ActorId>();return false;}
        foreach(ActorId fighter in result)assaultReserved.Add(fighter.Value);
        fighters=result.ToArray();return true;
    }

    public void ReleaseFighters(IEnumerable<ActorId> fighters)
    { if(fighters==null)return;foreach(ActorId fighter in fighters)assaultReserved.Remove(fighter.Value); }

    public bool IsAssaultReserved(ActorId actor)=>assaultReserved.Contains(actor.Value);

    private PopulationSnapshot BuildSnapshot()
    {
        int f = 0, fi = 0, g = 0, b = 0;
        foreach (GreedRole role in actors.Values)
        { if (role == GreedRole.Formless) f++; else if (role == GreedRole.Fighter) fi++; else if (role == GreedRole.Gatherer) g++; else if (role == GreedRole.Builder) b++; }
        return new PopulationSnapshot(actors.Count, f, fi, g, b);
    }
    private void Notify() => Changed?.Invoke(BuildSnapshot());
}

public sealed class FormlessCave
{
    public int Available { get; private set; }
    public int Maximum { get; }
    public float RespawnInterval { get; }
    private float elapsed;
    public FormlessCave(int maximum = 3, float respawnInterval = 90f) { Maximum = Math.Max(1, maximum); RespawnInterval = Math.Max(0.1f, respawnInterval); Available = Maximum; }
    public bool TryTake() { if (Available <= 0) return false; Available--; return true; }
    public void Tick(float deltaTime) { if (Available >= Maximum || deltaTime <= 0) return; elapsed += deltaTime; while (elapsed >= RespawnInterval && Available < Maximum) { elapsed -= RespawnInterval; Available++; } }
}
