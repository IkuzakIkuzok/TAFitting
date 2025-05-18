
// (c) 2025 Kazuki KOHZUKI

using TAFitting.Data;

namespace TAFitting.Controls.Analyzers;

/// <summary>
/// Interface for analyzers.
/// </summary>
internal interface IDecayAnalyzer
{
    /// <summary>
    /// Sets the decay to analyze.
    /// </summary>
    /// <param name="decay">The decay to analyze</param>
    /// <param name="wavelength">The wavelength of the decay</param>
    void SetDecay(Decay decay, double wavelength);
} // internal interface IDecayAnalyzer
