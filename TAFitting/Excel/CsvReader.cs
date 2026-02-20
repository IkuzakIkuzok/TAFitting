
// (c) 2025 Kazuki KOHZUKI

using DisposalGenerator;
using System.Text;
using TAFitting.IO;
using TAFitting.Model;

namespace TAFitting.Excel;

/// <summary>
/// Represents a reader for a CSV file.
/// </summary>
[AutoDisposal]
internal sealed partial class CsvReader : ISpreadSheetReader
{
    private CsvParser? reader;

    /// <inheritdoc/>
    public IFittingModel Model { get; init; }

    public bool IsOpened => this.reader is not null;

    /// <inheritdoc/>
    public bool ModelMatched { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyList<string> Parameters { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvReader"/> class.
    /// </summary>
    /// <param name="model">The model.</param>
    internal CsvReader(IFittingModel model)
    {
        this.Model = model;
        this.Parameters = model.Parameters.Names;
    } // ctor (IFittingModel)

    /// <inheritdoc/>
    public void Open(string path)
    {
        if (this.reader is not null)
            throw new InvalidOperationException("The CSV file is already opened.");

        try
        {
#pragma warning disable IDE0079
#pragma warning disable IDISP003  // Assignment is safe as the reader is null here.
            this.reader = new(path, Encoding.UTF8);
#pragma warning restore

            this.reader.SkipColumns(1); // Skip the wavelength column
            this.ModelMatched = this.reader.VerifyHeader(this.Model.Parameters.NamesAsSpan);
        }
        catch
        {
            this.ModelMatched = false;
        }
    } // public void Open (string)

    public bool ReadNextRow(out double wavelength, Span<double> parameters)
    {
        if (this.reader is null)
            throw new InvalidOperationException("The CSV file is not opened.");
        if (!this.ModelMatched)
            throw new InvalidOperationException("The model does not match with the CSV file.");
        if (parameters.Length != this.Parameters.Count)
            throw new ArgumentException("The length of the parameters span does not match the number of parameters.", nameof(parameters));

        var readCols = this.Parameters.Count + 1;
        var columns = (stackalloc double[readCols]);
        var n = this.reader.ParseLine(columns);
        if (n != readCols)
        {
            // Failed to read the expected number of columns, which may indicate a malformed line or end of file.
            goto Error;
        }

        wavelength = columns[0];
        columns[1..].CopyTo(parameters);

        return true;

    Error:
        wavelength = double.NaN;
        return false;
    } // public bool ReadNextRow (out double, Span<double>)
} // internal sealed partial class CsvReader : ISpreadSheetReader
