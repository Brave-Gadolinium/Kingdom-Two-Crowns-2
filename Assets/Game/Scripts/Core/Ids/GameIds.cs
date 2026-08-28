using System;

public readonly struct ActorId : IEquatable<ActorId>
{
    public int Value { get; }

    public ActorId(int value) => Value = value;
    public bool Equals(ActorId other) => Value == other.Value;
    public override bool Equals(object obj) => obj is ActorId other && Equals(other);
    public override int GetHashCode() => Value;
    public override string ToString() => Value.ToString();
}

public readonly struct BuildingId : IEquatable<BuildingId>
{
    public int Value { get; }

    public BuildingId(int value) => Value = value;
    public bool Equals(BuildingId other) => Value == other.Value;
    public override bool Equals(object obj) => obj is BuildingId other && Equals(other);
    public override int GetHashCode() => Value;
    public override string ToString() => Value.ToString();
}

public readonly struct ItemId : IEquatable<ItemId>
{
    public int Value { get; }

    public ItemId(int value) => Value = value;
    public bool Equals(ItemId other) => Value == other.Value;
    public override bool Equals(object obj) => obj is ItemId other && Equals(other);
    public override int GetHashCode() => Value;
    public override string ToString() => Value.ToString();
}

public readonly struct CycleNumber : IEquatable<CycleNumber>
{
    public int Value { get; }

    public CycleNumber(int value) => Value = value;
    public bool Equals(CycleNumber other) => Value == other.Value;
    public override bool Equals(object obj) => obj is CycleNumber other && Equals(other);
    public override int GetHashCode() => Value;
    public override string ToString() => Value.ToString();
}
