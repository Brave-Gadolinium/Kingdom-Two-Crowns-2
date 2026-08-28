public readonly struct OperationResult<TError>
{
    public bool Success { get; }
    public TError Error { get; }

    private OperationResult(bool success, TError error)
    {
        Success = success;
        Error = error;
    }

    public static OperationResult<TError> Ok()
    {
        return new OperationResult<TError>(true, default);
    }

    public static OperationResult<TError> Fail(TError error)
    {
        return new OperationResult<TError>(false, error);
    }
}