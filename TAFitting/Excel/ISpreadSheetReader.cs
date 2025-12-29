
// (c) 2025 Kazuki KOHZUKI

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
    /// Gets a value indicating whether the spreadsheet is opened successfully.
    /// </summary>
    bool IsOpened { get; }

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
    /// Attempts to read the next row of data and writes its parameter values into the specified span.
    /// </summary>
    /// <param name="wavelength">When this method returns, contains the wavelength value for the next row, if a row was read; otherwise, <see cref="double.NaN"/>.</param>
    /// <param name="parameters">A span of doubles that receives the parameter values for the next row.
    /// The span's length must match the number of parameters expected by the selected model.</param>
    /// <returns><see langword="true"/> if a row was read and parameter values were written to the span;
    /// otherwise, <see langword="false"/> if there are no more rows to read.</returns>
    bool ReadNextRow(out double wavelength, Span<double> parameters);
} // internal interface ISpreadSheetReader
