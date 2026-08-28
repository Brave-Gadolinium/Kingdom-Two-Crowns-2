public interface IEconomy
{
    int Greed { get; }

    void Add(int amount);

    OperationResult<EconomyError> TrySpend(int amount);
}