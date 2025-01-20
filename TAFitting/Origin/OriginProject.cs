
// (c) 2024 Kazuki KOHZUKI

using DisposalGenerator;
using System.Diagnostics.CodeAnalysis;

namespace TAFitting.Origin;

/// <summary>
/// Wraps an Origin project.
/// </summary>
[AutoDisposal(UnmanagedDisposalMethod = nameof(ReleaseUnmanagedResources))]
internal sealed partial class OriginProject
{
    private const string ProgID = "Origin.ApplicationSI";

    private static readonly Type? ProgType = Type.GetTypeFromProgID(ProgID);

    private readonly dynamic app;

    /// <summary>
    /// Gets a value indicating whether Origin is available.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if Origin is available; otherwise, <see langword="false"/>.
    /// </value>
    [MemberNotNullWhen(true, nameof(ProgType))]
    internal static bool IsAvailable => ProgType is not null;

    /// <summary>
    /// Gets or sets a value indicating whether the project is modified.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the project is modified; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Setting this property to <see langword="false"/> doen NOT save the project.
    /// Call <see cref="Save(string)"/> method to save the project.
    /// </remarks>
    internal bool IsModified
    {
        get => this.app.IsModified;
        set => this.app.IsModified = value;
    }

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
    /// Empties all worksheets and fill all matrices with missing-value.
    /// Size of all worksheets are adjusted to the default size (30),
    /// and size of all matrices are adjusted to the default size (2x2).
    /// </summary>
    /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
    internal bool Reset()
        => Reset(true, true);

    /// <summary>
    /// Empties all worksheets and fill all matrices with missing-value.
    /// </summary>
    /// <param name="reduceWorksheets"><see langword="true"/> to adjust the size of worksheets to thier default size; otherwise, <see langword="false"/>.</param>
    /// <param name="reduceMatrices"><see langword="true"/> to adjust the size of matrices to their default size; otherwise, <see langword="false"/>.</param>
    /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
    internal bool Reset(bool reduceWorksheets, bool reduceMatrices)
        => this.app.Reset(reduceWorksheets, reduceMatrices);

    /// <summary>
    /// Closes the current project and starts a new project.
    /// </summary>
    /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
    internal bool NewProject()
        => this.app.NewProject();

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

    private void ReleaseUnmanagedResources()
    {
        this.app.Exit();
        Marshal.ReleaseComObject(this.app);
    } // private void ReleaseUnmanagedResources ()
} // internal sealed class OriginProject
