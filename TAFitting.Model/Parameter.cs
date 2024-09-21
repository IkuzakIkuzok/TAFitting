﻿
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Model;

/// <summary>
/// Represents a fitting parameter.
/// </summary>
public readonly struct Parameter
{
    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    required public string Name { get; init; }

    /// <summary>
    /// Gets the constraints of the parameter.
    /// </summary>
    public ParameterConstraints Constraints { get; init; } = ParameterConstraints.None;

    /// <summary>
    /// Gets the initial value of the parameter.
    /// </summary>
    public double InitialValue { get; init; } = 0.0;

    public Parameter() { }
} // public struct Parameter
