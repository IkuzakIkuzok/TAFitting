
// (c) 2024-2026 Kazuki Kohzuki

using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;

namespace TAFitting.Controls;

/// <summary>
/// Provides utility methods for UI.
/// </summary>
internal static class UIUtils
{
    internal const double DecimalMin = -7.9e28;
    internal const double DecimalMax = +7.9e28;

    /// <summary>
    /// Formats the specified decimal value as a string in scientific (exponential) notation with two decimal places.
    /// </summary>
    /// <param name="value">The decimal value to format.</param>
    /// <returns>A string representation of the value in scientific notation with two decimal places.</returns>
    internal static string ExpFormatter(decimal value)
    {
        // +0.00x10⁻⁰⁰⁰ = 12 characters max, allocate with some margin for safety
        var s = (stackalloc char[16]);
        if (!value.TryFormat(s, out var len, "E2", CultureInfo.InvariantCulture))
            return value.ToInvariantString();

        return ExpFormatterInternal(s, len, []);
    } // internal static string ExpFormatter (decimal, string?)

    /// <summary>
    /// Formats the specified double-precision floating-point value as a string in scientific (exponential) notation with two decimal places.
    /// </summary>
    /// <param name="value">The double-precision floating-point value to format.</param>
    /// <returns>A string representation of the value in scientific notation with two decimal places.</returns>
    internal static string ExpFormatter(double value)
    {
        // +0.00x10⁻⁰⁰⁰ = 12 characters max, allocate with some margin for safety
        var s = (stackalloc char[16]);
        if (!value.TryFormat(s, out var len, "E2", CultureInfo.InvariantCulture))
            return value.ToInvariantString();

        return ExpFormatterInternal(s, len, []);
    } // internal static string ExpFormatter (double)

    /// <summary>
    /// Formats the specified double-precision floating-point value as a string in scientific (exponential) notation with two decimal places.
    /// </summary>
    /// <param name="value">The double-precision floating-point value to format.</param>
    /// <param name="unnecessaryPrefix">A prefix string to be removed from the formatted result, if present.</param>
    /// <returns>A string representation of the value in scientific notation with two decimal places.</returns>
    internal static string ExpFormatter(double value, string unnecessaryPrefix)
    {
        // +0.00x10⁻⁰⁰⁰ = 12 characters max, allocate with some margin for safety
        var s = (stackalloc char[16]);
        if (!value.TryFormat(s, out var len, "E2", CultureInfo.InvariantCulture))
            return value.ToInvariantString();

        return ExpFormatterInternal(s, len, unnecessaryPrefix);
    } // internal static string ExpFormatter (double, string)

    /// <summary>
    /// Formats a numeric string in scientific notation by replacing the exponent part with a Unicode superscript representation.
    /// </summary>
    /// <param name="s">A span of characters containing the numeric string to format. The span is modified in place to contain the formatted result.</param>
    /// <param name="len">The length of the valid data in the span <paramref name="s"/> to consider for formatting.</param>
    /// <param name="unnecessaryPrefix">A read-only span of characters representing a prefix to remove from the formatted result if present. If empty, no prefix is removed.</param>
    /// <returns>A string containing the formatted numeric value with the exponent expressed using Unicode superscript characters.</returns>
    private static string ExpFormatterInternal(Span<char> s, int len, ReadOnlySpan<char> unnecessaryPrefix)
    {
        var expIndex = s.IndexOf('E');
        var exponent = (stackalloc char[len - expIndex - 1]);
        s[(expIndex + 1)..(len)].CopyTo(exponent);  // copy exponent part to separate buffer, as `s` will be modified in-place

        var index = expIndex;

        s[index++] = '×';
        s[index++] = '1';
        s[index++] = '0';

        if (exponent[0] is '-' or '\u2212')
            s[index++] = '⁻';  // U+207B

        exponent = exponent[1..].TrimStart('0');
        if (exponent.Length == 0)
        {
            s[index++] = '⁰';  // U+2070
        }
        else
        {
            // append unicode superscript
            foreach (var c in exponent)
            {
                /*
                 * Superscript of 1, 2, 3 are located in U+00Bx,
                 * whereas the rest in U+207x.
                 */
                s[index++] = c switch
                {
                    '1' => '¹',  // U+00B9
                    '2' => '²',  // U+00B2
                    '3' => '³',  // U+00B3
                    _ => (char)(c + 0x2040)  // U+2070 - U+2079
                };
            }
        }

        s = s[..index];

        if (!unnecessaryPrefix.IsEmpty && s.StartsWith(unnecessaryPrefix, StringComparison.Ordinal))
            s = s[unnecessaryPrefix.Length..];

        return new(s);
    } // private static string ExpFormatterInternal (Span<char>, int, ReadOnlySpan<char>)

    /// <summary>
    /// Gets the range of the specified series.
    /// </summary>
    /// <param name="series">The series.</param>
    /// <returns>The minimum and maximum values of the series.</returns>
    internal static (double Min, double Max) GetRange(this Series series)
    {
        if (series.Points.Count == 0) return (0, 0);
        var min = double.MaxValue;
        var max = double.MinValue;
        foreach (var point in series.Points)
        {
            if (point.IsEmpty) continue;
            if (point.YValues.Length == 0) continue;
            var y = point.YValues[0];
            if (!double.IsFinite(y)) continue; // Skip non-finite values
            if (y < min) min = y;
            if (y > max) max = y;
        }
        return (min, max);
    } // internal static (double, double) GetRange (this Series)

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

    /// <summary>
    /// Sets exponential labels for the specified logarithmic axis.
    /// </summary>
    /// <param name="axis">The axis.</param>
    internal static void SetExpLabels(this Axis axis)
    {
        axis.CustomLabels.Clear();
        if (!axis.IsLogarithmic) return;

        var interval = axis.Interval;
        var logBase = Math.Pow(axis.LogarithmBase, interval);

        /*
         * Simple ceiling or flooring does not work due to floating-point precision issues:
         * e.g.,
         *  Math.Log(1e-9, 10) == -8.9999999999999982
         *  Math.Log(1e3, 10) == 2.9999999999999996
         * To address this, we round the logarithmic values first, then adjust them if necessary.
         */

        var logMin = axis.Minimum <= 0.0 ? 0.0 : Math.Round(Math.Log(axis.Minimum, logBase));
        if (Math.Pow(10, logMin) < axis.Minimum) logMin += interval;

        var logMax = axis.Maximum <= 0.0 ? 0.0 : Math.Round(Math.Log(axis.Maximum, logBase));
        if (Math.Pow(10, logMax) > axis.Maximum) logMax -= interval;

        var n = logMax - logMin + 1;

        for (var i = 0; i < n; ++i)
        {
            var offset = i * interval;
            var center = logMin + offset;
            var val = Math.Pow(logBase, logMin + i);
            var label = ExpFormatter(val, "1.00×");
            axis.CustomLabels.Add(center - 1, center + 1, label);
        }
    } // internal static void SetExpLabels (this Axis)
} // internal static class UIUtils
