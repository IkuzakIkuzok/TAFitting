
// (c) 2024 Kazuki Kohzuki

using System.Xml.Serialization;

namespace TAFitting.Config;

/// <summary>
/// Represents the font configuration.
/// </summary>
[Serializable]
public sealed class FontConfig
{
    private Font font = new("Arial", 12.0f, FontStyle.Regular, GraphicsUnit.Point);

    /// <summary>
    /// Gets or sets the name of the font.
    /// </summary>
    [XmlElement("name")]
    public string Name
    {
        get => this.Font.Name;
        set
        {
            if (this.Font.Name == value) return;
            this.Font = new(value, this.Size, this.Style, this.Unit);
        }
    }

    /// <summary>
    /// Gets or sets the size of the font.
    /// </summary>
    [XmlElement("size")]
    public float Size
    {
        get => this.Font.Size;
        set
        {
            if (this.Font.Size == value) return;
            this.Font = new(this.Name, value, this.Style, this.Unit);
        }
    }

    /// <summary>
    /// Gets or sets the style of the font.
    /// </summary>
    [XmlElement("style")]
    public FontStyle Style
    {
        get => this.Font.Style;
        set
        {
            if (this.Font.Style == value) return;
            this.Font = new(this.Name, this.Size, value, this.Unit);
        }
    }

    /// <summary>
    /// Gets or sets the unit of the font.
    /// </summary>
    public GraphicsUnit Unit
    {
        get => this.Font.Unit;
        set
        {
            if (this.Font.Unit == value) return;
            this.Font = new(this.Name, this.Size, this.Style, value);
        }
    }

    /// <summary>
    /// Gets or sets the font.
    /// </summary>
    [XmlIgnore]
    public Font Font
    {
        get => this.font;
        set => this.font = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontConfig"/> class.
    /// </summary>
    public FontConfig() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontConfig"/> class
    /// with the specified font.
    /// </summary>
    /// <param name="font">The font.</param>
    public FontConfig(Font font)
    {
        this.Font = font;
    } // ctor (Font)

    public static implicit operator Font(FontConfig config)
        => config.Font;

    public static implicit operator FontConfig(Font font)
        => new(font);
} // public sealed class FontConfig
