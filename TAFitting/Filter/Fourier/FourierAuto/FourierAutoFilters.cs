
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Filter.Fourier.FourierAuto;

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 1% of time bandwidth.
/// </summary>
[EquivalentSIMD(null)]
[Guid("B7A04641-4CBC-4B6C-AA9B-FED215E06021")]
internal sealed class FourierFilterAuto01 : FourierFilterAuto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilterAuto01"/> class.
    /// </summary>
    public FourierFilterAuto01()
    {
        this.ratio = 0.01;
    } // ctor ()
} // internal sealed class FourierFilterAuto01 : FourierFilterAuto

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 5% of time bandwidth.
/// </summary>
[EquivalentSIMD(null)]
[Guid("8C230AC9-0D06-49D9-A66A-64AEF5E28A9A")]
internal sealed class FourierFilterAuto05 : FourierFilterAuto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilterAuto05"/> class.
    /// </summary>
    public FourierFilterAuto05()
    {
        this.ratio = 0.05;
    } // ctor ()
} // internal sealed class FourierFilterAuto05 : FourierFilterAuto

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 10% of time bandwidth.
/// </summary>
[EquivalentSIMD(null)]
[Guid("BCD6AB78-3DE5-4D9A-BDD4-FD118970401E")]
internal sealed class FourierFilterAuto10 : FourierFilterAuto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilterAuto10"/> class.
    /// </summary>
    public FourierFilterAuto10()
    {
        this.ratio = 0.10;
    } // ctor ()
} // internal sealed class FourierFilterAuto10 : FourierFilterAuto

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency of 20% of time bandwidth.
/// </summary>
[EquivalentSIMD(null)]
[Guid("A8A0FDEB-2F11-40CD-8EBF-4B2F9AAA65CB")]
internal sealed class FourierFilterAuto20 : FourierFilterAuto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourierFilterAuto20"/> class.
    /// </summary>
    public FourierFilterAuto20()
    {
        this.ratio = 0.20;
    } // ctor ()
} // internal sealed class FourierFilterAuto20 : FourierFilterAuto
