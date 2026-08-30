using System;

public sealed class CrownService : ICrownService
{
    public CrownSnapshot GreedCrown { get; private set; }
    public event Action<CrownSnapshot> GreedCrownChanged;

    public CrownService()
    {
        GreedCrown = new CrownSnapshot(CrownOwner.Greed, new ActorId(1));
    }

    public void SetGreedCrown(CrownOwner owner, ActorId carrier)
    {
        GreedCrown = new CrownSnapshot(owner, carrier);
        GreedCrownChanged?.Invoke(GreedCrown);
    }
}
