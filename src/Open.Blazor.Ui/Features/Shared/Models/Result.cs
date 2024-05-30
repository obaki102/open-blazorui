using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Open.Blazor.Ui.Features.Shared.Models;

public class Result
{
    public bool IsSuccess { get; }
    public Error Error { get; }
    public bool IsFailure => !IsSuccess;

    public Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
            throw new ArgumentException("Invalid error", nameof(error));
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Failure(Error error) =>
        new(false, error);


    public static Result<T> Failure<T>(Error error) =>
        new(default, false, error);

    public static Result Success() =>
        new(true, Error.None);


    public static Result<T> Success<T>(T value) =>
        new(value, true, Error.None);


}

public class Result<TValue>(TValue? value, bool isSuccess, Error error) : Result(isSuccess, error)
{
    private readonly TValue? _value = value;

    public TValue? Value => _value!;

    public static implicit operator Result<TValue>(TValue value) => new(value, true, Error.None);

    public static implicit operator Result<TValue>(Error error) => new(default(TValue), false, error);

}