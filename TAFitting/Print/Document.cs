
// (c) 2025 Kazuki Kohzuki

using System.Drawing.Printing;

namespace TAFitting.Print;

/// <summary>
/// Represents a document for printing.
/// </summary>
[DesignerCategory("Code")]
internal class Document : PrintDocument
{
    /// <summary>
    /// Gets or sets the font name.
    /// </summary>
    internal string FontName { get; set; } = "Arial";

    /// <summary>
    /// Gets or sets the font size.
    /// </summary>
    /// <remarks>
    /// Actual font size may be smaller than this value to fit the table in the page.
    /// </remarks>
    internal float FonrSize { get; set; } = 12;

    /// <summary>
    /// Gets or sets the additional contents.
    /// </summary>
    internal AdditionalContentCollection AdditionalContents { get; set; } = [];

    override protected void OnPrintPage(PrintPageEventArgs e)
    {
        if (e.Graphics is null) return;

        var leftMargin = e.MarginBounds.Left;
        var topMargin = e.MarginBounds.Top;
        var docWidth = e.MarginBounds.Width;
        var docHeight = e.MarginBounds.Height;

        using var brush = new SolidBrush(Color.Black);

        foreach (var content in this.AdditionalContents)
        {
            using var f = content.Font ?? new Font(this.FontName, this.FonrSize);
            var s = content.Text;
            var size = e.Graphics.MeasureString(s, f);
            var w = size.Width;
            var h = size.Height;
            var cx = content.Position switch
            {
                AdditionalContentPosition.UpperLeft  => leftMargin,
                AdditionalContentPosition.UpperRight => leftMargin + docWidth - w,
                AdditionalContentPosition.LowerLeft  => leftMargin,
                AdditionalContentPosition.LowerRight => leftMargin + docWidth - w,
                _ => leftMargin
            };
            var cy = content.Position switch
            {
                AdditionalContentPosition.UpperLeft  => topMargin - h,
                AdditionalContentPosition.UpperRight => topMargin - h,
                AdditionalContentPosition.LowerLeft  => topMargin + docHeight,
                AdditionalContentPosition.LowerRight => topMargin + docHeight,
                _ => topMargin
            };
            e.Graphics.DrawString(s, f, brush, cx, cy);
        }
    } // OnPrintPage (PrintPageEventArgs)
} // internal class Document
