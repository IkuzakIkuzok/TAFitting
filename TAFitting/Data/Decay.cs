﻿
// (c) 2024 Kazuki KOHZUKI

using System.Collections;
using System.Diagnostics;

namespace TAFitting.Data;

/// <summary>
/// Represents a decay data.
/// </summary>
[DebuggerDisplay("{TimeMin.ToString(\"F2\"),nq}\u2013{TimeMax.ToString(\"F2\"),nq}, {this.times.Length} points")]
internal sealed class Decay : IEnumerable<(double Time, double Signal)>
{
    private readonly double[] times, signals;

    /// <summary>
    /// An empty decay data.
    /// </summary>
    internal static readonly Decay Empty = new([], []);

    /// <summary>
    /// Gets the times.
    /// </summary>
    internal IReadOnlyList<double> Times => this.times;

    /// <summary>
    /// Gets the signals.
    /// </summary>
    internal IReadOnlyList<double> Signals => this.signals;

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
    internal Decay Absolute => new(this.times, this.signals.Select(Math.Abs).ToArray());

    /// <summary>
    /// Gets the inverted decay data.
    /// </summary>
    internal Decay Inverted => new(this.times, this.signals.Select(s => -s).ToArray());

    internal Decay OnlyAfterT0
    {
        get
        {
            var index_t0 = this.times.Select((t, i) => (t, i)).First(t => t.t >= 0).i;
            return new(this.times[index_t0..], this.signals[index_t0..]);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Decay"/> class.
    /// </summary>
    /// <param name="times">The times.</param>
    /// <param name="signals">The signals.</param>
    /// <exception cref="ArgumentException">times and signals must have the same length.</exception>
    internal Decay(double[] times, double[] signals)
    {
        if (times.Length != signals.Length)
            throw new ArgumentException("times and signals must have the same length.");

        this.times = times;
        this.signals = signals;
    } // ctor (double[], double[])

    /// <summary>
    /// Reads a decay data from a file.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <param name="timeScaling">The time scaling.</param>
    /// <param name="signalScaling">The signal scaling.</param>
    /// <returns>The decay data.</returns>
    /// <exception cref="IOException">Failed to read the file.</exception>
    internal static Decay FromFile(string filename, double timeScaling = 1.0, double signalScaling = 1.0)
    {
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
            return new(times, signals);
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to read the file:\n{filename}", ex);
        }
    } // internal static Decay FromFile (string, [int], [int])

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
        return new(this.times[startIndex..endIndex], this.signals[startIndex..endIndex]);
    } // internal Decay OfRange (double, double)

    /// <summary>
    /// Adds the time.
    /// </summary>
    /// <param name="time">The time</param>
    /// <returns>The decay data with the shifted time.</returns>
    internal Decay AddTime(double time)
        => new(this.times.Select(t => t + time).ToArray(), this.signals);

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
} // internal sealed class Decay : IEnumerable<(double Time, double Signal)>
