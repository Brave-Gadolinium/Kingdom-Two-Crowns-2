public sealed class InMemoryEconomy : IEconomy
{
    public int Greed { get; private set; }

    public InMemoryEconomy(int startingGreed)
    {
        Greed = startingGreed;
    }

    public void Add(int amount)
    {
        if (amount > 0)
            Greed += amount;
    }

    public OperationResult<EconomyError> TrySpend(int amount)
    {
        if (amount <= 0)
            return OperationResult<EconomyError>
                .Fail(EconomyError.InvalidAmount);

        if (Greed < amount)
            return OperationResult<EconomyError>
                .Fail(EconomyError.NotEnoughGreed);

        Greed -= amount;

        return OperationResult<EconomyError>.Ok();
    }
}