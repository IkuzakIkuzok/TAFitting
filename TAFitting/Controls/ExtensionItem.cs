
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Controls;

/// <summary>
/// Represents an extension filter for file dialogs.
/// </summary>
/// <param name="Name">The name of the filter.</param>
/// <param name="Extension">The file extension pattern.</param>
/// <remarks>
/// <see cref="Extension"/> should start with a period and without a leading asterisk (e.g., ".txt").
/// </remarks>
internal record ExtensionItem(string Name, string Extension)
{
    internal static ExtensionItem AllFiles => new("All files", ".*");

    internal static ExtensionItem AssemblyFiles => new("Assembly files", ".dll");
    internal static ExtensionItem CsvFiles => new("CSV files", ".csv");
    internal static ExtensionItem ExcelFiles => new("Excel files", ".xlsx");
    internal static ExtensionItem FsTasFiles => new("fs-TAS data files", ".ufs;.csv");
    internal static ExtensionItem OriginProject => new("Origin project files", ".opju");
    internal static ExtensionItem TextFiles => new("Text files", ".txt");
    internal static ExtensionItem UfsFiles => new("Ultrafast System files", ".ufs");

    override public string ToString()
        => $"{this.Name}|{AddAsterisk(this.Extension)}";

    private static string AddAsterisk(string extension)
        => string.Join(";", extension.Split(';').Select(ext => $"*" + ext));
} // internal record ExtensionItem (string, string)
