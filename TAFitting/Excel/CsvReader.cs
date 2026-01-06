
// (c) 2025 Kazuki KOHZUKI

using System.Text;
using TAFitting.Controls;
using TAFitting.Data;
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

    public bool IsOpened => this.reader is not null && !this._disposed;

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

            // Maximum 128 KB, 65536 characters
            var buffer = (stackalloc char[0x10000]);
            var eol = this.reader.ReadLine(buffer, out var l);
            if (l == 0)
            {
                this.ModelMatched = false;
                return;
            }

            /*
             * If the number of columns is sufficient, it may not be necessary to read the remaining rows, but the last parameter name might be truncated.
             * It is possible to implement a fallback that reads the rest of the line if the parameter name does not match and the length is insufficient,
             * but the implementation cost is too high, so we read to the end of the line every time.
             */

            var span = eol ? buffer[..l] : ReadRemainingLine(buffer).AsSpan();
            var paramsCount = span.Count(',');
            if (paramsCount < this.Parameters.Count)
            {
                this.ModelMatched = false;
                return;
            }

            var start = span.IndexOf(',') + 1;
            var idx_p = 0;
            for (var i = start; i < span.Length; i++)
            {
                if (i != span.Length && span[i] != ',')
                    continue;

                var length = i - start;
                if (length == 0)
                {
                    this.ModelMatched = false;
                    return;
                }
                var segment = span.Slice(start, length);
                var name = this.Parameters[idx_p++];
                if (!segment.SequenceEqual(name))
                {
                    this.ModelMatched = false;
                    return;
                }
                if (idx_p == this.Parameters.Count) break;
                start = i + 1;
            }

            this.ModelMatched = true;
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

        // Maximum 128 KB, 65536 characters
        var buffer = (stackalloc char[0x10000]);
        var eol = this.reader.ReadLine(buffer, out var l);
        if (l == 0)
            goto Error;

        var span = eol ? buffer[..l] : ReadRemainingLine(buffer).AsSpan();
        var sep = span.IndexOf(',');
        if (sep <= 0)
            goto Error;

        var s_wavelength = span[..sep];
        var s_values = span[(sep + 1)..];

        if (!double.TryParse(s_wavelength, out wavelength))
            goto Error;

        if (NegativeSignHandler.ParseDoubles(s_values, ',', parameters) != parameters.Length)
            goto Error;

        return true;

    Error:
        wavelength = double.NaN;
        return false;
    } // public bool ReadNextRow (out double, Span<double>)

    private string ReadRemainingLine(ReadOnlySpan<char> buffer)
    {
        var builder = new StringBuilder();
        builder.Append(buffer);
        
        var s = (stackalloc char[0x400]);
        var eol = this.reader!.ReadLine(s, out var read);
        while (read > 0)
        {
            if (eol)
            {
                builder.Append(s[..read]);
                if (read < s.Length)
                    break;
            }
            builder.Append(s);
            eol = this.reader!.ReadLine(s, out read);
        }

        return builder.ToString();
    } // private string ReadRemainingLine (ReadOnlySpan<char>)

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
