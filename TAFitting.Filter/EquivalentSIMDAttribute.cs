
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Filter;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class EquivalentSIMDAttribute : Attribute
{
    /// <summary>
    /// Gets the equivalent SIMD type.
    /// </summary>
    /// <value>The equivalent SIMD type, or <see langword="null"/> if not available.</value>
    public Type? SIMDType { get; }

    /// <summary>
    /// Gets or sets the SIMD requirements.
    /// </summary>
    public SIMDRequirements SIMDRequirements { get; set; } = SIMDRequirements.None;

    /// <summary>
    /// Initializes a new instance of the <see cref="EquivalentSIMDAttribute"/> class.
    /// </summary>
    /// <param name="simdType">The equivalent SIMD type.</param>
    public EquivalentSIMDAttribute(Type? simdType)
    {
        this.SIMDType = simdType;
    } // ctor (Type?)
} // public sealed class EquivalentSIMDAttribute : Attribute
