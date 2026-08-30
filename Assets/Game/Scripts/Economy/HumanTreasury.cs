using System;
using System.Collections.Generic;

public enum SettlementTier { Camp, Village, FortifiedVillage, City, Castle }

public readonly struct GoldReservation
{
    public int Amount { get; }
    public bool Success => Amount > 0;
    public GoldReservation(int amount) { Amount = amount; }
}

public sealed class HumanTreasury
{
    private readonly Dictionary<int, int> reservations = new();
    public int StoredGold { get; private set; }
    public int Capacity { get; private set; }
    public int ReservedGold { get; private set; }
    public int AvailableGold => StoredGold - ReservedGold;

    public HumanTreasury(int startingGold = 0, int capacity = 30)
    { Capacity = Math.Max(1, capacity); StoredGold = Math.Clamp(startingGold, 0, Capacity); }

    public GoldReservation TryReserveGold(ActorId gatherer, int requested)
    {
        if (gatherer.Value <= 0 || requested <= 0 || reservations.ContainsKey(gatherer.Value)) return new GoldReservation(0);
        int amount = Math.Min(requested, AvailableGold);
        if (amount <= 0) return new GoldReservation(0);
        reservations.Add(gatherer.Value, amount);
        ReservedGold += amount;
        return new GoldReservation(amount);
    }

    public int ConfirmTaken(ActorId gatherer)
    {
        if (!reservations.Remove(gatherer.Value, out int amount)) return 0;
        ReservedGold -= amount;
        StoredGold -= amount;
        return amount;
    }

    public int CancelReservation(ActorId gatherer)
    {
        if (!reservations.Remove(gatherer.Value, out int amount)) return 0;
        ReservedGold -= amount;
        return amount;
    }

    public int Produce(int amount)
    {
        if (amount <= 0) return 0;
        int added = Math.Min(amount, Capacity - StoredGold);
        StoredGold += added;
        return added;
    }

    public bool TrySpendExact(int amount)
    {
        if (amount <= 0 || AvailableGold < amount) return false;
        StoredGold -= amount;
        return true;
    }

    public void SetCapacity(int capacity)
    {
        Capacity = Math.Max(Math.Max(1, capacity), StoredGold);
    }
}
