
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Model;

/// <summary>
/// Represents the constraints of a parameter.
/// </summary>
[Flags]
public enum ParameterConstraints
{
    /// <summary>
    /// No constraints.
    /// </summary>
    None = 0x00,

    /// <summary>
    /// Parameter must be an integer.
    /// </summary>
    Integer = 0x01,

    /// <summary>
    /// Parameter must be positive.
    /// </summary>
    Positive = 0x02,

    /// <summary>
    /// Parameter must be non-negative.
    /// </summary>
    NonNegative = 0x04,
} // public enum ParameterConstraints
