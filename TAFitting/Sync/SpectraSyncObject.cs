
// (c) 2025 Kazuki Kohzuki

using System.Diagnostics;
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
            sb.Append(';');  // Do NOT remove the trailing ';' even after the last entry.
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

        try
        {
            var dataSpan = data.AsSpan();
            if (dataSpan.Count('|') < 3) return null; // Invalid format

            var sep = dataSpan.IndexOf('|');
            var spectraIdPart = dataSpan[..sep];
            var spectraId = spectraIdPart.ParseIntInvariant();

            dataSpan = dataSpan[(sep + 1)..];
            sep = dataSpan.IndexOf('|');
            var hostName = dataSpan[..sep].ToString();

            dataSpan = dataSpan[(sep + 1)..];
            sep = dataSpan.IndexOf('|');
            var wavelengthsPart = dataSpan[..sep];
            var wavelengthCount = wavelengthsPart.Count(',') + 1;
            var wavelengths = new double[wavelengthCount];
            wavelengthCount = NegativeSignHandler.ParseDoubles(wavelengthsPart, ',', wavelengths.AsSpan());

            dataSpan = dataSpan[(sep + 1)..];
            sep = dataSpan.IndexOf('|');
            var maskingRanges = dataSpan[..sep].ToString();

            dataSpan = dataSpan[(sep + 1)..];

            var spectra = new Dictionary<double, IList<double>>();
            var values = (stackalloc double[wavelengthCount]);

            // Trailing ';' exists after the last entry, so checking for IndexOf(';') before loop is not necessary.
            while ((sep = dataSpan.IndexOf(';')) >= 0)
            {
                var part = dataSpan[..sep];
                dataSpan = dataSpan[(sep + 1)..];

                var equalIndex = part.IndexOf('=');
                if (equalIndex < 0) continue; // Invalid format

                var timePart = part[..equalIndex];
                var valuesPart = part[(equalIndex + 1)..];

                var time = timePart.ParseDoubleInvariant();
                var count = NegativeSignHandler.ParseDoubles(valuesPart, ',', values);
                Debug.Assert(count == wavelengthCount);
                spectra[time] = values.ToArray();
            }
            return new SpectraSyncObject(spectraId, hostName, wavelengths, maskingRanges, spectra);
        }
        catch
        {
            return null;
        }
    } // internal static SpectraSyncObject? Parse (string? data)
} // internal sealed class SpectraSyncObject
