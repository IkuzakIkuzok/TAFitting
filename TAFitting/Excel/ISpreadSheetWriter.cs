
// (c) 2024-2026 Kazuki KOHZUKI

using TAFitting.Model;

namespace TAFitting.Excel;

/// <summary>
/// Represents a writer for a spreadsheet.
/// </summary>
internal interface ISpreadSheetWriter : IDisposable
{
    /// <summary>
    /// Gets the model.
    /// </summary>
    IFittingModel Model { get; init; }

    /// <summary>
    /// Adds a row.
    /// </summary>
    /// <param name="wavelength">The wavelength.</param>
    /// <param name="parameters">The parameters.</param>
    void AddRow(double wavelength, IReadOnlyList<double> parameters);
} // internal interface ISpreadSheetWriter : IDisposable
