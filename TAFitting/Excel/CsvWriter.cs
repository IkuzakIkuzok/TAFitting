
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
            Parameters = paramValues,
            Values = [.. this.Times.Select(func)],
        };
        this.rows.Add(rowData);
    } // public void AddRow (double, IEnumerable<double>)

    public void Write(string path)
    {
        using var writer = new StreamWriter(path, false, Encoding.UTF8);
        writer.Write("Wavelength (nm),");
        writer.Write(string.Join(",", this.Parameters));
        writer.Write(',');
        writer.Write(string.Join(" µs,", this.Times));
        writer.WriteLine(" µs");
        foreach (var row in this.rows)
            writer.WriteLine(row.ToString());
    } // public void Write (string)

    private class RowData()
    {
        required internal double Wavelength { get; init; }
        required internal double[] Parameters { get; init; }
        required internal double[] Values { get; init; }

        override public string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(this.Wavelength);
            builder.Append(',');
            builder.Append(string.Join(",", this.Parameters));
            builder.Append(',');
            builder.Append(string.Join(",", this.Values));
            return builder.ToString();
        } // public override string ToString ()
    } // private class RowData
} // internal sealed class CsvWriter : ISpreadSheetWriter
