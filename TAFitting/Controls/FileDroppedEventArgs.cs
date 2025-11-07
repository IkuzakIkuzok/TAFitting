
// (c) 2025 Kazuki KOHZUKI

namespace TAFitting.Controls;

delegate void FileDroppedEventHandler(object? sender, FileDroppedEventArgs e);

/// <summary>
/// Provides data for the file dropped event.
/// </summary>
internal class FileDroppedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the file path of the dropped file.
    /// </summary>
    internal string FilePath { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDroppedEventArgs"/> class.
    /// </summary>
    /// <param name="filePath"></param>
    internal FileDroppedEventArgs(string filePath)
    {
        this.FilePath = filePath;
    } // ctor (string)
} // internal class FileDroppedEventArgs : EventArgs
