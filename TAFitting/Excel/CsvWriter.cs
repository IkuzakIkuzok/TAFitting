
// (c) 2024-2026 Kazuki KOHZUKI

using DisposalGenerator;
using System.Text;
using TAFitting.Model;

namespace TAFitting.Excel;

/// <summary>
/// Represents a writer for a CSV file.
/// </summary>
[AutoDisposal]
internal sealed partial class CsvWriter : ISpreadSheetWriter
{
    private readonly StreamWriter writer;
    private readonly IReadOnlyList<double> times;

    public IFittingModel Model { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvWriter"/> class.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="model">The model.</param>
    /// <param name="times">The times.</param>
    /// <param name="timeUnit">The time unit.</param>
    internal CsvWriter(string path, IFittingModel model, IReadOnlyList<double> times, string timeUnit)
    {
        this.writer = new StreamWriter(path, false, Encoding.UTF8);
        this.Model = model;
        this.times = times;

        this.writer.Write("Wavelength (nm)");
        for (var i = 0; i < model.Parameters.Count; i++)
        {
            this.writer.Write(',');
            this.writer.Write(model.Parameters[i].Name);
        }

        for (var i = 0; i < times.Count; i++)
        {
            this.writer.Write(',');
            this.writer.Write(times[i]);
            this.writer.Write(' ');
            this.writer.Write(timeUnit);
        }
        this.writer.WriteLine();
    } // internal CsvWriter (string, IFittingModel, IReadOnlyList<double>, string)

    public void AddRow(double wavelength, IReadOnlyList<double> parameters)
    {
        if (parameters.Count != this.Model.Parameters.Count)
            throw new ArgumentException("The number of parameters does not match the model.", nameof(parameters));

        this.writer.Write(wavelength);
        for (var i = 0; i < this.Model.Parameters.Count; i++)
        {
            this.writer.Write(',');
            this.writer.Write(parameters[i]);
        }

        var func = this.Model.GetFunction(parameters);
        for (var i = 0; i < this.times.Count; i++)
        {
            this.writer.Write(',');
            this.writer.Write(func(this.times[i]));
        }
        this.writer.WriteLine();
    } // public void AddRow (double, IReadOnlyList<double>)
} // internal sealed partial class CsvWriter : ISpreadSheetWriter
