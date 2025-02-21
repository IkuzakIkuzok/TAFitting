
// (c) 2024 Kazuki KOHZUKI

using System.Collections;
using System.Diagnostics;
using TAFitting.Filter;

namespace TAFitting.Data;

/// <summary>
/// Represents a decay data.
/// </summary>
[DebuggerDisplay("{TimeMin.ToString(\"F2\"),nq}\u2013{TimeMax.ToString(\"F2\"),nq}, {this.times.Length} points")]
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
    internal IReadOnlyList<double> RawTimes
    {
        get
        {
            if (this.TimeUnit == TimeUnit.Second) return this.times;
            return [.. this.times.Select(t => t * this.TimeUnit)];
        }
    }

    /// <summary>
    /// Gets the signals.
    /// </summary>
    internal IReadOnlyList<double> Signals => this.signals;

    /// <summary>
    /// Gets the raw signals.
    /// </summary>
    internal IReadOnlyList<double> RawSignals
    {
        get
        {
            if (this.SignalUnit == SignalUnit.OD) return this.signals;
            return [.. this.signals.Select(s => s * this.SignalUnit)];
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
    internal Decay Absolute => new(this.times, this.TimeUnit, [.. this.signals.Select(Math.Abs)], this.SignalUnit);

    /// <summary>
    /// Gets the inverted decay data.
    /// </summary>
    internal Decay Inverted => new(this.times, this.TimeUnit, [.. this.signals.Select(s => -s)], this.SignalUnit);

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
            var lines = File.ReadAllLines(filename);
            var times = new double[lines.Length];
            var signals = new double[lines.Length];
            for (var i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                times[i] = double.Parse(parts[0]) * timeScaling;
                signals[i] = double.Parse(parts[1]) * signalScaling;
            }
            return new(times, timeUnit, signals, signalUnit);
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to read the file:\n{filename}", ex);
        }
    } // internal static Decay FromFile (string, TimeUnit, SignalUnit)

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
    /// <returns>The decay data with the shifted time.</returns>
    internal Decay AddTime(double time)
        => new([.. this.times.Select(t => t + time)], this.TimeUnit, this.signals, this.SignalUnit);

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
        if (n <= 2) return;

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
