
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Filter;

/// <summary>
/// Initializes a new instance of the <see cref="EquivalentSIMDAttribute"/> class.
/// </summary>
/// <param name="simdType">The equivalent SIMD type.</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class EquivalentSIMDAttribute(Type? simdType) : Attribute
{
    /// <summary>
    /// Gets the equivalent SIMD type.
    /// </summary>
    /// <value>The equivalent SIMD type, or <see langword="null"/> if not available.</value>
    public Type? SIMDType { get; } = simdType;

    /// <summary>
    /// Gets or sets the SIMD requirements.
    /// </summary>
    public SIMDRequirements SIMDRequirements { get; set; } = SIMDRequirements.None;
} // public sealed class EquivalentSIMDAttribute : Attribute
