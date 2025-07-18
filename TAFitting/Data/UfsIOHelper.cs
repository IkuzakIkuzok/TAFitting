
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Data;

internal abstract class UfsIOHelper : IDisposable
{
    internal static int Version => 2;

    protected readonly Stream _stream;
    private readonly bool _leaveOpen;
    private bool _disposed;

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

    protected static string NormNewLineInput(string s)
    {
        s = s.Replace("\r\n", "\n", StringComparison.InvariantCulture);
        s = s.Replace("\r", "\n", StringComparison.InvariantCulture);
        return s.Replace("\n", "\r\n", StringComparison.InvariantCulture);
    } // internal static string NormNewLineInput (string)

    protected static string NormalizeNewLineOutUfs(string s)
        => s.Replace("\r\n", "\n", StringComparison.InvariantCulture);

    protected static string NormalizeNewLineOutCsv(string s)
        => s.Replace("\r\n", "\r", StringComparison.InvariantCulture);
} // internal abstract class UfsIOHelper : IDisposable
