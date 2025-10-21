
// (c) 2025 Kazuki Kohzuki

using System.Text;
using TAFitting.Controls;

namespace TAFitting.Sync;

/// <summary>
/// A wrapper for spectra synchronization data.
/// </summary>
internal sealed class SpectraSyncObject
{
    /// <summary>
    /// Gets the spectra ID.
    /// </summary>
    internal int SpectraId { get; private set; }

    /// <summary>
    /// Gets the host name of the source of the spectra.
    /// </summary>
    internal string HostName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the list of wavelengths.
    /// </summary>
    internal IList<double> Wavelengths { get; private set; }

    /// <summary>
    /// Gets the string representation of the masking ranges.
    /// </summary>
    internal string MaskRanges { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the spectra data, where the key is the wavelength and the value is a list of intensity values.
    /// </summary>
    internal IDictionary<double, IList<double>> Spectra { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpectraSyncObject"/> class.
    /// </summary>
    /// <param name="spectraId">The ID of the spectra.</param>
    /// <param name="hostName">The host name of the source of the spectra.</param>
    /// <param name="wavelengths">The list of wavelengths.</param>
    /// <param name="spectra">The spectra data, where the key is the wavelength and the value is a list of intensity values.</param>
    internal SpectraSyncObject(int spectraId, string hostName, IList<double> wavelengths, string maskingRanges, IDictionary<double, IList<double>> spectra)
    {
        this.SpectraId = spectraId;
        this.HostName = hostName;
        this.Wavelengths = wavelengths;
        this.MaskRanges = maskingRanges;
        this.Spectra = spectra;
    } // ctor (int, string, IList<double>, string, IDictionary<double, IList<double>>)

    /// <inheritdoc />
    override public string ToString()
    {
        var sb = new StringBuilder();

        sb.Append(this.SpectraId);
        sb.Append('|');
        sb.Append(this.HostName);
        sb.Append('|');

        sb.Append(string.Join(',', this.Wavelengths.Select(w => w.ToInvariantString())));
        sb.Append('|');

        sb.Append(this.MaskRanges);
        sb.Append('|');

        foreach ((var wavelength, var values) in this.Spectra)
        {
            sb.Append(wavelength);
            sb.Append('=');
            sb.Append(string.Join(',', values.Select(v => v.ToInvariantString())));
            sb.Append(';');
        }

        return sb.ToString();
    } // override public string ToString ()

    /// <summary>
    /// Parses a string representation of a spectra synchronization object.
    /// </summary>
    /// <param name="data">The string representation of the spectra synchronization object.</param>
    /// <returns>The parsed <see cref="SpectraSyncObject"/> or <see langword="null"/> if the format is invalid.</returns>
    internal static SpectraSyncObject? Parse(string? data)
    {
        if (string.IsNullOrEmpty(data)) return null!;

        var parts = data.Split('|');
        if (parts.Length < 4) return null; // Invalid format
        var spectraId = parts[0].ParseIntInvariant();
        var hostName = parts[1];
        var wavelengths = parts[2].Split(',').Select(double.Parse).ToList();
        var maskingRanges = parts[3]; // Assuming this is a string representation of masking ranges
        var spectra = new Dictionary<double, IList<double>>();
        var spectraParts = parts[4].Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in spectraParts)
        {
            var kvp = part.Split('=');
            if (kvp.Length != 2) continue; // Invalid format
            var wavelength = kvp[0].ParseDoubleInvariant();
            var values =
                kvp[1]
                .Split(',')
                .Select(NegativeSignHandler.ToMinusSign)
                .Select(double.Parse).ToList();
            spectra[wavelength] = values;
        } // foreach (var part in spectraParts)
        return new SpectraSyncObject(spectraId, hostName, wavelengths, maskingRanges, spectra);
    } // internal static SpectraSyncObject? Parse (string? data)
} // internal sealed class SpectraSyncObject
