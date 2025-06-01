
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Sync;

internal delegate void SpectraReceivedEventHandler(object? sender, SpectraReceivedEventArgs e);

/// <summary>
/// Represents the event arguments for the synchronization of spectra data.
/// </summary>
internal sealed class SpectraReceivedEventArgs : EventArgs
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
    /// Initializes a new instance of the <see cref="SpectraReceivedEventArgs"/> class.
    /// </summary>
    /// <param name="syncObject">The spectra synchronization object containing the data.</param>
    internal SpectraReceivedEventArgs(SpectraSyncObject syncObject)
    {
        this.SpectraId = syncObject.SpectraId;
        this.HostName = syncObject.HostName;
        this.Wavelengths = syncObject.Wavelengths;
        this.MaskRanges = syncObject.MaskRanges;
        this.Spectra = syncObject.Spectra;
    } // ctor (SpectraSyncObject)
} // internal sealed class SpectraReceivedEventArgs : EventArgs
