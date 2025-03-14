
// (c) 2024-2025 Kazuki KOHZUKI

// Use optimized code for Tekave outputs
// The optimization is valid only if the maximum time is less than 10 s.
#define Tekave

using System.Collections;
using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
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
    /// An empty decay data.
    /// </summary>
    internal static readonly Decay Empty = new([], TimeUnit.Second, [], SignalUnit.OD);

    /// <summary>
    /// Gets the time unit.
    /// </summary>
    internal TimeUnit TimeUnit { get; }

    /// <summary>
    /// Gets the signal unit.
    /// </summary>
    internal SignalUnit SignalUnit { get; }

    /// <summary>
    /// Gets a value indicating whether the data has been filtered.
    /// </summary>
    internal bool HasFiltered { get; private set; } = false;

    /// <summary>
    /// Gets the times.
    /// </summary>
    internal IReadOnlyList<double> Times => this.times;

    /// <summary>
    /// Gets the raw times.
    /// </summary>
    unsafe internal IReadOnlyList<double> RawTimes
    {
        get
        {
            if (this.TimeUnit == TimeUnit.Second) return this.times;

            var times = new double[this.times.Length];
            var i = 0;
            var tu = (double)this.TimeUnit;
            if (Avx.IsSupported)
            {
                var a = stackalloc double[] { tu, tu, tu, tu };
                var v = Avx.LoadVector256(a);
                fixed (double* s = this.times, d = times)
                {
                    do
                    {
                        Avx.Store(d + i, Avx.Multiply(Avx.LoadVector256(s + i), v));
                        i += Vector256<double>.Count;
                    } while (i <= this.times.Length - Vector256<double>.Count);
                }
            }
            for (; i < this.times.Length; i++)
                times[i] = this.times[i] * tu;

            return times;
        }
    }

    /// <summary>
    /// Gets the signals.
    /// </summary>
    internal IReadOnlyList<double> Signals => this.signals;

    /// <summary>
    /// Gets the raw signals.
    /// </summary>
    unsafe internal IReadOnlyList<double> RawSignals
    {
        get
        {
            if (this.SignalUnit == SignalUnit.OD) return this.signals;

            var signals = new double[this.signals.Length];
            var i = 0;
            var su = (double)this.SignalUnit;
            if (Avx.IsSupported)
            {
                var a = stackalloc double[] { su, su, su, su };
                var v = Avx.LoadVector256(a);
                fixed (double* s = this.signals, d = signals)
                {
                    do
                    {
                        Avx.Store(d + i, Avx.Multiply(Avx.LoadVector256(s + i), v));
                        i += Vector256<double>.Count;
                    } while (i <= this.signals.Length - Vector256<double>.Count);
                }
            }
            for (; i < this.signals.Length; i++)
                signals[i] = this.signals[i] * su;
            return signals;
        }
    }

    /// <summary>
    /// Gets the minimum time.
    /// </summary>
    internal double TimeMin => this.times.Min();

    /// <summary>
    /// Gets the maximum time.
    /// </summary>
    internal double TimeMax => this.times.Max();

    /// <summary>
    /// Gets the time step.
    /// </summary>
    internal double TimeStep => this.times[1] - this.times[0];

    /// <summary>
    /// Gets the minimum signal.
    /// </summary>
    internal double SignalMin => this.signals.Min();

    /// <summary>
    /// Gets the maximum signal.
    /// </summary>
    internal double SignalMax => this.signals.Max();

    /// <summary>
    /// Gets the absolute decay data.
    /// </summary>
    internal Decay Absolute
    {
       get
        {
            var abs = new double[this.signals.Length];
            for (var i = 0; i < this.signals.Length; i++)
                abs[i] = Math.Abs(this.signals[i]);
            return new(this.times, this.TimeUnit, abs, this.SignalUnit);
        }
    }


    /// <summary>
    /// Gets the inverted decay data.
    /// </summary>
    internal Decay Inverted
    {
        get
        {
            var inv = new double[this.signals.Length];
            for (var i = 0; i < this.signals.Length; i++)
                inv[i] = -this.signals[i];
            return new(this.times, this.TimeUnit, inv, this.SignalUnit);
        }
    }

    /// <summary>
    /// Gets the filtered decay data.
    /// </summary>
    internal Decay Filtered => this.HasFiltered ? new(this.times, this.TimeUnit, this.filtered, this.SignalUnit) : this;

    /// <summary>
    /// Gets the decay data after t=0.
    /// </summary>
    internal Decay OnlyAfterT0
    {
        get
        {
            if (this.times[0] >= 0) return this;
            var index_t0 = this.times.Select((t, i) => (t, i)).First(t => t.t >= 0).i;
            return new(this.times[index_t0..], this.TimeUnit, this.signals[index_t0..], this.SignalUnit);
        }
    }

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
    /// <exception cref="ArgumentException">times and signals must have the same length.</exception>
    internal Decay(double[] times, TimeUnit timeUnit, double[] signals, SignalUnit signalUnit)
    {
        if (times.Length != signals.Length)
            throw new ArgumentException("times and signals must have the same length.");

        this.times = times;
        this.signals = signals;

        this.TimeUnit = timeUnit;
        this.SignalUnit = signalUnit;

        this.filtered = new double[times.Length];
        RestoreOriginal(true);
    } // ctor (double[], double[])

#if Tekave

    private const double SCALING_FACTOR = 0.000_000_000_001;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long FastParseFixedPoint(ReadOnlySpan<byte> span)
    {
        var neg = span[0] == '-';
        var val = span[1] - (long)'0';

        Debug.Assert(span.Length == 15);

        span = span.Slice(3);
        for (var i = 0; i < span.Length; i++)
        {
            var c = span[i];
            val = val * 10 + (c - '0');
        }

        return neg ? -val : val;
    } // private static long FastParseFixedPoint (ReadOnlySpan<byte>)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double FastParse(ReadOnlySpan<byte> span)
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
        var timeScaling = 1.0 / timeUnit;
        var signalScaling = 1.0 / signalUnit;

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

            const int BUFF_LEN = 43;
            const int LINES = 3;

            timeScaling *= SCALING_FACTOR;

            var times = new double[2499];
            var signals = new double[2499];

            using var reader = new FileStream(
                filename, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: BUFF_LEN * 3 * 7 * 7,
                false
            );
            Span<byte> buffer = stackalloc byte[BUFF_LEN * LINES];

            // Read the first line outside the loop to get the time step.
            reader.Read(buffer);
            var t = buffer.Slice(5 + BUFF_LEN * 0, 15);

            // Calculating the time by some althmetic operations is significantly faster than parsing the string.
            // Time spep is constant and is stored as long integer to keep the precision (like a fixed-point number).
            var dt = FastParseFixedPoint(t);

            var s0 = buffer.Slice(26 + BUFF_LEN * 0, 15);
            var s1 = buffer.Slice(26 + BUFF_LEN * 1, 15);
            var s2 = buffer.Slice(26 + BUFF_LEN * 2, 15);

            times[0] = dt * timeScaling;
            times[1] = (dt << 1) * timeScaling;
            times[2] = (dt * 3) * timeScaling;
            signals[0] = FastParse(s0) * signalScaling;
            signals[1] = FastParse(s1) * signalScaling;
            signals[2] = FastParse(s2) * signalScaling;

            for (var i = LINES; i < times.Length; i += LINES)
            {
                var read = reader.Read(buffer);

                s0 = buffer.Slice(26 + BUFF_LEN * 0, 15);
                s1 = buffer.Slice(26 + BUFF_LEN * 1, 15);
                s2 = buffer.Slice(26 + BUFF_LEN * 2, 15);

                times[i + 0] = (dt * (i + 1)) * timeScaling;
                times[i + 1] = (dt * (i + 2)) * timeScaling;
                times[i + 2] = (dt * (i + 3)) * timeScaling;
                signals[i + 0] = FastParse(s0) * signalScaling;
                signals[i + 1] = FastParse(s1) * signalScaling;
                signals[i + 2] = FastParse(s2) * signalScaling;
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
            return new(times, timeUnit, signals, signalUnit);

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
    /// <returns>The decay data.</returns>
    internal static Decay FromFile(string filename, TimeUnit timeUnit, SignalUnit signalUnit, byte[]? preLoadData)
    {
        if (preLoadData is null) return FromFile(filename, timeUnit, signalUnit);

#if Tekave
        const int BUFF_LEN = 43;
        var span = preLoadData.AsSpan();

        var timeScaling = SCALING_FACTOR / timeUnit;
        var signalScaling = 1.0 / signalUnit;

        var times = new double[2499];
        var signals = new double[2499];

        var t = span.Slice(5 + BUFF_LEN * 0, 15);
        var dt = FastParseFixedPoint(t);

        for (var i = 0; i < times.Length; i++)
        {
            var s = span.Slice(26 + BUFF_LEN * i, 15);
            times[i] = (dt * (i + 1)) * timeScaling;
            signals[i] = FastParse(s) * signalScaling;
        }
#else
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
#endif

        return new(times, timeUnit, signals, signalUnit);
    } // internal static Decay FromFile (string, TimeUnit, SignalUnit, byte[])

    /// <inheritdoc/>
    public IEnumerator<(double Time, double Signal)> GetEnumerator()
    {
        for (var i = 0; i < this.times.Length; i++)
            yield return (Time: this.times[i], Signal: this.signals[i]);
    } // public IEnumerator<(double Time, double Signal)> GetEnumerator ()

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Gets the decay data of the specified range.
    /// </summary>
    /// <param name="start">The start time of the range.</param>
    /// <param name="end">The end time of the range.</param>
    /// <returns>The decay data of the specified range.</returns>
    internal Decay OfRange(double start, double end)
    {
        var startIndex = Array.BinarySearch(this.times, start);
        if (startIndex < 0) startIndex = ~startIndex;  // If not found, Array.BinarySearch returns the bitwise complement of the index of the next element.
        var endIndex = Array.BinarySearch(this.times, end);
        if (endIndex < 0) endIndex = ~endIndex;
        return new(this.times[startIndex..endIndex], this.TimeUnit, this.signals[startIndex..endIndex], this.SignalUnit);
    } // internal Decay OfRange (double, double)

    /// <summary>
    /// Adds the time.
    /// </summary>
    /// <param name="time">The time</param>
    unsafe internal void AddTime(double time)
    {
        var i = 0;
        if (Avx.IsSupported)
        {
            var t = stackalloc double[] { time, time, time, time };
            var v = Avx.LoadVector256(t);
            fixed (double* p = this.times)
            {
                do
                {
                    Avx.Store(p + i, Avx.Add(Avx.LoadVector256(p + i), v));
                    i += Vector256<double>.Count;
                } while (i <= this.times.Length - Vector256<double>.Count);
            }
        }
        for (; i < this.times.Length; i++)
            this.times[i] += time;
    } // unsafe internal Decay AddTime (double)

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
    /// <returns>The time origin.</returns>
    /// <remarks>
    /// The time origin is the time at which the signal is minimum,
    /// and it is in the first half of the decay data.
    /// </remarks>
    internal double FilndT0()
    {
        var len = this.times.Length >> 1;
        var min = this.signals.Take(len).Min();
        var index = Array.IndexOf(this.signals, min);
        return this.times[index];
    } // internal double FilndT0 ()

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
            var filtered = filter.Filter(this.RawTimes, this.filtered).ToArray();
            Array.Copy(filtered, this.filtered, this.times.Length);
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
    internal void Interpolate()
    {
        var n = this.times.Length;
        if (n <= 2)
        {
            // To keep the behavior consistent, the original data must be restored.
            RestoreOriginal(true);
            return;
        }

        var new_times = new double[n];
        var new_signals = new double[n];
        var dt = (this.TimeMax - this.TimeMin) / (n - 1);

        new_times[0] = this.TimeMin;
        new_times[n - 1] = this.TimeMax;

        new_signals[0] = this.signals[0];
        new_signals[n - 1] = this.signals[^1];

        for (var i = 1; i < n - 1; ++i)
        {
            var t = this.TimeMin + i * dt;
            new_times[i] = t;

            // linear interpolation
            var index = Array.BinarySearch(this.times, t);
            if (index < 0) index = ~index;

            if (index == 0)
            {
                new_signals[i] = this.signals[0];
                continue;
            }
            if (index == n)
            {
                new_signals[i] = this.signals[^1];
                continue;
            }

            var t1 = this.times[index - 1];
            var t2 = this.times[index];
            var s1 = this.signals[index - 1];
            var s2 = this.signals[index];
            new_signals[i] = s1 + (s2 - s1) * (t - t1) / (t2 - t1);
        }

        Array.Copy(new_times, this.times, n);
        Array.Copy(new_signals, this.signals, n);
        RestoreOriginal(true);
    } // internal void Interpolate ()
} // internal sealed partial class Decay : IEnumerable<(double Time, double Signal)>
