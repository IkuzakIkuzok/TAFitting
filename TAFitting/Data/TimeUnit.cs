
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Data;

/// <summary>
/// Represents a time unit.
/// </summary>
internal sealed class TimeUnit : ValueUnit
{
    /// <summary>
    /// Second (s)
    /// </summary>
    internal static readonly TimeUnit Second = new(SIPrefix.None);

    /// <summary>
    /// Millisecond (ms)
    /// </summary>
    internal static readonly TimeUnit Millisecond = new(SIPrefix.Milli);

    /// <summary>
    /// Microsecond (μs)
    /// </summary>
    internal static readonly TimeUnit Microsecond = new(SIPrefix.Micro);

    /// <summary>
    /// Nanosecond (ns)
    /// </summary>
    internal static readonly TimeUnit Nanosecond = new(SIPrefix.Nano);

    /// <summary>
    /// Picosecond (ps)
    /// </summary>
    internal static readonly TimeUnit Picosecond = new(SIPrefix.Pico);

    /// <summary>
    /// Femtosecond (fs)
    /// </summary>

    internal static readonly TimeUnit Femtosecond = new(SIPrefix.Femto);

    private TimeUnit(SIPrefix scale) : base($"{GetPrefixSymbol(scale)}s", scale) { }
} // internal sealed class TimeUnit : ValueUnit
