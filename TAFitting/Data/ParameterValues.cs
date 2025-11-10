
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Data;

/// <summary>
/// Represents parameter values at a specific wavelength.
/// </summary>
internal sealed class ParameterValues
{
    /// <summary>
    /// Gets the wavelength.
    /// </summary>
    internal double Wavelength { get; }

    /// <summary>
    /// Gets the parameter values.
    /// </summary>
    internal IReadOnlyList<double> Parameters { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterValues"/> class.
    /// </summary>
    /// <param name="wavelength">The wavelength.</param>
    /// <param name="values">The parameter values.</param>
    internal ParameterValues(double wavelength, IReadOnlyList<double> values)
    {
        this.Wavelength = wavelength;
        this.Parameters = values;
    } // ctor (double, IReadOnlyList<double>)
} // internal sealed class ParameterValues
