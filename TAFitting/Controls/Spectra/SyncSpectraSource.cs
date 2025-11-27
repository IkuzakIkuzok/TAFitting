
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Controls.Spectra;

/// <summary>
/// Represents a synchronous source for spectra data, identified by host name and spectra ID.
/// </summary>
/// <param name="HostName">The name of the host providing the spectra data.</param>
/// <param name="SpectraId">The unique identifier for the spectra source on the specified host.</param>
internal record struct SyncSpectraSource(string HostName, int SpectraId);
