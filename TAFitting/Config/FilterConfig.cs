﻿
// (c) 2025 Kazuki Kohzuki

using TAFitting.Filter.SavitzkyGolay;

namespace TAFitting.Config;

/// <summary>
/// Represents the filter configuration.
/// </summary>
[Serializable]
public sealed class FilterConfig
{
    /// <summary>
    /// Gets or sets the default filter.
    /// </summary>
    public Guid DefaultFilter { get; set; } = typeof(SavitzkyGolayFilterCubic25).GUID;

    /// <summary>
    /// Gets or sets the auto-apply flag.
    /// </summary>
    public bool AutoApply { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to hide the original data.
    /// </summary>
    public bool HideOriginal { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterConfig"/> class.
    /// </summary>
    public FilterConfig() { }
} // public sealed class FilterConfig
