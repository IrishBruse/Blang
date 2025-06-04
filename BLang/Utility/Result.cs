namespace BLang.Utility;

using System;

public record Error<TError>(TError Value);

public readonly struct Result<T, TError>
{
    private readonly T value;
    private readonly TError error;
    public readonly bool IsOk;

    private Result(T value, TError error, bool isOk)
    {
        this.value = value;
        this.error = error;
        IsOk = isOk;
    }

    public static Result<T, TError> Ok(T? value)
    {
        if (value == null)
        {
            return new Result<T, TError>(default!, default!, false);
        }
        return new Result<T, TError>(value, default!, true);
    }

    public static Result<T, TError> Fail(TError? error)
    {
        if (error == null)
        {
            return new Result<T, TError>(default!, default!, false);
        }
        return new Result<T, TError>(default!, error, false);
    }

    public T Value
    {
        get
        {
            if (!IsOk)
            {
                throw new InvalidOperationException("Result is not Ok. Access Error for details.");
            }
            return value;
        }
    }

    public TError Error
    {
        get
        {
            if (IsOk)
            {
                throw new InvalidOperationException("Result is Ok. Access Value for details.");
            }
            return error;
        }
    }

    public R Match<R>(Func<T, R> onSuccess, Func<TError, R> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsOk ? onSuccess(Value) : onFailure(Error);
    }

    public void Switch(Action<T> onSuccess, Action<TError> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (IsOk)
        {
            onSuccess(Value);
        }
        else
        {
            onFailure(Error);
        }
    }

    public T? Catch(Action<TError> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onFailure);

        if (!IsOk)
        {
            onFailure(Error);
        }

        return Value;
    }

    public void IfError(Action<TError> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onFailure);

        if (!IsOk)
        {
            onFailure(Error);
        }
    }

    public void IfOk(Action<T> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);

        if (IsOk)
        {
            onSuccess(Value);
        }
    }

    public override string ToString()
    {
        return IsOk ? $"Ok({value})" : $"Error({error})";
    }

    public override bool Equals(object? obj)
    {
        if (obj is Result<T, TError> other)
        {
            if (IsOk && other.IsOk)
            {
                return Equals(value, other.value);
            }
            if (!IsOk && !other.IsOk)
            {
                return Equals(error, other.error);
            }
        }
        return false;
    }

    public override int GetHashCode()
    {
        return IsOk ? value?.GetHashCode() ?? 0 : error?.GetHashCode() ?? 0;
    }

    public static bool operator ==(Result<T, TError> left, Result<T, TError> right) => left.Equals(right);
    public static bool operator !=(Result<T, TError> left, Result<T, TError> right) => !left.Equals(right);
}
