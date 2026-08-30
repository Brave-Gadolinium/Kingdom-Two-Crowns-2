using System;
using System.Collections.Generic;

public enum TerritorySide { Left, Right }
public enum BuildingType { InfectionNode, GreedWall, EyeTower, ProfessionNest, CommandNode, Heart }
public enum PlacementFailure { None, OutsideInfection, NotAtBoundary, ChainBusy }

public readonly struct TerritoryChanged
{
    public float Left { get; }
    public float Right { get; }
    public TerritoryChanged(float left, float right) { Left = left; Right = right; }
}

public readonly struct InfectionNodeRecord
{
    public BuildingId Id { get; }
    public TerritorySide Side { get; }
    public float Boundary { get; }
    public InfectionNodeRecord(BuildingId id, TerritorySide side, float boundary)
    { Id = id; Side = side; Boundary = boundary; }
}

[Serializable]
public struct InfectionNodeSaveData
{
    public int Id;
    public TerritorySide Side;
    public float Boundary;
}

[Serializable]
public sealed class InfectionTerritorySnapshot
{
    public InfectionNodeSaveData[] LeftNodes;
    public InfectionNodeSaveData[] RightNodes;
}

public interface IInfectionTerritory
{
    float LeftBoundary { get; }
    float RightBoundary { get; }
    IReadOnlyList<InfectionNodeRecord> ActiveNodes { get; }
    bool Contains(float worldX);
    bool CanPlace(BuildingType type, float worldX, out PlacementFailure failure);
    bool CompleteNode(BuildingId id, TerritorySide side);
    bool DestroyNode(BuildingId id);
    InfectionTerritorySnapshot CaptureSnapshot();
    void Restore(InfectionTerritorySnapshot snapshot);
    event Action<TerritoryChanged> Changed;
}

public sealed class InfectionTerritory : IInfectionTerritory, ITerritoryService
{
    private const float BoundaryTolerance = 0.5f;
    private readonly float initialLeft;
    private readonly float initialRight;
    private readonly float expansion;
    private readonly List<InfectionNodeRecord> leftNodes = new();
    private readonly List<InfectionNodeRecord> rightNodes = new();
    private readonly HashSet<int> ids = new();

    public float LeftBoundary { get; private set; }
    public float RightBoundary { get; private set; }
    public float Size => RightBoundary - LeftBoundary;
    public IReadOnlyList<InfectionNodeRecord> ActiveNodes
    {
        get
        {
            var result = new List<InfectionNodeRecord>(leftNodes.Count + rightNodes.Count);
            result.AddRange(leftNodes);
            result.AddRange(rightNodes);
            return result;
        }
    }

    public event Action<TerritoryChanged> Changed;

    public InfectionTerritory(float center, float startingSize, float expansionSize)
    {
        float half = Math.Max(0.5f, startingSize * 0.5f);
        initialLeft = center - half;
        initialRight = center + half;
        expansion = Math.Max(0.01f, expansionSize);
        LeftBoundary = initialLeft;
        RightBoundary = initialRight;
    }

    public bool Contains(float worldX) => worldX >= LeftBoundary && worldX <= RightBoundary;

    public bool CanPlace(BuildingType type, float worldX, out PlacementFailure failure)
    {
        if (type == BuildingType.InfectionNode)
        {
            bool atBoundary = Math.Abs(worldX - LeftBoundary) <= BoundaryTolerance ||
                              Math.Abs(worldX - RightBoundary) <= BoundaryTolerance;
            failure = atBoundary ? PlacementFailure.None : PlacementFailure.NotAtBoundary;
            return atBoundary;
        }

        bool inside = Contains(worldX);
        failure = inside ? PlacementFailure.None : PlacementFailure.OutsideInfection;
        return inside;
    }

    public bool CompleteNode(BuildingId id, TerritorySide side)
    {
        if (id.Value <= 0 || !ids.Add(id.Value)) return false;
        if (side == TerritorySide.Left)
        {
            LeftBoundary -= expansion;
            leftNodes.Add(new InfectionNodeRecord(id, side, LeftBoundary));
        }
        else
        {
            RightBoundary += expansion;
            rightNodes.Add(new InfectionNodeRecord(id, side, RightBoundary));
        }
        Changed?.Invoke(new TerritoryChanged(LeftBoundary, RightBoundary));
        return true;
    }

    public bool DestroyNode(BuildingId id)
    {
        if (!ids.Contains(id.Value)) return false;
        if (RemoveChainFrom(leftNodes, id)) LeftBoundary = leftNodes.Count == 0 ? initialLeft : leftNodes[^1].Boundary;
        else if (RemoveChainFrom(rightNodes, id)) RightBoundary = rightNodes.Count == 0 ? initialRight : rightNodes[^1].Boundary;
        else return false;
        Changed?.Invoke(new TerritoryChanged(LeftBoundary, RightBoundary));
        return true;
    }

    public InfectionTerritorySnapshot CaptureSnapshot()
    {
        return new InfectionTerritorySnapshot
        {
            LeftNodes = leftNodes.ConvertAll(ToSaveData).ToArray(),
            RightNodes = rightNodes.ConvertAll(ToSaveData).ToArray()
        };
    }

    public void Restore(InfectionTerritorySnapshot snapshot)
    {
        leftNodes.Clear(); rightNodes.Clear(); ids.Clear();
        if (snapshot?.LeftNodes != null)
            foreach (InfectionNodeSaveData node in snapshot.LeftNodes)
                leftNodes.Add(new InfectionNodeRecord(new BuildingId(node.Id), node.Side, node.Boundary));
        if (snapshot?.RightNodes != null)
            foreach (InfectionNodeSaveData node in snapshot.RightNodes)
                rightNodes.Add(new InfectionNodeRecord(new BuildingId(node.Id), node.Side, node.Boundary));
        foreach (InfectionNodeRecord node in leftNodes) ids.Add(node.Id.Value);
        foreach (InfectionNodeRecord node in rightNodes) ids.Add(node.Id.Value);
        LeftBoundary = leftNodes.Count == 0 ? initialLeft : leftNodes[^1].Boundary;
        RightBoundary = rightNodes.Count == 0 ? initialRight : rightNodes[^1].Boundary;
        Changed?.Invoke(new TerritoryChanged(LeftBoundary, RightBoundary));
    }

    private static InfectionNodeSaveData ToSaveData(InfectionNodeRecord node)
    {
        return new InfectionNodeSaveData { Id = node.Id.Value, Side = node.Side, Boundary = node.Boundary };
    }

    private bool RemoveChainFrom(List<InfectionNodeRecord> chain, BuildingId id)
    {
        int index = chain.FindIndex(node => node.Id.Equals(id));
        if (index < 0) return false;
        for (int i = index; i < chain.Count; i++) ids.Remove(chain[i].Id.Value);
        chain.RemoveRange(index, chain.Count - index);
        return true;
    }

    public void Expand(float amount)
    {
        if (amount <= 0f) return;
        RightBoundary += amount;
        Changed?.Invoke(new TerritoryChanged(LeftBoundary, RightBoundary));
    }
}
