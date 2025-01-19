
// (c) 2025 Kazuki Kohzuki

using System.Drawing.Printing;
using System.Text;
using System.Text.RegularExpressions;
using TAFitting.Controls;

namespace TAFitting.Print;

/// <summary>
/// Represents a document for printing the summary of spectra.
/// </summary>
[DesignerCategory("Code")]
internal sealed partial class SpectraSummaryDocument : Document
{
    private const float MARGIN = 20;
    private const float MARGIN_CELL = 5;

    [GeneratedRegex(@"(?<mantissa>.*)(E(?<exponent>.*))")]
    private static partial Regex RegexExpFormat();

    private static readonly Regex re_expFormat = RegexExpFormat();

    private readonly Bitmap plot;
    private readonly string[] parameters;
    private readonly Dictionary<double, double[]> values;

    /// <summary>
    /// Gets or sets the baseline skip.
    /// </summary>
    internal float BaselineSkip { get; set; } = 1.2f;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpectraSummaryDocument"/> class.
    /// </summary>
    /// <param name="plot">The plot.</param>
    /// <param name="parameters">The parameters.</param>
    /// <param name="values">The values.</param>
    internal SpectraSummaryDocument(Bitmap plot, string[] parameters, Dictionary<double, double[]> values)
    {
        this.plot = plot;
        this.parameters = parameters;
        this.values = values;
    } // ctor (Bitmap, string[], Dictionary<double, double[]>)

    override protected void OnPrintPage(PrintPageEventArgs e)
    {
        base.OnPrintPage(e);

        if (e.Graphics is null) return;

        var leftMargin = e.MarginBounds.Left;
        var topMargin = e.MarginBounds.Top;
        var docWidth = e.MarginBounds.Width;
        var docHeight = e.MarginBounds.Height;

        // Draw the plot
        var plotWidth = Math.Min(this.plot.Width, docWidth);
        var plotHeight = plotWidth * this.plot.Height / this.plot.Width;
        var plotLeftMargin = (docWidth - plotWidth) / 2;
        e.Graphics.DrawImage(this.plot, leftMargin + plotLeftMargin, topMargin, plotWidth, plotHeight);

        var height = e.MarginBounds.Height - plotHeight - MARGIN;

        // Determine the font size for the table
        var fontSize = this.FonrSize;
        var thead = "Wavelength" + string.Join("", this.parameters);
        while (fontSize > 4)
        {
            using var f = new Font(this.FontName, fontSize);
            if (e.Graphics.MeasureString(thead, f).Width <= docWidth)
                break;
            fontSize -= 0.5f;
        }
        var nrow = this.values.Count + 1;  // +1 for the header
        while (fontSize > 4)
        {
            using var f = new Font(this.FontName, fontSize);
            if (e.Graphics.MeasureString("Wavelength", f).Height * nrow * this.BaselineSkip <= height)
                break;
            fontSize -= 0.5f;
        }

        // Draw the table
        using var font = new Font(this.FontName, fontSize);
        var size = e.Graphics.MeasureString("Wavelength", font);
        using var brush = new SolidBrush(Color.Black);
        
        var dx = (docWidth - e.Graphics.MeasureString(thead, font).Width) / (this.parameters.Length + 1);
        dx = Math.Min(dx, size.Width);
        var dy = size.Height * this.BaselineSkip;

        var xOffset = 0f;

        var x = leftMargin + xOffset;
        var y = topMargin + plotHeight + MARGIN;

        var pos = new float[this.parameters.Length];
        var ws = new float[this.parameters.Length];
        e.Graphics.DrawString("Wavelength", font, brush, x, y);
        x += size.Width;
        foreach ((var i, var p) in this.parameters.Enumerate())
        {
            x += dx;
            pos[i] = x;
            var w = ws[i] = e.Graphics.MeasureString(p, font).Width;
            e.Graphics.DrawString(p, font, brush, x + dx / 2, y);
            x += w;
        }
        var tableRight = x + dx;
        using var blackPen = new Pen(Color.Black);
        void DrawHorizontalLine(float ypos)
        {
            // Adjust the baseline to the center of the text
            var yOffset = size.Height * (this.BaselineSkip - 1) / 2;
            ypos -= yOffset;
            e.Graphics!.DrawLine(blackPen, leftMargin + xOffset, ypos, tableRight, ypos);
        }

        DrawHorizontalLine(y);  // toprule
        y += dy;
        DrawHorizontalLine(y);  // midrule

        foreach ((var w, var v) in this.values)
        {
            x = leftMargin + xOffset;
            var wl = w.ToString();
            var offset = size.Width - e.Graphics.MeasureString(wl, font).Width - MARGIN_CELL;
            e.Graphics.DrawString(wl, font, brush, x + offset, y);
            foreach ((var i, var p) in this.parameters.Enumerate())
            {
                x = pos[i];
                var max = ws[i] + dx - MARGIN_CELL;
                var s = FormatValue(v[i], s => e.Graphics.MeasureString(s, font).Width <= max);
                var o = max - e.Graphics.MeasureString(s, font).Width;
                e.Graphics.DrawString(s, font, brush, x + o, y);
            }
            y += dy;
        }
        DrawHorizontalLine(y);  // bottomrule

        e.HasMorePages = false;
    } // override protected void OnPrintPage (PrintPageEventArgs)

    private static string FormatValue(double value, Func<string, bool> checkWidth)
    {
        if (value == 0) return "0";
        if (double.IsInfinity(value)) return "∞";
        if (double.IsNegativeInfinity(value)) return "-∞";
        if (double.IsNaN(value)) return "NaN";

        if (Math.Abs(value) is <= 1e-3 or >= 1e3) return ExpFormatValue(value, checkWidth);

        var n = 3;
        while (n > 0)
        {
            var s = value.ToString($"F{n}");
            if (checkWidth(s)) return s;
            n--;
        }

        return value.ToString("F");
    } // private static string FormatValue (double, Func<string, bool>)

    private static string ExpFormatValue(double value, Func<string, bool> checkWidth)
    {
        var n = 3;
        while (n > 0)
        {
            var s = ExpFormatValue(value, n--);
            if (checkWidth(s)) return s;
        }
        return value.ToString("E2");
    } // private static string ExpFormatValue (double, Func<string, bool>)

    private static string ExpFormatValue(double value, int n)
    {
        var s = value.ToString($"E{n}");

        Match? match;
        try
        {
            match = re_expFormat.Match(s);
        }
        catch (RegexMatchTimeoutException)
        {
            return s;
        }

        if (!(match?.Success ?? false)) return s;

        var mantissa = match.Groups["mantissa"].Value;
        var exponent = match.Groups["exponent"].Value;

        var sb = new StringBuilder(mantissa);
        sb.Append("×10");
        if (exponent.StartsWith(NegativeSignHandler.NegativeSign))
            sb.Append('⁻');  // U+207B

        var e = exponent[1..].TrimStart('0');
        if (e.Length == 0)
        {
            sb.Append('⁰');  // U+2070
        }
        else
        {
            // append unicode superscript
            foreach (var c in e)
            {
                /*
                 * Superscript of 1, 2, 3 are located in U+00Bx,
                 * whereas the rest in U+207x.
                 */
                sb.Append(c switch
                {
                    '1' => '¹',  // U+00B9
                    '2' => '²',  // U+00B2
                    '3' => '³',  // U+00B3
                    _ => (char)(c + 0x2040)  // U+2070 - U+2079
                });
            }
        }

        return sb.ToString();
    } // private static string ExpFormatValue (double, int)
} // internal sealed partial class Document
