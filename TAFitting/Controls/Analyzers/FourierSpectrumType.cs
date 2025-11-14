
// (c) 2025 Kazuki KOHZUKI

using EnumSerializer;

namespace TAFitting.Controls.Analyzers;

/// <summary>
/// Represents the type of Fourier spectrum.
/// </summary>
[EnumSerializable(typeof(DefaultSerializeValueAttribute))]
public enum FourierSpectrumType
{
    /// <summary>
    /// Amplitude spectrum.
    /// </summary>
    [DefaultSerializeValue("Amplitude")]
    AmplitudeSpectrum,

    /// <summary>
    /// Amplitude spectral density.
    /// </summary>
    [DefaultSerializeValue("Amplitude spectral density")]
    AmplitudeSpectralDensity,

    /// <summary>
    /// Power spectrum.
    /// </summary>
    [DefaultSerializeValue("Power")]
    PowerSpectrum,

    /// <summary>
    /// Power spectral density.
    /// </summary>
    [DefaultSerializeValue("Power spectral density")]
    PowerSpectralDensity,

    /// <summary>
    /// Power spectral density in decibel.
    /// </summary>
    [DefaultSerializeValue("Power spectral density (dB)")]
    PowerSpectralDensityDecibel,
} // internal enum FourierSpectrumType
