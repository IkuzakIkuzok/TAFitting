
// (c) 2025 Kazuki Kohzuki

using EnumSerializer;

namespace TAFitting;

/// <summary>
/// Provides a description for a value.
/// </summary>
internal class DescriptionAttribute : SerializeValueAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptionAttribute"/> class.
    /// </summary>
    /// <param name="description">The description.</param>
    internal DescriptionAttribute(string description) : base(description) { }
} // internal class DescriptionAttribute
