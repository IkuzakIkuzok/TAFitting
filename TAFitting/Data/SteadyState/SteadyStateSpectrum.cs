
// (c) 2025 Kazuki Kohzuki

using System.Collections;

namespace TAFitting.Data.SteadyState;

/// <summary>
/// Represents a steady-state spectrum of a sample, which is a collection of wavelength and absorbance pairs.
/// </summary>
internal partial class SteadyStateSpectrum : IEnumerable<(double Wavelength, double Absorbance)>
{
    protected readonly List<(double Wavelength, double Absorbance)> _spectrum = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="SteadyStateSpectrum"/> class.
    /// </summary>
    internal SteadyStateSpectrum() { }

    protected SteadyStateSpectrum(IEnumerable<(double, double)> spectrum)
    {
        this._spectrum = spectrum.ToList();
    } // ctor (IEnumerable<(double, double)>)

    /// <summary>
    /// Loads the spectrum data from a file.
    /// </summary>
    /// <param name="path">The path to the file containing the spectrum data.</param>
    /// <exception cref="NotImplementedException">
    /// This method is not implemented in the base class.
    /// Call this method on a derived class that implements it.
    /// </exception>
    internal virtual void LoadFile(string path)
    {
        throw new NotImplementedException();
    } // internal virtual void LoadFile (string)

    /// <summary>
    /// Normalizes the spectrum by dividing each absorbance value by the maximum absorbance value.
    /// </summary>
    /// <param name="wavelengthMin">The minimum wavelength to consider for normalization.</param>
    /// <param name="wavelengthMax">The maximum wavelength to consider for normalization.</param>
    /// <param name="scale">The scale factor to apply to the maximum absorbance value.</param>
    /// <returns>The normalized spectrum.</returns>
    internal SteadyStateSpectrum Normalize(double wavelengthMin, double wavelengthMax, double scale = 1.0)
    {
        var points = this._spectrum.Where(x => x.Wavelength >= wavelengthMin && x.Wavelength <= wavelengthMax).ToList();

        if (points.Count == 0)
            throw new InvalidOperationException("Spectrum is empty.");

        var max = points.Max(x => x.Absorbance) / scale;
        return new(points.Select(x => (x.Wavelength, x.Absorbance / max)));
    } // internal SteadyStateSpectrum Normalize (double, double, [double])

    public IEnumerator<(double Wavelength, double Absorbance)> GetEnumerator()
        => this._spectrum.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => this._spectrum.GetEnumerator();
} // internal partial class SteadyStateSpectrum : IEnumerable<(double Wavelength, double Absorbance)>
