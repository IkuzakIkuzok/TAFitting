
// (c) 2026 Kazuki KOHZUKI

using System.Runtime.CompilerServices;

namespace TAFitting.Excel;

/// <summary>
/// Represents a fixed-size array of <see cref="ParameterMapEntry"/>.
/// </summary>
[InlineArray(Capacity)]
internal struct ParameterMapEntryArray
{
    /// <summary>
    /// Specifies the capacity value.
    /// </summary>
    internal const int Capacity = 32;

    private ParameterMapEntry _value0;
} // internal struct ParameterMapEntryArray
