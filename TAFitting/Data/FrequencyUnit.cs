
// (c) 2025 Kazuki KOHZUKI

namespace TAFitting.Data;

/// <summary>
/// Represents a unit of frequency expressed in hertz (Hz) with an associated SI prefix scale.
/// </summary>
internal sealed class FrequencyUnit : ValueUnit
{
    /// <summary>
    /// Initializes a new instance of the FrequencyUnit class with the specified SI prefix scale.
    /// </summary>
    /// <param name="scale">The SI prefix to apply to the frequency unit (e.g., kilo, mega, milli). Determines the scale of the unit.</param>
    internal FrequencyUnit(SIPrefix scale) : base($"{GetPrefixSymbol(scale)}Hz", scale) { }
} // internal sealed class FrequencyUnit : ValueUnit
