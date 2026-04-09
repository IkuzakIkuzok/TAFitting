
// (c) 2025 Kazuki Kohzuki

using EnumSerializer;

namespace TAFitting.Data;

/// <summary>
/// Defines interpolation modes.
/// </summary>
[EnumSerializable(typeof(DefaultSerializeValueAttribute), Methods = ExtensionMethods.ToString)]
[EnumSerializable(typeof(DescriptionAttribute), Methods = ExtensionMethods.ToString)]
internal enum InterpolationMode
{
    /// <summary>
    /// Linear interpolation.
    /// </summary>
    [DefaultSerializeValue("Linear interpolation")]
    [Description("Interpolate linearly between data points.")]
    Linear,

    /// <summary>
    /// Spline interpolation.
    /// </summary>
    [DefaultSerializeValue("Spline interpolation")]
    [Description("Interpolate using spline curves between data points.")]
    Spline,
} // internal enum InterpolationMode
