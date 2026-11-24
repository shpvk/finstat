namespace Finstat.Domain.Results;

public record Error(ErrorType Type, string Message)
{
    public static Error Validation(string message)
    {
        return new Error(ErrorType.Validation, message);
    }

    public static Error NotFound()
    {
        return new Error(ErrorType.NotFound, "Не знайдено");
    }
}
