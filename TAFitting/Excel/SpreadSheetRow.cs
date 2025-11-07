
// (c) 2025 Kazuki KOHZUKI

namespace TAFitting.Excel;

internal sealed class SpreadSheetRow
{
    /// <summary>
    /// Gets the wavelength.
    /// </summary>
    internal double Wavelength { get; }

    /// <summary>
    /// Gets the parameters of the row.
    /// </summary>
    internal IReadOnlyList<double> Parameters { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpreadSheetRow"/> class.
    /// </summary>
    /// <param name="wavelength">The wavelength.</param>
    /// <param name="parameters">The parameters.</param>
    internal SpreadSheetRow(double wavelength, IReadOnlyList<double> parameters)
    {
        this.Wavelength = wavelength;
        this.Parameters = parameters;
    } // ctor (double, IReadOnlyList<double>)
} // internal sealed class SpreadSheet
