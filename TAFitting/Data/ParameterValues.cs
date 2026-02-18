
// (c) 2025-2026 Kazuki Kohzuki

namespace TAFitting.Data;

/// <summary>
/// Represents parameter values at a specific wavelength.
/// </summary>
/// <param name="Wavelength">The wavelength in nanometers.</param>
/// <param name="Parameters">The parameter values corresponding to the wavelength. The order of values should match the order of parameters defined in the model.</param>
internal sealed record ParameterValues(double Wavelength, IReadOnlyList<double> Parameters);
