
// (c) 2024-2026 Kazuki KOHZUKI

using ClosedXML.Excel;
using TAFitting.Excel.Formulas;
using TAFitting.Model;

namespace TAFitting.Excel;

/// <summary>
/// Represents a writer for an Excel spreadsheet.
/// </summary>
internal sealed partial class ExcelWriter : ISpreadSheetWriter
{
    private bool _disposed = false;

    private readonly string path;
    private readonly XLWorkbook workbook;
    private readonly IXLWorksheet worksheet;

    private readonly ExcelFormulaTemplate formulaTemplate;
    private readonly IReadOnlyList<double> times;
    private int rowIndex = 2;

    public IFittingModel Model { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcelWriter"/> class.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="model">The model.</param>
    /// <param name="times">The times.</param>
    internal ExcelWriter(string path, IFittingModel model, IReadOnlyList<double> times)
    {
        this.path = path;
        this.Model = model;
        this.formulaTemplate = ExcelFormulaTemplate.GetInstance(model);
        this.times = times;

        // Do not specify the file path here to avoid appending to an existing file.
        // Instead, save (overwrite) the file in Dispose().
        this.workbook = new();
        this.worksheet = this.workbook.AddWorksheet("TA Spectra");
        this.worksheet.Cell(1, 1).Value = "Wavelength / nm";
        this.worksheet.SheetView.Freeze(1, 1);

        for (var i = 0; i < model.Parameters.Count; i++)
        {
            var col = i + 2;
            var param = model.Parameters[i];
            this.worksheet.Cell(1, col).Value = param.Name;
        }

        for (var i = 0; i < times.Count; i++)
        {
            var col = model.Parameters.Count + i + 2;
            this.worksheet.Cell(1, col).Value = times[i];
        }
    } // internal ExcelWriter (string, IFittingModel, IReadOnlyList<double>)

    public void AddRow(double wavelength, IReadOnlyList<double> parameters)
    {
        if (parameters.Count != this.Model.Parameters.Count)
            throw new ArgumentException("The number of parameters does not match the model.", nameof(parameters));

        this.worksheet.Cell(this.rowIndex, 1).Value = wavelength;

        for (var i = 0; i < this.Model.Parameters.Count; i++)
        {
            var col = i + 2;
            this.worksheet.Cell(this.rowIndex, col).Value = parameters[i];
        }

        for (var i = 0; i < this.times.Count; i++)
        {
            var col = this.Model.Parameters.Count + i + 2;
            this.worksheet.Cell(this.rowIndex, col).FormulaA1 = this.formulaTemplate.ToFormula(this.rowIndex, col);
        }

        this.rowIndex++;
    } // public void AddRow (double, IEnumerable<double>)

    public void Dispose()
    {
        if (this._disposed) return;

        this.workbook.SaveAs(this.path);
        this.workbook.Dispose();

        this._disposed = true;
    } // public void Dispose()
} // internal sealed partial class ExcelWriter : ISpreadSheetWriter
