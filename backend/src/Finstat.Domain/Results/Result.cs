namespace Finstat.Domain.Results;

public class Result
{
    private readonly Error? error;

    protected Result(Error? error)
    {
        this.error = error;
    }

    public bool IsSuccess
    {
        get { return error is null; }
    }

    public bool IsFailure
    {
        get { return error is not null; }
    }

    public Error Error
    {
        get
        {
            if (error is null)
                throw new InvalidOperationException("Успішний результат не містить помилки");
            return error;
        }
    }

    public static Result Success()
    {
        return new Result(null);
    }

    public static Result Failure(Error error)
    {
        return new Result(error);
    }
}
