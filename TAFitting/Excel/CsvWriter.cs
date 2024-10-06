
// (c) 2024 Kazuki KOHZUKI

using System.Text;
using TAFitting.Model;

namespace TAFitting.Excel;

/// <summary>
/// Represents a writer for a CSV file.
/// </summary>
internal sealed class CsvWriter : ISpreadSheetWriter
{
    private readonly List<RowData> rows = [];

    public IFittingModel Model { get; init; }

    public IReadOnlyList<string> Parameters { get; set; } = [];

    public IReadOnlyList<double> Times { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvWriter"/> class.
    /// </summary>
    /// <param name="model">The model.</param>
    internal CsvWriter(IFittingModel model)
    {
        this.Model = model;
    } // internal CsvWriter (IFittingModel)

    public void AddRow(double wavelength, IEnumerable<double> parameters)
    {
        var paramValues = parameters.ToArray();
        var func = this.Model.GetFunction(paramValues);
        var rowData = new RowData
        {
            Wavelength = wavelength,
            Values = this.Times.Select(func).ToArray(),
        };
        this.rows.Add(rowData);
    } // public void AddRow (double, IEnumerable<double>)

    public void Write(string path)
    {
        using var writer = new StreamWriter(path, false, Encoding.UTF8);
        writer.WriteLine("Wavelength (nm)," + string.Join(" µs,", this.Times) + " µs");
        foreach (var row in this.rows)
            writer.WriteLine(row.ToString());
    } // public void Write (string)

    private class RowData()
    {
        required internal double Wavelength { get; init; }
        required internal double[] Values { get; init; }

        public override string ToString()
            => $"{this.Wavelength},{string.Join(",", this.Values)}";
    } // private class RowData
} // internal sealed class CsvWriter : ISpreadSheetWriter
