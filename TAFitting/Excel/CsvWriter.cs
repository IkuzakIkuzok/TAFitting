
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

        writer.Write("Wavelength (nm)");
        for (var i = 0; i < this.Parameters.Count; i++)
        {
            writer.Write(',');
            writer.Write(this.Parameters[i]);
        }
        for (var i = 0; i < this.Times.Count; i++)
        {
            writer.Write(',');
            writer.Write(this.Times[i]);
            writer.Write(" µs");
        }
        writer.WriteLine();

        foreach (var row in this.rows)
            row.WriteTo(writer);
    } // public void Write (string)

    private class RowData()
    {
        required internal double Wavelength { get; init; }
        required internal double[] Parameters { get; init; }
        required internal double[] Values { get; init; }

        internal void WriteTo(StreamWriter writer)
        {
            writer.Write(this.Wavelength);
            for (var i = 0; i < this.Parameters.Length; i++)
            {
                writer.Write(',');
                writer.Write(this.Parameters[i]);
                
            }
            for (var i = 0; i < this.Values.Length; i++)
            {
                writer.Write(',');
                writer.Write(this.Values[i]);
            }
            writer.WriteLine();
        } // internal void WriteTo (StreamWriter)
    } // private class RowData
} // internal sealed class CsvWriter : ISpreadSheetWriter
