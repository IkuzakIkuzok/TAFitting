
// (c) 2024 Kazuki KOHZUKI

using EnumSerializer;

namespace TAFitting.Data;

/// <summary>
/// Represents a SI prefix.
/// </summary>
[EnumSerializable(typeof(DefaultSerializeValueAttribute))]
internal enum SIPrefix
{
    /// <summary>
    /// No prefix.
    /// </summary>
    [DefaultSerializeValue("")]
    None = 0,

    /// <summary>
    /// Quetta (Q, 10^30)
    /// </summary>
    [DefaultSerializeValue("Q")]
    Quetta = 30,

    /// <summary>
    /// Ronna (R, 10^27)
    /// </summary>
    [DefaultSerializeValue("R")]
    Ronna = 27,

    /// <summary>
    /// Yotta (Y, 10^24)
    /// </summary>
    [DefaultSerializeValue("Y")]
    Yotta = 24,

    /// <summary>
    /// Zetta (Z, 10^21)
    /// </summary>
    [DefaultSerializeValue("Z")]
    Zetta = 21,

    /// <summary>
    /// Exa (E, 10^18)
    /// </summary>
    [DefaultSerializeValue("E")]
    Exa = 18,

    /// <summary>
    /// Peta (P, 10^15)
    /// </summary>
    [DefaultSerializeValue("P")]
    Peta = 15,

    /// <summary>
    /// Tera (T, 10^12)
    /// </summary>
    [DefaultSerializeValue("T")]
    Tera = 12,

    /// <summary>
    /// Giga (G, 10^9)
    /// </summary>
    [DefaultSerializeValue("G")]
    Giga = 9,

    /// <summary>
    /// Mega (M, 10^6)
    /// </summary>
    [DefaultSerializeValue("M")]
    Mega = 6,

    /// <summary>
    /// Kilo (k, 10^3)
    /// </summary>
    [DefaultSerializeValue("k")]
    Kilo = 3,

    /// <summary>
    /// Hecto (h, 10^2)
    /// </summary>
    [DefaultSerializeValue("h")]
    Hecto = 2,

    /// <summary>
    /// Deca (da, 10^1)
    /// </summary>
    [DefaultSerializeValue("da")]
    Deca = 1,

    /// <summary>
    /// Deci (d, 10^-1)
    /// </summary>
    [DefaultSerializeValue("d")]
    Deci = -1,

    /// <summary>
    /// Centi (c, 10^-2)
    /// </summary>
    [DefaultSerializeValue("c")]
    Centi = -2,

    /// <summary>
    /// Milli (m, 10^-3)
    /// </summary>
    [DefaultSerializeValue("m")]
    Milli = -3,

    /// <summary>
    /// Micro (μ, 10^-6)
    /// </summary>
    [DefaultSerializeValue("\u00b5")]
    Micro = -6,

    /// <summary>
    /// Nano (n, 10^-9)
    /// </summary>
    [DefaultSerializeValue("n")]
    Nano = -9,

    /// <summary>
    /// Pico (p, 10^-12)
    /// </summary>
    [DefaultSerializeValue("p")]
    Pico = -12,

    /// <summary>
    /// Femto (f, 10^-15)
    /// </summary>
    [DefaultSerializeValue("f")]
    Femto = -15,

    /// <summary>
    /// Atto (a, 10^-18)
    /// </summary>
    [DefaultSerializeValue("a")]
    Atto = -18,

    /// <summary>
    /// Zepto (z, 10^-21)
    /// </summary>
    [DefaultSerializeValue("z")]
    Zepto = -21,

    /// <summary>
    /// Yocto (y, 10^-24)
    /// </summary>
    [DefaultSerializeValue("y")]
    Yocto = -24,

    /// <summary>
    /// Ronto (r, 10^-27)
    /// </summary>
    [DefaultSerializeValue("r")]
    Ronto = -27,

    /// <summary>
    /// Quecto (q, 10^-30)
    /// </summary>
    [DefaultSerializeValue("q")]
    Quecto = -30,
} // internal enum SIPrefix
