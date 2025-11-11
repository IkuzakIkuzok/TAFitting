
// (c) 2025 Kazuki Kohzuki

using ClosedXML.Excel;
using TAFitting.Data;
using TAFitting.Model;

namespace TAFitting.Excel;

/// <summary>
/// Represents a reader for an Excel spreadsheet.
/// </summary>
internal class ExcelReader : ISpreadSheetReader, IDisposable
{
    private XLWorkbook? workbook;
    private IXLWorksheet? worksheet;
    private int rowIndex = 2;
    private bool _disposed = false;

    /// <inheritdoc/>
    public IFittingModel Model { get; init; }

    /// <inheritdoc/>
    public bool IsOpened => this.workbook is not null && this.worksheet is not null && !this._disposed;

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
    public IEnumerable<ParameterValues> ReadRows()
    {
        if (this.worksheet is null)
            throw new InvalidOperationException("The workbook is not opened.");
        if (!this.ModelMatched)
            throw new InvalidOperationException("The model does not match with the spreadsheet.");

        IXLRow row;
        while (!(row = this.worksheet.Row(this.rowIndex)).IsEmpty())
        {
            var parameters = new double[this.Parameters.Count];
            var wavelength = row.Cell(1).GetDouble();
            for (var i = 0; i < this.Parameters.Count; i++)
                parameters[i] = row.Cell(i + 2).GetDouble();
            yield return new(wavelength, parameters);
            this.rowIndex++;
        }
    } // public IEnumerable<ParameterValues> ReadRows ()

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    } // public void Dispose ()

    protected virtual void Dispose(bool disposing)
    {
        if (this._disposed) return;

        if (disposing)
            this.workbook?.Dispose();
        this._disposed = true;
    } // protected virtual void Dispose (bool)
} // internal class ExcelReader
