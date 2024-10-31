
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Origin;

/// <summary>
/// Wraps a workbook.
/// </summary>
internal class Workbook
{
    private readonly dynamic workbook;
    private readonly Worksheets worksheets;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    internal string Name
    {
        get => this.workbook.Name;
        set => this.workbook.Name = value;
    }

    /// <summary>
    /// Gets the worksheets.
    /// </summary>
    internal Worksheets Worksheets => this.worksheets;

    /// <summary>
    /// Initializes a new instance of the <see cref="Workbook"/> class.
    /// </summary>
    /// <param name="workbook">The workbook.</param>
    internal Workbook(dynamic workbook)
    {
        this.workbook = workbook;
        this.worksheets = new(this.workbook.Layers);
    } // ctor (dynamic)
} // internal class Workbook
