
// (c) 2024 Kazuki Kohzuki

using TAFitting.Data;
using System.Windows.Forms.DataVisualization.Charting;
using System.Text.RegularExpressions;
using System.Text;

namespace TAFitting.Controls;

/// <summary>
/// Provides utility methods for UI.
/// </summary>
internal static partial class UIUtils
{
    private const double DecimalMin = -7.9e28;
    private const double DecimalMax = +7.9e28;

    [GeneratedRegex(@"(?<mantissa>.*)(E(?<exponent>.*))")]
    private static partial Regex RegexExpFormat();

    private static readonly Regex re_expFormat = RegexExpFormat();

    /// <summary>
    /// Formats the specified value in exponential notation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>String representation of the value in exponential notation.</returns>
    // Constant formant is used and no exception is thrown with this format.
    // ExceptionAdjustment: M:System.Decimal.ToString(System.String) -T:System.FormatException
    internal static string ExpFormatter(decimal value)
    {
        var s = value.ToString("E2");

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
    } // internal static string ExpFormatter (decimal)


    /// <summary>
    /// Adds the specified decay to the data points.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <param name="decay">The decay.</param>
    internal static void AddDecay(this DataPointCollection points, Decay decay)
        => points.AddDecay((IEnumerable<(double, double)>)decay);

    /// <summary>
    /// Adds the specified decay to the data points.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <param name="times">The time series.</param>
    /// <param name="signals">The signal series.</param>
    internal static void AddDecay(this DataPointCollection points, IEnumerable<double> times, IEnumerable<double> signals)
        => points.AddDecay(times.Zip(signals));

    private static void AddDecay(this DataPointCollection points, IEnumerable<(double, double)> signals)
    {
        foreach (var (time, signal) in signals)
        {
            if (time <= 0) continue;
            if (double.IsNaN(signal)) continue;
            var y = Math.Clamp(signal, DecimalMin, DecimalMax);
            points.AddXY(time, y);
        }
    } // private static void AddDecay (this DataPointCollection, IEnumerable<(double, double)>)

    /// <summary>
    /// Calculates the inverted color of the specified color.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The inverted color.</returns>
    internal static Color CalcInvertColor(Color color)
    {
        var r = color.R;
        var g = color.G;
        var b = color.B;
        var m = 255;
        return Color.FromArgb(color.A, m - r, m - g, m - b);
    } // internal static Color CalcInvertColor (Color)

    internal static Bitmap CaptureControl(this Control control)
    {
        var bmp = new Bitmap(control.Width, control.Height);
        control.DrawToBitmap(bmp, control.ClientRectangle);
        return bmp;
    } // internal static Bitmap CaptureControl (this Control)

    private static readonly int[] axisSplitCount = [1, 2, 4, 5, 10, 20, 40, 50, 100];
    private static readonly int[] axisIntervalSteps = [1, 2, 5, 10];

    /// <summary>
    /// Adjusts the interval of the specified axis.
    /// </summary>
    /// <param name="axis">The axis.</param>
    /// <param name="pixelWidthInterval">The width of the interval in pixels.</param>
    internal static void AdjustAxisInterval(this Axis axis, double pixelWidthInterval = 30)
    {
        if (axis.IsLogarithmic)
            axis.AdjustAxisIntervalLogarithmic(pixelWidthInterval);
        else
            axis.AdjustAxisIntervalLinear(pixelWidthInterval);
    } // internal static void AdjustAxisInterval (this Axis, [double])

    /// <summary>
    /// Adjusts the interval of the specified axis in linear scale.
    /// </summary>
    /// <param name="axis">The axis.</param>
    /// <param name="pixelWidthInterval">The width of the interval in pixels.</param>
    internal static void AdjustAxisIntervalLinear(this Axis axis, double pixelWidthInterval = 50)
    {
        var min = axis.Minimum;
        var max = axis.Maximum;

        var pixelWidth = Math.Abs(axis.ValueToPixelPosition(max) - axis.ValueToPixelPosition(min));
        var splitCount = (int)Math.Floor(pixelWidth / pixelWidthInterval);
        var index = Array.FindIndex(axisSplitCount, sc => sc >= splitCount);
        if (index < 0) index = axisSplitCount.Length - 1;
        splitCount = axisSplitCount[index];

        var interval = (max - min) / splitCount;
        var exponent = Math.Floor(Math.Log10(interval));
        var mantissa = interval / Math.Pow(10, exponent);

        index = Array.FindIndex(axisIntervalSteps, m => m >= mantissa);
        if (index < 0) index = axisIntervalSteps.Length - 1;
        axis.Interval = axisIntervalSteps[index] * Math.Pow(10, exponent);
        axis.MinorGrid.Interval = axis.Interval / 5;

        var offset = Math.Floor(min / axis.Interval) * axis.Interval - min;
        axis.IntervalOffset = offset;
        axis.MinorGrid.IntervalOffset = offset;
    } // internal static void AdjustAxisIntervalLinear (this Axis, [double])

    /// <summary>
    /// Adjusts the interval of the specified axis in logarithmic scale.
    /// </summary>
    /// <param name="axis">The axis.</param>
    /// <param name="pixelWidthInterval">The width of the interval in pixels.</param>
    internal static void AdjustAxisIntervalLogarithmic(this Axis axis, double pixelWidthInterval = 50)
    {
        var min = Math.Log10(axis.ScaleView.ViewMaximum);
        var max = Math.Log10(axis.ScaleView.ViewMaximum);

        var maxPx = axis.ValueToPixelPosition(axis.Maximum);
        var minPx = axis.ValueToPixelPosition(axis.Minimum);
        var pixelWidth = Math.Abs(maxPx - minPx);
        var splitCount = (int)Math.Floor(pixelWidth / pixelWidthInterval);
        var index = Array.FindIndex(axisSplitCount, sc => sc >= splitCount);
        if (index < 0) index = axisSplitCount.Length - 1;
        splitCount = axisSplitCount[index];

        var interval = Math.Ceiling((max / min) / splitCount);
        if (double.IsNaN(interval) || interval == 0) interval = 1;
        axis.Interval = interval;
        axis.MinorGrid.Interval = 1;

        var offset = Math.Floor(Math.Log10(axis.Minimum) / axis.Interval) * axis.Interval - Math.Log10(axis.Minimum);
        axis.IntervalOffset = offset;
        axis.MinorGrid.IntervalOffset = offset;
    } // internal static void AdjustAxisIntervalLogarithmic (this Axis, [double])
} // internal static partial class UIUtils
