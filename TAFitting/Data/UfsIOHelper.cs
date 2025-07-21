
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Data;

/// <summary>
/// Helps with reading and writing data to a stream in UFS format.
/// </summary>
internal abstract class UfsIOHelper : IDisposable
{
    internal static int Version => 2;

    protected readonly Stream _stream;
    private readonly bool _leaveOpen;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="UfsIOHelper"/> class with the specified stream.
    /// </summary>
    /// <param name="stream">The stream to read from or write to.</param>
    /// <param name="leaveOpen">A value indicating whether to leave the stream open after disposing this instance.</param>
    internal UfsIOHelper(Stream stream, bool leaveOpen = false)
    {
        this._stream = stream;
        this._leaveOpen = leaveOpen;
    } // ctor (Stream, [bool])

    public void Dispose()
    {
        Dispose(true);
    } // public void Dispose ()

    private void Dispose(bool disposing)
    {
        if (this._disposed) return;
        if (disposing && !this._leaveOpen)
#pragma warning disable IDE0079  // Remove unnecessary suppression
#pragma warning disable IDISP007 // Don't dispose injected
        this._stream.Dispose();
#pragma warning restore
        this._disposed = true;
    } // private void Dispose (bool)

    /// <summary>
    /// Normalizes new line characters in the input string.
    /// </summary>
    /// <param name="s">The input string to normalize.</param>
    /// <returns>The normalized string with new line characters replaced.</returns>
    /// <remarks>
    /// All occurrences of `CR` and `LF` characters are replaced with `CRLF`.
    /// </remarks>
    protected static string NormNewLineInput(string s)
    {
        s = s.Replace("\r\n", "\n", StringComparison.InvariantCulture);
        s = s.Replace("\r", "\n", StringComparison.InvariantCulture);
        return s.Replace("\n", "\r\n", StringComparison.InvariantCulture);
    } // internal static string NormNewLineInput (string)

    /// <summary>
    /// Normalizes new line characters in the output string for UFS format.
    /// </summary>
    /// <param name="s">The output string to normalize.</param>
    /// <returns>The normalized string with new line characters replaced.</returns>
    /// <remarks>
    /// All occurrences of `CRLF` characters are replaced with `LF`,
    /// as Surface Xplorer only supports `LF` line endings in UFS files.
    /// </remarks>
    protected static string NormalizeNewLineOutUfs(string s)
        => s.Replace("\r\n", "\n", StringComparison.InvariantCulture);

    /// <summary>
    /// Normalizes new line characters in the output string for CSV format.
    /// </summary>
    /// <param name="s">The output string to normalize.</param>
    /// <returns>The normalized string with new line characters replaced.</returns>
    /// <remarks>
    /// All occurrences of `CRLF` characters are replaced with `CR`, which is compatible with CSV outputs from Surface Xplorer.
    /// Although other new line characters like `LF` are more common in CSV files,
    /// CR shoud be used from the perspective of equivalence when converting multiple times.
    /// </remarks>
    protected static string NormalizeNewLineOutCsv(string s)
        => s.Replace("\r\n", "\r", StringComparison.InvariantCulture);
} // internal abstract class UfsIOHelper : IDisposable
