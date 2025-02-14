
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Filter.Fourier;

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 1 kHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("28E11BB0-83D5-424B-AB72-B3B257805471")]
internal sealed class FourierFilter1e3 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter1e3"/> class.
    /// </summary>
    public FourierFilter1e3()
    {
        this.cutoff = 1_000;
    } // ctor ()
} // internal sealed class FourierFilter1e3 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 2 kHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("71C2A631-72E1-4ED3-99C7-57A899BC80F8")]
internal sealed class FourierFilter2e3 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter2e3"/> class.
    /// </summary>
    public FourierFilter2e3()
    {
        this.cutoff = 2_000;
    } // ctor ()
} // internal sealed class FourierFilter2e3 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 5 kHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("48E3161E-96A2-443E-A5A1-FB30AAC570B6")]
internal sealed class FourierFilter5e3 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter5e3"/> class.
    /// </summary>
    public FourierFilter5e3()
    {
        this.cutoff = 5_000;
    } // ctor ()
} // internal sealed class FourierFilter5e3 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 10 kHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("62AC7236-4DD7-4000-8F1C-F3B11A1E3BB9")]
internal sealed class FourierFilter1e4 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter1e4"/> class.
    /// </summary>
    public FourierFilter1e4()
    {
        this.cutoff = 10_000;
    } // ctor ()
} // internal sealed class FourierFilter1e4 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 20 kHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("E52CA919-FFED-4E69-93C9-E3DBABCF58CF")]
internal sealed class FourierFilter2e4 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter2e4"/> class.
    /// </summary>
    public FourierFilter2e4()
    {
        this.cutoff = 20_000;
    } // ctor ()
} // internal sealed class FourierFilter2e4 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 50 kHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("4BB9850F-0A1D-4565-B042-0B94FBF14306")]
internal sealed class FourierFilter5e4 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter5e4"/> class.
    /// </summary>
    public FourierFilter5e4()
    {
        this.cutoff = 50_000;
    } // ctor ()
} // internal sealed class FourierFilter5e4 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 100 kHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("72637FEC-9C61-4D7A-941D-619040326F81")]
internal sealed class FourierFilter1e5 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter1e5"/> class.
    /// </summary>
    public FourierFilter1e5()
    {
        this.cutoff = 100_000;
    } // ctor ()
} // internal sealed class FourierFilter1e5 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 200 kHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("223D03A2-12A6-43ED-9D63-2DA1A8B58AC4")]
internal sealed class FourierFilter2e5 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter2e5"/> class.
    /// </summary>
    public FourierFilter2e5()
    {
        this.cutoff = 200_000;
    } // ctor ()
} // internal sealed class FourierFilter2e5 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 500 kHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("2036612F-A5C6-40E8-953D-FD19E541FCF5")]
internal sealed class FourierFilter5e5 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter5e5"/> class.
    /// </summary>
    public FourierFilter5e5()
    {
        this.cutoff = 500_000;
    } // ctor ()
} // internal sealed class FourierFilter5e5 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 1 MHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("DB433693-0E08-4B72-B94D-36CCC16589FE")]
internal sealed class FourierFilter1e6 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter1e6"/> class.
    /// </summary>
    public FourierFilter1e6()
    {
        this.cutoff = 1_000_000;
    } // ctor ()
} // internal sealed class FourierFilter1e6 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 2 MHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("FE04780B-F215-49E5-832F-23A734111052")]
internal sealed class FourierFilter2e6 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter2e6"/> class.
    /// </summary>
    public FourierFilter2e6()
    {
        this.cutoff = 2_000_000;
    } // ctor ()
} // internal sealed class FourierFilter2e6 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 5 MHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("A18A05B8-5DF7-4A11-ABA8-A3CEB7FE8D44")]
internal sealed class FourierFilter5e6 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter5e6"/> class.
    /// </summary>
    public FourierFilter5e6()
    {
        this.cutoff = 5_000_000;
    } // ctor ()
} // internal sealed class FourierFilter5e6 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 10 MHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("AB5BECD4-E807-44E9-B36F-41BD68EAF601")]
internal sealed class FourierFilter1e7 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter1e7"/> class.
    /// </summary>
    public FourierFilter1e7()
    {
        this.cutoff = 10_000_000;
    } // ctor ()
} // internal sealed class FourierFilter1e7 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 20 MHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("5CBC4B54-15AA-4614-8751-56716DF41CEB")]
internal sealed class FourierFilter2e7 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter2e7"/> class.
    /// </summary>
    public FourierFilter2e7()
    {
        this.cutoff = 20_000_000;
    } // ctor ()
} // internal sealed class FourierFilter2e7 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 50 MHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("3C101A68-94D5-4393-AD3B-F129591EC281")]
internal sealed class FourierFilter5e7 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter5e7"/> class.
    /// </summary>
    public FourierFilter5e7()
    {
        this.cutoff = 50_000_000;
    } // ctor ()
} // internal sealed class FourierFilter5e7 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 100 MHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("E96C92DF-9DF2-4064-B3EA-965B326774BE")]
internal sealed class FourierFilter1e8 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter1e8"/> class.
    /// </summary>
    public FourierFilter1e8()
    {
        this.cutoff = 100_000_000;
    } // ctor ()
} // internal sealed class FourierFilter1e8 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 200 MHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("28CEB5E1-5A38-40F9-B9AC-1EE2625BAB28")]
internal sealed class FourierFilter2e8 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter2e8"/> class.
    /// </summary>
    public FourierFilter2e8()
    {
        this.cutoff = 200_000_000;
    } // ctor ()
} // internal sealed class FourierFilter2e8 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 500 MHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("588067C2-93DB-4DF9-8A2F-6826CF55C1E3")]
internal sealed class FourierFilter5e8 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter5e8"/> class.
    /// </summary>
    public FourierFilter5e8()
    {
        this.cutoff = 500_000_000;
    } // ctor ()
} // internal sealed class FourierFilter5e8 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 1 GHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("9AAF9D11-4DFA-4A25-8676-C3A34DB76EFF")]
internal sealed class FourierFilter1e9 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter1e9"/> class.
    /// </summary>
    public FourierFilter1e9()
    {
        this.cutoff = 1_000_000_000;
    } // ctor ()
} // internal sealed class FourierFilter1e9 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 2 GHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("01774EEE-8C1A-426B-AC4F-F1B191EA8F41")]
internal sealed class FourierFilter2e9 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter2e9"/> class.
    /// </summary>
    public FourierFilter2e9()
    {
        this.cutoff = 2_000_000_000;
    } // ctor ()
} // internal sealed class FourierFilter2e9 : FourierFilter

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 5 GHz.
/// </summary>
[EquivalentSIMD(null)]
[Guid("880A8B62-62A6-4C95-9875-846EC578188C")]
internal sealed class FourierFilter5e9 : FourierFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilter5e9"/> class.
    /// </summary>
    public FourierFilter5e9()
    {
        this.cutoff = 5_000_000_000;
    } // ctor ()
} // internal sealed class FourierFilter5e9 : FourierFilter
