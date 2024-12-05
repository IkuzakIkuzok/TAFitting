
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Data;

/// <summary>
/// Represents a signal unit for the optical density.
/// </summary>
internal sealed class SignalUnit : ValueUnit
{
    /// <summary>
    /// ΔOD
    /// </summary>
    internal static readonly SignalUnit OD = new(SIPrefix.None);

    /// <summary>
    /// ΔmOD
    /// </summary>
    internal static readonly SignalUnit MilliOD = new(SIPrefix.Milli);

    /// <summary>
    /// ΔµOD
    /// </summary>
    internal static readonly SignalUnit MicroOD = new(SIPrefix.Micro);

    private SignalUnit(SIPrefix scale) : base($"Δ{GetPrefixSymbol(scale)}OD", scale) { }
} // internal sealed class SignalUnit
