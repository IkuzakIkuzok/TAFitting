
// (c) 2024 Kazuki KOHZUKI

using TAFitting.Model;

namespace TAFitting.Excel;

internal sealed class CsvWriter : ISpreadSheetWriter
{
    private readonly List<RowData> rows = [];

    public IFittingModel Model { get; init; }

    public IReadOnlyList<string> Parameters { get; set; } = [];

    public IReadOnlyList<double> Times { get; set; } = [];

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
        using var writer = new StreamWriter(path);
        writer.WriteLine("Wavelength," + string.Join(",", this.Times));
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
