
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Print;

/// <summary>
/// Represents the position of additional content.
/// </summary>
internal class AdditionalContent
{
    /// <summary>
    /// Gets or sets the position of the additional content.
    /// </summary>
    internal AdditionalContentPosition Position { get; set; } = AdditionalContentPosition.UpperLeft;

    /// <summary>
    /// Gets or sets the text of the additional content.
    /// </summary>
    internal string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the font of the additional content.
    /// </summary>
    /// <remarks>
    /// If this property is <see langword="null"/>, the default font will be used.
    /// </remarks>
    internal Font? Font { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdditionalContent"/> class.
    /// </summary>
    /// <param name="text">The text of the additional content.</param>
    internal AdditionalContent(string text) : this(text, AdditionalContentPosition.UpperLeft) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdditionalContent"/> class.
    /// </summary>
    /// <param name="text">The text of the additional content.</param>
    /// <param name="position">The position of the additional content.</param>
    internal AdditionalContent(string text, AdditionalContentPosition position)
    {
        this.Text = text;
        this.Position = position;
    } // ctor (string, AdditionalContentPosition)
} // internal class AdditionalContent
