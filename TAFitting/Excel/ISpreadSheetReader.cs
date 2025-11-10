
// (c) 2025 Kazuki KOHZUKI

using TAFitting.Data;
using TAFitting.Model;

namespace TAFitting.Excel;

/// <summary>
/// Represents a reader for a spreadsheet.
/// </summary>
internal interface ISpreadSheetReader
{
    /// <summary>
    /// Gets the model.
    /// </summary>
    IFittingModel Model { get; init; }

    /// <summary>
    /// Gets a value indicating whether the model matched with the spreadsheet.
    /// </summary>
    bool ModelMatched { get; }

    /// <summary>
    /// Gets or sets the parameters.
    /// </summary>
    IReadOnlyList<string> Parameters { get; }

    /// <summary>
    /// Opens the spreadsheet from the specified path.
    /// </summary>
    /// <param name="path">The path for the spreadsheet</param>
    void Open(string path);

    /// <summary>
    /// Reads the rows.
    /// </summary>
    /// <returns>The rows.</returns>
    IEnumerable<ParameterValues> ReadRows();
} // internal interface ISpreadSheetReader
