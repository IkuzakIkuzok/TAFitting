
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Filter;

[EquivalentSIMD(null)]
internal class SavitzkyGolayFilterCubic15 : SavitzkyGolayFilterCubic
{
    private static readonly double h = 1 / 1105.0;

    override protected void Initialize()
    {
        this.name = "Savitzky-Golay filter (cubic, 15 points)";
        this.description = "A Savitzky-Golay filter with a cubic polynomial and 15 points.";
        this.coefficient0 = 167 * h;
        this.coefficients = [
            162 * h, 147 * h, 122 * h, 87 * h, 42 * h, -13 * h, -78 * h
        ];
    } // override protected void Initialize ()
} // internal class SavitzkyGolayFilterCubic15 : SavitzkyGolayFilterCubic
