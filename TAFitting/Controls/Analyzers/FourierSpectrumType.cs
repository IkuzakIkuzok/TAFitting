
// (c) 2025 Kazuki KOHZUKI

namespace TAFitting.Controls.Analyzers;

/// <summary>
/// Represents the type of Fourier spectrum.
/// </summary>
public enum FourierSpectrumType
{
    /// <summary>
    /// Amplitude spectrum.
    /// </summary>
    AmplitudeSpectrum,

    /// <summary>
    /// Amplitude spectral density.
    /// </summary>
    AmplitudeSpectralDensity,

    /// <summary>
    /// Power spectrum.
    /// </summary>
    PowerSpectrum,

    /// <summary>
    /// Power spectral density.
    /// </summary>
    PowerSpectralDensity,

    /// <summary>
    /// Power spectral density in decibel.
    /// </summary>
    PowerSpectralDensityDecibel,
} // internal enum FourierSpectrumType
