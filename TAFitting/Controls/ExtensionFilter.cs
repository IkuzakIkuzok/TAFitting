
// (c) 2025 Kazuki Kohzuki

using System.Collections;
using System.Runtime.CompilerServices;

namespace TAFitting.Controls;

/// <summary>
/// Represents an extension filter for file dialogs.
/// </summary>
[CollectionBuilder(typeof(ExtensionFilter), nameof(Create))]
internal sealed partial class ExtensionFilter : IEnumerable<ExtensionItem>
{
    private readonly List<ExtensionItem> _extensions = [];

    internal static ExtensionFilter Assemblies => [ExtensionItem.AssemblyFiles, ExtensionItem.AllFiles];
    internal static ExtensionFilter CsvFiles => [ExtensionItem.CsvFiles, ExtensionItem.AllFiles];
    internal static ExtensionFilter FsTasFiles => [ExtensionItem.FsTasFiles, ExtensionItem.UfsFiles, ExtensionItem.CsvFiles, ExtensionItem.AllFiles];
    internal static ExtensionFilter OriginProjects => [ExtensionItem.OriginProject, ExtensionItem.AllFiles];
    internal static ExtensionFilter SpreadSheets => [ExtensionItem.ExcelFiles, ExtensionItem.CsvFiles, ExtensionItem.AllFiles];
    internal static ExtensionFilter TextFiles => [ExtensionItem.TextFiles, ExtensionItem.AllFiles];

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionFilter"/> class.
    /// </summary>
    internal ExtensionFilter() : this([]) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionFilter"/> class
    /// with the specified extension items.
    /// </summary>
    /// <param name="items">The extension items to include in the filter.</param>
    internal ExtensionFilter(ReadOnlySpan<ExtensionItem> items)
    {
        this._extensions.AddRange(items);
    } // ctor (ReadOnlySpan<ExtensionItem>)

    internal static ExtensionFilter Create(ReadOnlySpan<ExtensionItem> items)
        => new(items);

    override public string ToString()
        => string.Join("|", this._extensions);

    public static implicit operator string(ExtensionFilter filter)
        => filter.ToString();

    public IEnumerator<ExtensionItem> GetEnumerator()
        => this._extensions.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => this._extensions.GetEnumerator();
} // internal sealed partial class ExtensionFilter : IEnumerable<ExtensionItem>
