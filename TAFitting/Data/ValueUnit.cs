
// (c) 2024-2025 Kazuki KOHZUKI

using System.Diagnostics;

namespace TAFitting.Data;

/// <summary>
/// Represents a value unit.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ValueUnit"/> class.
/// </remarks>
/// <param name="label">The label.</param>
/// <param name="scale">The scale.</param>
[DebuggerDisplay("{Label}")]
internal abstract class ValueUnit(string label, SIPrefix scale)
{
    /// <summary>
    /// Gets the label.
    /// </summary>
    internal string Label { get; } = label;

    /// <summary>
    /// Gets the scale.
    /// </summary>
    internal SIPrefix Scale { get; } = scale;

    /// <summary>
    /// Gets the scale factor.
    /// </summary>
    /// <param name="prefix">The prefix.</param>
    /// <returns>The scale factor.</returns>
    protected static double GetScale(SIPrefix prefix)
        => Math.Pow(10.0, (double)prefix);

    /// <summary>
    /// Gets the symbol.
    /// </summary>
    /// <param name="prefix">The prefix.</param>
    /// <returns>The prefix symbol.</returns>
    protected static string GetPrefixSymbol(SIPrefix prefix)
        => prefix.ToDefaultSerializeValue();

    public static implicit operator string(ValueUnit value)
        => value.Label;

    public static implicit operator double(ValueUnit value)
        => GetScale(value.Scale);

    override public string ToString()
        => this.Label;
} // internal abstract class ValueUnit
