
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Data;

/// <summary>
/// Represents an immutable token that identifies the state of a parameters list for internal operations.
/// </summary>
/// <remarks>This struct is used to track and compare the state of parameter lists within internal components.
/// Instances may be invalid if not constructed with a hash value; use the <see cref="IsValid"/> property to determine validity.
/// Equality and hash code operations are based on the underlying state token value.</remarks>
internal readonly struct ParametersListStateToken
{
    private readonly bool _valid;
    private readonly long _hash;

    /// <summary>
    /// Gets a value indicating whether the current object is in a valid state.
    /// </summary>
    internal bool IsValid => this._valid;

    /// <summary>
    /// Gets the computed hash value as a 64-bit integer.
    /// </summary>
    internal long Value => this._hash;

    /// <summary>
    /// Initializes a new instance of the ParametersListStateToken class using the specified hash value.
    /// </summary>
    /// <param name="hash">The hash value that uniquely identifies the parameters list state.</param>
    internal ParametersListStateToken(long hash) : this()
    {
        this._valid = true;
        this._hash = hash;
    } // ctor (long hash)

    public static bool operator ==(ParametersListStateToken left, ParametersListStateToken right)
    {
        if (!left._valid) return !right._valid;
        return left._hash == right._hash;
    } // public static bool operator == (ParametersListStateToken, ParametersListStateToken)

    public static bool operator !=(ParametersListStateToken left, ParametersListStateToken right)
        => !(left == right);

    override public bool Equals(object? obj)
    {
        if (obj is ParametersListStateToken token)
            return this == token;
        return false;
    } // override public bool Equals (object?)

    override public int GetHashCode()
        => this._hash.GetHashCode();
} // internal readonly struct ParametersListStateToken
