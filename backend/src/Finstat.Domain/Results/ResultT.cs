namespace Finstat.Domain.Results;

public class Result<T> : Result
{
    private readonly T? value;

    private Result(T? value, Error? error) : base(error)
    {
        this.value = value;
    }

    public T Value
    {
        get
        {
            if (IsFailure)
                throw new InvalidOperationException("Невдалий результат не містить значення");
            return value!;
        }
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(value, null);
    }

    public static new Result<T> Failure(Error error)
    {
        return new Result<T>(default, error);
    }
}
