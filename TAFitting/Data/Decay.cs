
// (c) 2024-2025 Kazuki KOHZUKI

// Use optimized code for Tekave outputs
// The optimization is valid only if the maximum time is less than 10 s.
#define Tekave

// Accept partial preloaded data, which is rarely appears for actual data.
//#define AcceptPartialPreload

using System.Collections;
using System.Diagnostics;
using System.Runtime.Intrinsics;
using TAFitting.Buffers;
using TAFitting.Filter;

#if Tekave
// for MethodImplAttribute
using System.Runtime.CompilerServices;
#endif

namespace TAFitting.Data;

/// <summary>
/// Represents a decay data.
/// </summary>
[DebuggerDisplay("{TimeMin.ToString(\"F2\"),nq}\u2013{TimeMax.ToString(\"F2\"),nq} {TimeUnit,nq}, {this.times.Length} points")]
internal sealed partial class Decay : IEnumerable<(double Time, double Signal)>
{
    // Separately store the original data and the filtered data,
    // so that the original data can be restored.
    private readonly double[] times, signals, filtered;

    /// <summary>
    /// Gets an empty decay data.
    /// </summary>
    internal static Decay Empty
        => field ??= new([], TimeUnit.Second, [], SignalUnit.OD, TasMode.None);

    /// <summary>
    /// Gets the time unit.
    /// </summary>
    internal TimeUnit TimeUnit { get; }

    /// <summary>
    /// Gets the signal unit.
    /// </summary>
    internal SignalUnit SignalUnit { get; }

    internal TasMode Mode { get; }

    /// <summary>
    /// Gets a value indicating whether the data has been filtered.
    /// </summary>
    internal bool HasFiltered { get; private set; } = false;

    /// <summary>
    /// Gets the times.
    /// </summary>
    internal IReadOnlyList<double> Times => this.times;

    /// <summary>
    /// Gets the signals.
    /// </summary>
    internal IReadOnlyList<double> Signals => this.signals;

    /// <summary>
    /// Gets the raw times.
    /// </summary>
    internal ReadOnlySpan<double> RawTimes
    {
        get
        {
            if (this.TimeUnit == TimeUnit.Second) return this.times;

            var times = new double[this.times.Length];
            RestoreRawValues(this.times, times, this.TimeUnit);
            return times;
        }
    }

    /// <summary>
    /// Gets the raw signals.
    /// </summary>
    internal ReadOnlySpan<double> RawSignals
    {
        get
        {
            if (this.SignalUnit == SignalUnit.OD) return this.signals;

            var signals = new double[this.signals.Length];
            RestoreRawValues(this.signals, signals, this.SignalUnit);
            return signals;
        }
    }

    /// <summary>
    /// Multiplies each element in the specified input array by the given scale factor and writes the results to the output array.
    /// </summary>
    /// <remarks>If hardware acceleration is available, vectorized operations are used for improved performance.
    /// The method does not allocate new arrays; results are written directly to the provided output array.</remarks>
    /// <param name="values">The input span containing the values to be scaled.</param>
    /// <param name="output">The span where the scaled output values will be written.</param>
    /// <param name="unit">The unit containing the scale factor.</param>
    private static void RestoreRawValues(ReadOnlySpan<double> values, Span<double> output, ValueUnit unit)
    {
        if (unit.Scale == SIPrefix.None)
        {
            values.CopyTo(output);
            return;
        }

        var scale = (double)unit;
        var i = 0;
        if (Vector256.IsHardwareAccelerated && Vector256<double>.IsSupported)
        {
            var v_su = Vector256.Create(scale);

            ref var begin = ref MemoryMarshal.GetReference(values);
            ref var to = ref Unsafe.Add(ref begin, values.Length - Vector256<double>.Count);
            ref var current = ref begin;
            ref var current_raw = ref MemoryMarshal.GetReference(output);
            while (Unsafe.IsAddressLessThan(ref current, ref to))
            {
                var v_current = Vector256.LoadUnsafe(ref current);
                Vector256.Multiply(v_current, v_su).StoreUnsafe(ref current_raw);
                current = ref Unsafe.Add(ref current, Vector256<double>.Count);
                current_raw = ref Unsafe.Add(ref current_raw, Vector256<double>.Count);
                i += Vector256<double>.Count;
            }
        }

        for (; i < values.Length; i++)
            output[i] = values[i] * scale;
    } // private static void RestoreRawValues (ReadOnlySpan<double>, Span<double>, double)

    /// <summary>
    /// Gets the minimum time.
    /// </summary>
    internal double TimeMin => this.times.AsSpan().Min();

    /// <summary>
    /// Gets the maximum time.
    /// </summary>
    internal double TimeMax => this.times.AsSpan().Max();

    /// <summary>
    /// Gets the time step.
    /// </summary>
    internal double TimeStep => this.times[1] - this.times[0];

    /// <summary>
    /// Gets the minimum signal.
    /// </summary>
    internal double SignalMin => this.signals.AsSpan().Min();

    /// <summary>
    /// Gets the maximum signal.
    /// </summary>
    internal double SignalMax => this.signals.AsSpan().Max();

    /// <summary>
    /// Gets the finite absolute maximum signal.
    /// </summary>
    internal double SignalFiniteAbsMax
    {
        get
        {
            var max = double.MinValue;
            foreach (var signal in this.signals)
            {
                if (!double.IsFinite(signal)) continue;
                var abs = Math.Abs(signal);
                if (abs > max) max = abs;
            }
            return max;
        }
    } // internal double SignalFiniteAbsMax

    /// <summary>
    /// Gets the filtered decay data.
    /// </summary>
    internal Decay Filtered => this.HasFiltered ? new(this.times, this.TimeUnit, this.filtered, this.SignalUnit, this.Mode) : this;

    /// <summary>
    /// Gets the filtered signals.
    /// </summary>
    internal ReadOnlySpan<double> FilteredSignals
        => this.HasFiltered ? this.filtered.AsSpan() : this.signals.AsSpan();

    /// <summary>
    /// Gets the times after t=0.
    /// </summary>
    internal ReadOnlySpan<double> TimesAfterT0
    {
        get
        {
            var index_t0 = GetIndexT0();
            return this.times.AsSpan(index_t0);
        }
    }

    /// <summary>
    /// Gets the signals after t=0.
    /// </summary>
    internal ReadOnlySpan<double> SignalsAfterT0
    {
        get
        {
            var index_t0 = GetIndexT0();
            return this.signals.AsSpan(index_t0);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIndexT0()
    {
        if (this.times[0] >= 0) return 0;

        var i0 = 0;
        var i1 = this.times.Length - 1;
        while (i1 - i0 > 1)
        {
            var im = (i0 + i1) >> 1;
            if (this.times[im] < 0)
                i0 = im;
            else
                i1 = im;
        }
        return i1;
    } // private int GetIndexT0 ()

    /// <summary>
    /// Gets the signal at the specified time.
    /// </summary>
    /// <param name="time">The time.</param>
    /// <returns>The signal at the specified time.</returns>
    internal double this[double time]
    {
        get
        {
            var index = Array.BinarySearch(this.times, time);
            if (index < 0) index = ~index;
            if (index == 0) return this.signals[0];
            if (index == this.times.Length) return this.signals[^1];
            return this.signals[index];
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Decay"/> class.
    /// </summary>
    /// <param name="times">The times.</param>
    /// <param name="timeUnit">The time unit.</param>
    /// <param name="signals">The signals.</param>
    /// <param name="signalUnit">The signal unit.</param>
    /// <param name="mode">The TAS mode.</param>
    /// <exception cref="ArgumentException">times and signals must have the same length.</exception>
    internal Decay(double[] times, TimeUnit timeUnit, double[] signals, SignalUnit signalUnit, TasMode mode)
    {
        if (times.Length != signals.Length)
            throw new ArgumentException("times and signals must have the same length.");

        this.times = times;
        this.signals = signals;

        this.TimeUnit = timeUnit;
        this.SignalUnit = signalUnit;

        this.Mode = mode;

        this.filtered = new double[times.Length];
        RestoreOriginal(true);
    } // ctor (double[], TimeUnit, double[], SignalUnit, TasMode)

#if Tekave

    private const int FILE_SIZE = 107_457;
    private const double SCALING_FACTOR = 0.000_000_000_001;

    private const int PARSING_LENGTH = 17; // The length of the string to be parsed

    /// <summary>
    /// Parses a fixed-point number in the format of "-123.123456789012" or " 123.123456789012".
    /// </summary>
    /// <param name="span">The span of bytes to be parsed.</param>
    /// <returns>The parsed value as a long integer.</returns>
    /// <remarks>
    /// The input string must be exactly 17 bytes long, with the format of "-123.123456789012" or " 123.123456789012".
    /// Leading whitespace is allowed, but leading zeros are not.
    /// Trailing whitespace is not allowed; the string must end with a digit.
    /// The integer part must be at most 3 digits (including the sign, if negative).
    /// Floating point sign ('.') must be at the 5th position (index 4).
    /// Returns the value multiplied by 10^12 as a long integer.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    unsafe private static long FastParseFixedPoint(Span<byte> span)
    {
        /*
         * 0-3: integer part (including sign, if negative)
         * 4: floating point sign ('.')
         * 5-16: 12 digits of the fractional part
         */

        Debug.Assert(span.Length == PARSING_LENGTH);
        Debug.Assert(span[4] == '.');

        var p = (byte*)Unsafe.AsPointer(ref span.GetPinnableReference());

        var val = span[3] - (long)'0';
        bool neg;

        // Whiltespace (0x20) or negative sign (0x2D) is less than '0' (0x30).
        if (p[2] < '0')
        {
            // 1 digit integer part
            neg = p[2] == '-';
            goto END_INTEGER_PART;
        }

        // 2 or 3 digit integer part
        val += (p[2] - '0') * 10;
        if (p[1] < '0')
        {
            // 2 digit integer part
            neg = p[1] == '-';
            goto END_INTEGER_PART;
        }

        // 3 digit integer part
        val += (p[1] - '0') * 100;
        neg = p[0] == '-';

    END_INTEGER_PART:
        val *= 1_000_000_000_000; // Shift the integer part to the left by 12 digits

        /*
         * Parse the factorial part of the floating point number by splitting the 12 digits into 8 + 4 digits.
         * See
         *  https://kholdstare.github.io/technical/2020/05/26/faster-integer-parsing.html#the-divide-and-conquer-insight
         * for the details of the algorithm.
         */

        // Parse the first 8 bytes after the floating point sign
        var f64 = *(ulong*)(p + 5);

        var l64 = (f64 & 0x0f_00_0f_00_0f_00_0f_00) >> 8;
        var u64 = (f64 & 0x00_0f_00_0f_00_0f_00_0f) * 10;
        f64 = l64 + u64;

        l64 = (f64 & 0x00ff_0000_00ff_0000) >> 16;
        u64 = (f64 & 0x0000_00ff_0000_00ff) * 100;
        f64 = l64 + u64;

        l64 = (f64 & 0x0000ffff_00000000) >> 32;
        u64 = (f64 & 0x00000000_0000ffff) * 10000;
        f64 = l64 + u64;
        val += (long)f64 * 10_000;

        // Parse the next 4 bytes
        var f32 = *(uint*)(p + 13);

        var l32 = (f32 & 0x0f_00_0f_00) >> 8;
        var u32 = (f32 & 0x00_0f_00_0f) * 10;
        f32 = l32 + u32;

        l32 = (f32 & 0x00ff_0000) >> 16;
        u32 = (f32 & 0x0000_00ff) * 100;
        f32 = l32 + u32;
        val += (int)f32;

        return neg ? -val : val;
    } // unsafe private static long FastParseFixedPoint (Span<byte>)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double FastParse(Span<byte> span)
        => FastParseFixedPoint(span) * SCALING_FACTOR;

#endif

    /// <summary>
    /// Reads a decay data from a file.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <param name="timeUnit">The time unit.</param>
    /// <param name="signalUnit">The signal unit.</param>
    /// <returns>The decay data.</returns>
    /// <exception cref="IOException">Failed to read the file.</exception>
    internal static Decay FromFile(string filename, TimeUnit timeUnit, SignalUnit signalUnit)
    {
        var timeScaling = SCALING_FACTOR / timeUnit;
        var signalScaling = SCALING_FACTOR / signalUnit;

        try
        {
#if Tekave
            /*
             * Optimized parsing for Tekave outputs.
             * 
             * Each CSV file contains 2499 lines
             * and each line has 43 bytes including trailing 0x0D and 0x0A.
             * 
             * 2499 = 3 * 7 * 7 * 17
             * Read 3 lines at a time.
             */

            const int LINE_LEN = FileCache.LINE_LENGTH;
            const int LINES = 3;
            const int SIGNAL_POS = 24; // The position of the signal in a line.

            var times = new double[2499];
            var signals = new double[2499];

            using var reader = new FileStream(
                filename, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: LINE_LEN * 3 * 7 * 7,
                false
            );
            var buffer = (stackalloc byte[LINE_LEN * LINES]);

            // Read the first line outside the loop to get the time step.
            reader.ReadExactly(buffer);
            var t = buffer.Slice(3 + LINE_LEN * 0, PARSING_LENGTH);

            // Calculating the time by some althmetic operations is significantly faster than parsing the string.
            // Time spep is constant and is stored as long integer to keep the precision (like a fixed-point number).
            var dt = FastParseFixedPoint(t);

            var s0 = buffer.Slice(LINE_LEN * 0 + SIGNAL_POS, PARSING_LENGTH);
            var s1 = buffer.Slice(LINE_LEN * 1 + SIGNAL_POS, PARSING_LENGTH);
            var s2 = buffer.Slice(LINE_LEN * 2 + SIGNAL_POS, PARSING_LENGTH);

            times[0] = dt * timeScaling;
            times[1] = (dt << 1) * timeScaling;
            times[2] = (dt * 3) * timeScaling;
            signals[0] = FastParseFixedPoint(s0) * signalScaling;
            signals[1] = FastParseFixedPoint(s1) * signalScaling;
            signals[2] = FastParseFixedPoint(s2) * signalScaling;

            for (var i = LINES; i < times.Length; i += LINES)
            {
                var read = reader.Read(buffer);

                s0 = buffer.Slice(LINE_LEN * 0 + SIGNAL_POS, PARSING_LENGTH);
                s1 = buffer.Slice(LINE_LEN * 1 + SIGNAL_POS, PARSING_LENGTH);
                s2 = buffer.Slice(LINE_LEN * 2 + SIGNAL_POS, PARSING_LENGTH);

                times[i + 0] = (dt * (i + 1)) * timeScaling;
                times[i + 1] = (dt * (i + 2)) * timeScaling;
                times[i + 2] = (dt * (i + 3)) * timeScaling;
                signals[i + 0] = FastParseFixedPoint(s0) * signalScaling;
                signals[i + 1] = FastParseFixedPoint(s1) * signalScaling;
                signals[i + 2] = FastParseFixedPoint(s2) * signalScaling;
            }
#else
            var lines = File.ReadAllLines(filename);
            var times = new double[lines.Length];
            var signals = new double[lines.Length];
            for (var i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                times[i] = double.Parse(parts[0]) * timeScaling;
                signals[i] = double.Parse(parts[1]) * signalScaling;
            }
#endif
            return new(times, timeUnit, signals, signalUnit, TasMode.Microsecond);

        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to read the file:\n{filename}", ex);
        }
    } // internal static Decay FromFile (string, TimeUnit, SignalUnit)

    /// <summary>
    /// Reads a decay data from a file and preloaded data.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <param name="timeUnit">The time unit.</param>
    /// <param name="signalUnit">The signal unit.</param>
    /// <param name="preLoadData">The preloaded data.</param>
    /// <param name="lines">The number of lines to read.</param>
    /// <returns>The decay data.</returns>
    internal static Decay FromFile(string filename, TimeUnit timeUnit, SignalUnit signalUnit, FileCache? preLoadData, int lines = 2499)
    {
        if (preLoadData is null) return FromFile(filename, timeUnit, signalUnit);

#if AcceptPartialPreload
        if (preLoadData.Length < (lines >> 1)) return FromFile(filename, timeUnit, signalUnit);
#else
        // If the data is not fully loaded, return the data from the file.
        if (preLoadData.Length < lines * 43) return FromFile(filename, timeUnit, signalUnit);
#endif

#if Tekave
        const int LINE_LEN = FileCache.LINE_LENGTH;
        const int SIGNAL_POS = 24; // The position of the signal in a line.

        var timeScaling = SCALING_FACTOR / timeUnit;
        var signalScaling = SCALING_FACTOR / signalUnit;

        var times = new double[lines];
        var signals = new double[lines];

        var line = preLoadData.ReadLine(0);
        var t = line.Slice(3, PARSING_LENGTH);
        var dt = FastParseFixedPoint(t);

        times[0] = dt * timeScaling;
        signals[0] = FastParseFixedPoint(line.Slice(SIGNAL_POS, PARSING_LENGTH)) * signalScaling;

        var l = Math.Min(preLoadData.Length / LINE_LEN, lines);
        var i = 1;
        for (; i < l; ++i)
        {
            line = preLoadData.ReadLine(i);
            var s = line.Slice(SIGNAL_POS, PARSING_LENGTH);
            times[i] = (dt * (i + 1)) * timeScaling;
            signals[i] = FastParseFixedPoint(s) * signalScaling;
        }

#if AcceptPartialPreload
        if (l < lines)
        {
            const int LINES = 3;

            using var reader = new FileStream(
                filename, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: LINE_LEN * 3 * 7 * 7,
                false
            );
            var buffer = (stackalloc byte[LINE_LEN * LINES]);

            reader.Seek(span.Length, SeekOrigin.Begin);
            for (; i < times.Length; i += LINES)
            {
                var read = reader.Read(buffer);

                var s0 = buffer.Slice(LINE_LEN * 0 + SIGNAL_POS, PARSING_LENGTH);
                var s1 = buffer.Slice(LINE_LEN * 1 + SIGNAL_POS, PARSING_LENGTH);
                var s2 = buffer.Slice(LINE_LEN * 2 + SIGNAL_POS, PARSING_LENGTH);

                times[i + 0] = (dt * (i + 1)) * timeScaling;
                times[i + 1] = (dt * (i + 2)) * timeScaling;
                times[i + 2] = (dt * (i + 3)) * timeScaling;
                signals[i + 0] = FastParseFixedPoint(s0) * signalScaling;
                signals[i + 1] = FastParseFixedPoint(s1) * signalScaling;
                signals[i + 2] = FastParseFixedPoint(s2) * signalScaling;
            }
        }
#endif  // AcceptPartialPreload
#else  // Tekave
        var timeScaling = 1.0 / timeUnit;
        var signalScaling = 1.0 / signalUnit;

        var text = preLoadData.GetText();
        var lines = text.Split('\n');

        var times = new double[lines.Length];
        var signals = new double[lines.Length];
        for (var i = 0; i < lines.Length; i++)
        {
            var parts = lines[i].Split(',');
            times[i] = double.Parse(parts[0]) * timeScaling;
            signals[i] = double.Parse(parts[1]) * signalScaling;
        }
#endif  // Tekave

        return new(times, timeUnit, signals, signalUnit, TasMode.Microsecond);
    } // internal static Decay FromFile (string, TimeUnit, SignalUnit, FileCache?, [int])

    /// <inheritdoc/>
    public IEnumerator<(double Time, double Signal)> GetEnumerator()
    {
        for (var i = 0; i < this.times.Length; i++)
            yield return (Time: this.times[i], Signal: this.signals[i]);
    } // public IEnumerator<(double Time, double Signal)> GetEnumerator ()

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Gets the range of indices for the specified time range.
    /// </summary>
    /// <param name="start">The start time of the range.</param>
    /// <param name="end">The end time of the range.</param>
    /// <returns>The range of indices for the specified time range.</returns>
    internal Range GetRange(double start, double end)
    {
        var startIndex = Array.BinarySearch(this.times, start);
        if (startIndex < 0) startIndex = ~startIndex;  // If not found, Array.BinarySearch returns the bitwise complement of the index of the next element.
        var endIndex = Array.BinarySearch(this.times, end);
        if (endIndex < 0) endIndex = ~endIndex;
        return startIndex..endIndex;
    } // internal Range GetRange (double, double)

    /// <summary>
    /// Gets the range of indices after t=0.
    /// </summary>
    /// <returns>The range of indices after t=0.</returns>
    internal Range GetRangeAfterT0()
    {
       var index_t0 = GetIndexT0();
        return index_t0..this.times.Length;
    } // internal Range GetRangeAfterT0 ()

    /// <summary>
    /// Gets the times as a span.
    /// </summary>
    /// <returns>The times as a span.</returns>
    internal ReadOnlySpan<double> GetTimesAsSpan()
        => this.times.AsSpan();

    /// <summary>
    /// Gets the signals as a span.
    /// </summary>
    /// <returns>The signals as a span.</returns>
    internal ReadOnlySpan<double> GetSignalsAsSpan()
        => this.signals.AsSpan();

    /// <summary>
    /// Gets the signals as a span for the specified length.
    /// </summary>
    /// <param name="length">The length of the span.</param>
    /// <returns>The signals as a span for the specified length.</returns>
    internal ReadOnlySpan<double> GetSignalsAsSpan(int length)
        => this.signals.AsSpan(0, length);

    /// <summary>
    /// Adds the time.
    /// </summary>
    /// <param name="time">The time</param>
    internal void AddTime(double time)
    {
        if (AvxVector.IsSupported && this.times.Length > Vector256<double>.Count && false)
        {
            // Vectorized addition
            var v = new AvxVector(this.times);
#pragma warning disable IDE0059  // Unnecessary assignment of a value
            v += time;
#pragma warning restore
        }
        else
        {
            for (var i = 0; i < this.times.Length; i++)
                this.times[i] += time;
        }
    } // internal Decay AddTime (double)

    /// <summary>
    /// Gets the time at which the signal is minimum.
    /// </summary>
    /// <returns>The time at which the signal is minimum.</returns>
    internal double GetMinTime()
    {
        var min = this.SignalMin;
        var index = Array.IndexOf(this.signals, min);
        return this.times[index];
    } // internal double GetMinTime ()

    /// <summary>
    /// Finds the time origin.
    /// </summary>
    /// <returns>The index, time, and signal at the time origin.</returns>
    /// <remarks>
    /// The time origin is the time at which the signal is minimum.
    /// </remarks>
    internal (int, double, double) FilndT0()
    {
        var span = this.signals.AsSpan();
        var min = span.Min();
        var index = span.IndexOf(min);
        return (index, this.times[index], min);
    } // internal (int, double, double) FilndT0 ()

    /// <summary>
    /// Removes the NaN.
    /// </summary>
    internal void RemoveNaN()
    {
        for (var i = 0; i < this.signals.Length; i++)
        {
            if (double.IsFinite(this.signals[i])) continue;
            var left = i > 0 ? this.signals[i - 1] : 0.0;
            var right = i < this.signals.Length - 1 ? this.signals[i + 1] : 0.0;
            if (double.IsNaN(left) || double.IsNaN(right))
                this.signals[i] = 0.0;
            else
                this.signals[i] = (left + right) / 2.0;
        }
    } // internal void RemoveNaN ()

    /// <summary>
    /// Applies the filter.
    /// </summary>
    /// <param name="filter">The filter.</param>
    internal void Filter(IFilter filter)
    {
        try
        {
            using var pooled = new PooledBuffer<double>(this.times.Length);
            var buffer = this.times.Length <= 0x1000 ? stackalloc double[this.times.Length] : pooled.GetSpan();
            RestoreRawValues(this.times, buffer, this.TimeUnit);

            filter.Filter(buffer, this.signals, this.filtered);
            this.HasFiltered = true;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    } // internal void Filter (IFilter)

    /// <summary>
    /// Restores the original data.
    /// </summary>
    internal void RestoreOriginal(bool forceRestore = false)
    {
        if (!this.HasFiltered && !forceRestore) return;
        Array.Copy(this.signals, this.filtered, this.signals.Length);
        this.HasFiltered = false;
    } // internal void RestoreOriginal ([bool])

    /// <summary>
    /// Interpolates the decay data so that the time points are evenly spaced.
    /// </summary>
    /// <param name="mode">The interpolation mode.</param>
    internal void Interpolate(InterpolationMode mode = InterpolationMode.Linear)
    {
        var n = this.times.Length;
        if (n <= 2)
        {
            // To keep the behavior consistent, the original data must be restored.
            RestoreOriginal(true);
            return;
        }

        using var pooled = new PooledGroupedBuffer<double>(n, 2);
        var new_times = n <= 0x1000 ? stackalloc double[n] : pooled.GetSpan(0);
        var new_signals = n <= 0x1000 ? stackalloc double[n] : pooled.GetSpan(1);

        Interpolation.Interpolate(mode, this.times, this.signals, new_times, new_signals);

        new_times.CopyTo(this.times);
        new_signals.CopyTo(this.signals);
        RestoreOriginal(true);
    } // internal void Interpolate ()
} // internal sealed partial class Decay : IEnumerable<(double Time, double Signal)>
