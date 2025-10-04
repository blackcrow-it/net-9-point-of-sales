namespace Application.Common.Models;

/// <summary>
/// Represents the result of an operation
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public List<string> Errors { get; }

    private Result(bool isSuccess, T? data, List<string> errors)
    {
        IsSuccess = isSuccess;
        Data = data;
        Errors = errors;
    }

    public static Result<T> Success(T data)
        => new(true, data, new List<string>());

    public static Result<T> Failure(params string[] errors)
        => new(false, default, new List<string>(errors));

    public static Result<T> Failure(List<string> errors)
        => new(false, default, errors);
}

/// <summary>
/// Represents the result of an operation without data
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public List<string> Errors { get; }

    private Result(bool isSuccess, List<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static Result Success()
        => new(true, new List<string>());

    public static Result Failure(params string[] errors)
        => new(false, new List<string>(errors));

    public static Result Failure(List<string> errors)
        => new(false, errors);
}
