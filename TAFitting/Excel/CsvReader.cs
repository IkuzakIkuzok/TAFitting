
// (c) 2025 Kazuki KOHZUKI

using System.Text;
using TAFitting.Controls;
using TAFitting.Model;

namespace TAFitting.Excel;

/// <summary>
/// Represents a reader for a CSV file.
/// </summary>
internal sealed class CsvReader : ISpreadSheetReader, IDisposable
{
    private StreamReader? reader;
    private bool _disposed = false;

    /// <inheritdoc/>
    public IFittingModel Model { get; init; }

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
            this.reader = new StreamReader(path, Encoding.UTF8);
#pragma warning restore

            var header = this.reader.ReadLine();
            if (string.IsNullOrEmpty(header))
            {
                this.ModelMatched = false;
                return;
            }

            var columns = header.Split(',');
            if (columns.Length - 1 < this.Parameters.Count)
            {
                this.ModelMatched = false;
                return;
            }

            var parameters = columns.AsSpan(1, this.Parameters.Count);
            for (var i = 0; i < this.Parameters.Count; i++)
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
    public IEnumerable<SpreadSheetRow> ReadRows()
    {
        if (this.reader is null)
            throw new InvalidOperationException("The CSV file is not opened.");
        if (!this.ModelMatched)
            throw new InvalidOperationException("The model does not match with the CSV file.");

        string? line;
        while ((line = this.reader.ReadLine()) is not null)
        {
            var fields = line.Split(',');
            if (fields.Length - 1 < this.Parameters.Count)
                continue;

            if (!double.TryParse(fields[0], out var wavelength))
                continue;

            var parameters = new double[this.Parameters.Count];
            var values = parameters.AsSpan();
            if (!NegativeSignHandler.TryParseDoubles(fields.AsSpan(1, this.Parameters.Count), values))
                continue;

            yield return new SpreadSheetRow(wavelength, parameters);
        }
    } // public IEnumerable<SpreadSheetRow> ReadRows ()

    public void Dispose()
    {
        Dispose(true);
    } // public void Dispose ()

    private void Dispose(bool disposing)
    {
        if (this._disposed) return;

        if (disposing)
            this.reader?.Dispose();

        this._disposed = true;
    } // private void Dispose (bool)
} // internal sealed class CsvReader : ISpreadSheetReader, IDisposable
