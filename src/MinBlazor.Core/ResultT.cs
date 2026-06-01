using System.Diagnostics.CodeAnalysis;

namespace MinBlazor.Core;

public readonly record struct Result<T>
{
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccess { get; }

    [MaybeNull]
    public T Value { get; }

    public string? Error { get; }

    private Result(bool isSuccess, [AllowNull] T value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value!;
        Error = error;
    }

    public static Result<T> Ok(T value) => new(true, value, null);

    public static Result<T> Fail(string error) => new(false, default, error);

    public static implicit operator Result<T>(T value) => Ok(value);
}
