
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Filter;

[EquivalentSIMD(null)]
[Guid("3F0D8F4F-7EE6-49A6-BB39-08DF05F4E64E")]
internal class SavitzkyGolayFilterCubic15 : ConvolutionFilter
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
} // internal class SavitzkyGolayFilterCubic15 : ConvolutionFilter
