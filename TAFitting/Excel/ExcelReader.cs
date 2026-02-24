
// (c) 2025 Kazuki Kohzuki

using ClosedXML.Excel;
using DisposalGenerator;
using TAFitting.Model;

namespace TAFitting.Excel;

/// <summary>
/// Represents a reader for an Excel spreadsheet.
/// </summary>
[AutoDisposal]
internal sealed partial class ExcelReader : ISpreadSheetReader
{
    private XLWorkbook? workbook;
    private IXLWorksheet? worksheet;
    private int rowIndex = 2;

    /// <inheritdoc/>
    public IFittingModel Model { get; init; }

    /// <inheritdoc/>
    public bool IsOpened => this.workbook is not null && this.worksheet is not null;

    /// <inheritdoc/>
    public bool ModelMatched { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyList<string> Parameters { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcelReader"/> class.
    /// </summary>
    /// <param name="model">The model.</param>
    internal ExcelReader(IFittingModel model)
    {
        this.Model = model;
        this.Parameters = model.Parameters.Names;
    } // ctor (IFittingModel)

    /// <inheritdoc/>
    public void Open(string path)
    {
        try
        {
            this.workbook = new(path);
            this.worksheet = this.workbook.Worksheet(1);

            var parameters = this.worksheet.Range(1, 2, 1, this.Parameters.Count + 1).Cells()
                .Select(cell => cell.GetString()).ToArray();

            for (var i = 0 ; i < this.Parameters.Count; i++)
            {
                if (parameters[i] != this.Parameters[i])
                {
                    this.ModelMatched = false;
                    return;
                }
            }

            this.ModelMatched = true;
        }
        catch
        {
            this.ModelMatched = false;
        }
    } // public void Open (string)

    /// <inheritdoc/>
    public bool ReadNextRow(out double wavelength, Span<double> parameters)
    {
        if (this.worksheet is null)
            throw new InvalidOperationException("The workbook is not opened.");
        if (!this.ModelMatched)
            throw new InvalidOperationException("The model does not match with the spreadsheet.");
        if (parameters.Length != this.Parameters.Count)
            throw new ArgumentException("The length of the parameters span does not match the number of parameters.", nameof(parameters));

        var row = this.worksheet.Row(this.rowIndex++);
        if (row.IsEmpty())
        {
            wavelength = double.NaN;
            return false;
        }

        wavelength = row.Cell(1).GetDouble();
        for (var i = 0; i < this.Parameters.Count; i++)
            parameters[i] = row.Cell(i + 2).GetDouble();
        return true;
    } // public bool ReadNextRow (out double, Span<double>)
} // internal sealed partial class ExcelReader : ISpreadSheetReader
