
// (c) 2024 Kazuki KOHZUKI

using System.Diagnostics.CodeAnalysis;

namespace TAFitting.Origin;

/// <summary>
/// Wraps an Origin project.
/// </summary>
internal sealed class OriginProject : IDisposable
{
    private const string ProgID = "Origin.ApplicationSI";

    private static readonly Type? ProgType = Type.GetTypeFromProgID(ProgID);

    private readonly dynamic app;

    private bool _disposed = false;

    /// <summary>
    /// Gets a value indicating whether Origin is available.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if Origin is available; otherwise, <see langword="false"/>.
    /// </value>
    [MemberNotNullWhen(true, nameof(ProgType))]
    internal static bool IsAvailable => ProgType is not null;

    /// <summary>
    /// Initializes a new instance of the <see cref="OriginProject"/> class.
    /// </summary>
    internal OriginProject()
    {
        this.app = CreateObject()!;
        if (this.app is null) ThrowCOMException();
    } // ctor ()

    /// <summary>
    /// Initializes a new instance of the <see cref="OriginProject"/> class with the specified path.
    /// </summary>
    /// <param name="path">The path of the Origin project.</param>
    internal OriginProject(string path) : this()
    {
        Load(path);
    } // internal OriginProject (string)

    /// <summary>
    /// Loads an Origin project from the specified path.
    /// </summary>
    /// <param name="path">The path of the Origin project.</param>
    internal void Load(string path)
    {
        this.app.Load(path);
    } // public void Load (string)

    /// <summary>
    /// Saves the Origin project to the specified path.
    /// </summary>
    /// <param name="path">The path of the Origin project.</param>
    internal void Save(string path)
    {
        this.app.Save(path);
    } // public void Save (string)

    /// <summary>
    /// Adds a workbook.
    /// </summary>
    /// <returns>The added workbook.</returns>
    internal Workbook AddWorkbook()
        => new(this.app.WorkSheetPages.Add("Origin"));

    /// <summary>
    /// Adds a graph page.
    /// </summary>
    /// <param name="name">The name of the graph page.</param>
    /// <param name="templateName">The name of the template.</param>
    /// <returns>The added graph.</returns>
    internal GraphPage AddGraph(string name, string templateName)
    {
        name = new(this.app.CreatePage(PageType.Graph, name, templateName));
        return new(this.app.GraphPages[name]);
    } // internal GraphPage AddGraph (string, string)

    private static object? CreateObject()
    {
        if (ProgType is null) return null;
        return Activator.CreateInstance(ProgType);
    } // CreateObject ()

    [DoesNotReturn]
    private static void ThrowCOMException()
    {
        throw new COMException("Origin is not available.");
    } // private static void ThrowCOMException ()

    public void Dispose()
        => Dispose(true);

    private void Dispose(bool disposing)
    {
        if (this._disposed) return;
        if (disposing)
        {
            this.app.Exit();
            Marshal.ReleaseComObject(this.app);
        }
        this._disposed = true;
    } // private void Dispose (bool
} // internal sealed class OriginProject : IDisposable
