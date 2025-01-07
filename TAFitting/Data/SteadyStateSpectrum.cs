
// (c) 2025 Kazuki Kohzuki

using System.Collections;
using System.Text;

namespace TAFitting.Data;

/// <summary>
/// Represents a steady-state spectrum of a sample, which is a collection of wavelength and absorbance pairs.
/// </summary>
internal sealed class SteadyStateSpectrum : IEnumerable<(double Wavelength, double Absorbance)>
{
    private readonly List<(double Wavelength, double Absorbance)> _spectrum = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="SteadyStateSpectrum"/> class from a stream reader.
    /// </summary>
    /// <param name="reader">The stream reader to read the spectrum data from.</param>
    internal SteadyStateSpectrum(StreamReader reader)
    {
        LoadFile(reader);
    } // ctor (StreamReader)

    /// <summary>
    /// Initializes a new instance of the <see cref="SteadyStateSpectrum"/> class from a file.
    /// </summary>
    /// <param name="filename">The name of the file to read the spectrum data from.</param>
    /// <param name="encoding">The encoding to use for reading the file.</param>
    internal SteadyStateSpectrum(string filename, Encoding encoding)
    {
        using var reader = new StreamReader(filename, encoding);
        LoadFile(reader);
    } // ctor (string)

    private SteadyStateSpectrum(IEnumerable<(double, double)> spectrum)
    {
        this._spectrum = spectrum.ToList();
    } // ctor (IEnumerable<(double, double)>)

    private void LoadFile(StreamReader reader)
    {
        string? line;
        var abs = false;
        while ((line = reader.ReadLine()) is not null)
        {
            if (!line.StartsWith("nm")) continue;
            var fields = line.Split('\t');
            if (fields.Length < 2) continue;
            abs = fields[1].Contains("Abs");
            break;
        }

        Func<double, double> a_map = abs ? FromAbsorbance : FromTransmittance;
        while ((line = reader.ReadLine()) is not null)
        {
            var fields = line.Split('\t');
            if (fields.Length < 2) continue;
            if (!double.TryParse(fields[0], out var wl)) continue;
            if (!double.TryParse(fields[1], out var i)) continue;
            var a = a_map(i);
            this._spectrum.Add((wl, a));
        }
    } // private void LoadFile (string text)

    private static double FromTransmittance(double x)
        => -Math.Log10(x);

    private static double FromAbsorbance(double x)
        => x;

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
} // internal sealed class SteadyStateSpectrum : IEnumerable<(double, double)>
