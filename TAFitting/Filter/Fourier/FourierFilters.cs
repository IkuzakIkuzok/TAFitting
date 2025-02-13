
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Filter.Fourier;

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
