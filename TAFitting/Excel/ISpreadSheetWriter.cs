
// (c) 2024 Kazuki KOHZUKI

using TAFitting.Model;

namespace TAFitting.Excel;

/// <summary>
/// Represents a writer for a spreadsheet.
/// </summary>
internal interface ISpreadSheetWriter
{
    /// <summary>
    /// Gets the model.
    /// </summary>
    IFittingModel Model { get; init; }

    /// <summary>
    /// Gets or sets the parameters.
    /// </summary>
    IReadOnlyList<string> Parameters { get; set; }

    /// <summary>
    /// Gets or sets the times.
    /// </summary>
    IReadOnlyList<double> Times { get; set; }

    /// <summary>
    /// Adds a row.
    /// </summary>
    /// <param name="wavelength">The wavelength.</param>
    /// <param name="parameters">The parameters.</param>
    void AddRow(double wavelength, IEnumerable<double> parameters);

    /// <summary>
    /// Writes the spreadsheet to the specified path.
    /// </summary>
    /// <param name="path">The path.</param>
    void Write(string path);
} // internal interface ISpreadSheetWriter
