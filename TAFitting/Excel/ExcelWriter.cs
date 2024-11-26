
// (c) 2024 Kazuki KOHZUKI

using ClosedXML.Excel;
using TAFitting.Model;

namespace TAFitting.Excel;

/// <summary>
/// Represents a writer for an Excel spreadsheet.
/// </summary>
internal partial class ExcelWriter : ISpreadSheetWriter, IDisposable
{
    private readonly XLWorkbook workbook;
    private readonly IXLWorksheet worksheet;

    private int parametersCount = 0;
    private int timesCount = 0;
    private int rowIndex = 2;

    private readonly Dictionary<string, int> parametersindices = [];

    public IFittingModel Model { get; init; }

    public IReadOnlyList<string> Parameters
    {
        get => this.worksheet.Range(1, 2, 1, this.parametersCount + 1).Cells()
            .Select(cell => cell.GetString())
            .ToArray();
        set
        {
            this.parametersCount = value.Count;
            this.parametersindices.Clear();
            for (var i = 0; i < value.Count; i++)
            {
                var index = i + 2;
                var name = value[i];
                this.worksheet.Cell(1, index).Value = name;
                this.parametersindices[name] = index;
            }
        }
    }

    public IReadOnlyList<double> Times
    {
        get => this.worksheet.Range(1, this.parametersCount + 2, 1, this.parametersCount + this.timesCount + 2).Cells()
            .Select(cell => cell.GetDouble())
            .ToArray();
        set
        {
            this.timesCount = value.Count;
            for (var i = 0; i < value.Count; i++)
                this.worksheet.Cell(1, this.parametersCount + i + 2).Value = value[i];
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcelWriter"/> class.
    /// </summary>
    /// <param name="model">The model.</param>
    internal ExcelWriter(IFittingModel model)
    {
        this.Model = model;
        this.workbook = new();
        this.worksheet = this.workbook.AddWorksheet("TA Spectra");
        this.worksheet.Cell(1, 1).Value = "Wavelength / nm";
        this.worksheet.SheetView.Freeze(1, 1);
    } // internal ExcelWriter (IFittingModel)

    public void AddRow(double wavelength, IEnumerable<double> parameters)
    {
        var parametersValues = parameters.ToArray();
        var formulaTemplate = this.Model.ExcelFormula;

        this.worksheet.Cell(this.rowIndex, 1).Value = wavelength;

        for (var i = 0; i < this.parametersCount; i++)
        {
            var col = i + 2;
            this.worksheet.Cell(this.rowIndex, col).Value = parametersValues[i];
        }

        for (var i = 0; i < this.timesCount; i++)
        {
            var col = this.parametersCount + i + 2;
            this.worksheet.Cell(this.rowIndex, col).FormulaA1 = GetFormula(formulaTemplate, this.rowIndex, col);
        }

        this.rowIndex++;
    } // public void AddRow (double, IEnumerable<double>)

    private string GetFormula(string template, int row, int column)
    {
        var time = GetColumnLetter(column) + "$1";
        template = template.Replace("$X", time);
        foreach ((var name, var index) in this.parametersindices)
        {
            var cell = "$" + GetColumnLetter(index) + row;
            template = template.Replace($"[{name}]", cell);
        }
        return template;
    } // private string GetFormula (string, int, int)

    private static string GetColumnLetter(int column)
    {
        var letter = "";

        while (column > 0)
        {
            var mod = (column - 1) % 26;
            letter = (char)('A' + mod) + letter;
            column = (column - mod) / 26;
        }
        return letter;
    } // private static string GetColumnLetter (int)
    
    public void Write(string path)
    {
        this.workbook.SaveAs(path);
    } // public void Write (string)

    public void Dispose()
    {
        ((IDisposable)this.workbook).Dispose();
    } // public void Dispose ()
} // internal partial class ExcelWriter : ISpreadSheetWriter, IDisposable
