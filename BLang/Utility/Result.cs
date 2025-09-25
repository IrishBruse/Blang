namespace BLang.Utility;

using System;

public readonly struct Result<T>
{

    private readonly T value;
    private readonly string error;
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private Result(T value)
    {
        IsSuccess = true;
        this.value = value;
        error = default!; // Use default to avoid an uninitialized value warning
    }

    private Result(string error)
    {
        IsSuccess = false;
        value = default!;
        this.error = error;
    }

    /// <summary>
    /// Gets the success value. Throws an InvalidOperationException if the result is a failure.
    /// </summary>
    public T Value => IsSuccess ? value : throw new InvalidOperationException("Cannot access value on a failed result.");

    /// <summary>
    /// Gets the error value. Throws an InvalidOperationException if the result is a success.
    /// </summary>
    public string Error => IsFailure ? error : throw new InvalidOperationException("Cannot access error on a successful result.");

    /// <summary>
    /// Implicitly converts a success value into a successful Result.
    /// </summary>
    public static implicit operator Result<T>(T value)
    {
        return new(value);
    }

    /// <summary>
    /// Implicitly converts an error value into a failed Result.
    /// </summary>
    public static implicit operator Result<T>(string error)
    {
        return new(error);
    }

    /// <summary>
    /// Performs an action based on whether the result is a success or a failure.
    /// </summary>
    public void Match(Action<T> success, Action<string> failure)
    {
        if (IsSuccess)
        {
            success(Value);
        }
        else
        {
            failure(Error);
        }
    }
}

#pragma warning disable IDE1006
#pragma warning disable CA1715

public readonly struct Result<T, E>
{
    private readonly T value;
    private readonly E error;
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private Result(T value)
    {
        IsSuccess = true;
        this.value = value;
        error = default!; // Use default to avoid an uninitialized value warning
    }

    private Result(E error)
    {
        IsSuccess = false;
        value = default!;
        this.error = error;
    }

    /// <summary>
    /// Gets the success value. Throws an InvalidOperationException if the result is a failure.
    /// </summary>
    public T Value => IsSuccess ? value : throw new InvalidOperationException("Cannot access value on a failed result.");

    /// <summary>
    /// Gets the error value. Throws an InvalidOperationException if the result is a success.
    /// </summary>
    public E Error => IsFailure ? error : throw new InvalidOperationException("Cannot access error on a successful result.");

    /// <summary>
    /// Implicitly converts a success value into a successful Result.
    /// </summary>
    public static implicit operator Result<T, E>(T value)
    {
        return new(value);
    }

    /// <summary>
    /// Implicitly converts an error value into a failed Result.
    /// </summary>
    public static implicit operator Result<T, E>(E error)
    {
        return new(error);
    }

    /// <summary>
    /// Performs an action based on whether the result is a success or a failure.
    /// </summary>
    public void Match(Action<T> success, Action<E> failure)
    {
        if (IsSuccess)
        {
            success(Value);
        }
        else
        {
            failure(Error);
        }
    }
}
