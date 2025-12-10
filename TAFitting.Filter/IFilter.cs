
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Filter;

/// <summary>
/// Represents a filter.
/// </summary>
public interface IFilter
{
    /// <summary>
    /// Gets the name of the model.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the model.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Filters the signal.
    /// </summary>
    /// <param name="time">The time.</param>
    /// <param name="signal">The signal.</param>
    /// <param name="output">The output buffer.</param>
    public void Filter(ReadOnlySpan<double> time, ReadOnlySpan<double> signal, Span<double> output);
} // public interface IFilter
