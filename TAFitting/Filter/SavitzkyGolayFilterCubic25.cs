
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Filter;

[EquivalentSIMD(null)]
internal sealed class SavitzkyGolayFilterCubic25 : SavitzkyGolayFilterCubic
{
    private static readonly double h = 1 / 5175.0;

    override protected void Initialize()
    {
        this.name = "Savitzky-Golay filter (cubic, 25 points)";
        this.description = "A Savitzky-Golay filter with a cubic polynomial and 25 points.";
        this.coefficient0 = 467 * h;
        this.coefficients = [
            462 * h, 447 * h, 422 * h, 387 * h, 343 * h, 287 * h, 222 * h, 147 * h, 62 * h, -33 * h, -138 * h, -253 * h,
        ];
    } // override protected void Initialize ()
} // internal sealed class SavitzkyGolayFilterCubic25 : SavitzkyGolayFilterCubic
