using System;

public enum CrownOwner
{
    Greed,
    Ground,
    HumanCarrier,
    GreedCarrier,
    Heart,
    Settlement
}

public readonly struct CrownSnapshot
{
    public CrownOwner Owner { get; }
    public ActorId Carrier { get; }

    public CrownSnapshot(CrownOwner owner, ActorId carrier)
    {
        Owner = owner;
        Carrier = carrier;
    }
}

public interface ICrownService
{
    CrownSnapshot GreedCrown { get; }
    event Action<CrownSnapshot> GreedCrownChanged;
}
