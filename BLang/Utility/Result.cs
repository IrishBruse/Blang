namespace BLang.Utility;

using System;

public readonly struct Result<T>
{
    private readonly T value;
    private readonly string error;
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public Result(T value)
    {
        IsSuccess = true;
        this.value = value;
        error = default!; // Use default to avoid an uninitialized value warning
    }

    public Result(string error)
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
    /// Defines a true expression for a successful result.
    /// This allows using 'if (result)' as a shorthand for 'if (result.IsSuccess)'.
    /// </summary>
    public static bool operator true(Result<T> result)
    {
        return result.IsSuccess;
    }

    /// <summary>
    /// Defines a false expression for a failed result.
    /// This allows using 'if (!result)' as a shorthand for 'if (result.IsFailure)'.
    /// </summary>
    public static bool operator false(Result<T> result)
    {
        return result.IsFailure;
    }

    /// <summary>
    /// Overloads the logical negation (!) operator.
    /// When combined with the true/false operators, this allows '!result' to check for failure.
    /// </summary>
    public static bool operator !(Result<T> result)
    {
        return result.IsFailure;
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

    public Result<T> Success(Action<T> success)
    {
        if (IsSuccess)
        {
            success(Value);
        }

        return this;
    }

    public void Failure(Action<string> failure)
    {
        if (!IsSuccess)
        {
            failure(Error);
        }
    }
}
