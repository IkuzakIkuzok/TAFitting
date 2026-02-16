
// (c) 2026 Kazuki KOHZUKI

using System.Runtime.CompilerServices;

namespace TAFitting.Excel.Formulas;

/// <summary>
/// Provides helper methods for calculating the length of row and column indices when formatting Excel formulas.
/// </summary>
internal static class FormattingHelper
{
    /// <summary>
    /// Calculates the number of decimal digits required to represent the specified unsigned integer value.
    /// </summary>
    /// <returns>The number of decimal digits needed to represent the value of <paramref name="index"/> in base 10.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetRowIndexLength(uint index)
    {
        // Algorithm based on https://lemire.me/blog/2021/06/03/computing-the-number-of-digits-of-an-integer-even-faster.
        ReadOnlySpan<long> table = [
            4294967296,
            8589934582,
            8589934582,
            8589934582,
            12884901788,
            12884901788,
            12884901788,
            17179868184,
            17179868184,
            17179868184,
            21474826480,
            21474826480,
            21474826480,
            21474826480,
            25769703776,
            25769703776,
            25769703776,
            30063771072,
            30063771072,
            30063771072,
            34349738368,
            34349738368,
            34349738368,
            34349738368,
            38554705664,
            38554705664,
            38554705664,
            41949672960,
            41949672960,
            41949672960,
            42949672960,
            42949672960,
        ];

        var tableValue = table[(int)uint.Log2(index)];
        return (int)((index + tableValue) >> 32);
    } // internal static int GetRowIndexLength (uint)

    /*
     * Following branchless version is slower than the version using if statements for sequential checks.
     * This is because the branch predictor in modern CPUs can easily predict the outcome of such simple comparisons,
     * making the branchless approach less efficient due to the overhead of additional arithmetic operations.
     * However, for random access patterns where branch prediction is less effective, the branchless version MAY perform better.
     * DO measure performance before changing to the branchless version in such cases.
     * 
     * Note that the simple branch strategy does NOT work well for GetRowIndexLength(uint) due to the larger number of possible lengths (1 to 7).
     
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetColumnIndexLength(uint index)
    {
        var i = (int)index;
        return 1 - ((26 - i) >> 31) - ((702 - i) >> 31);
    } // internal static int GetColumnIndexLength (uint)
    */

    /// <summary>
    /// Determines the number of characters required to represent the specified column index in Excel-style column notation.
    /// </summary>
    /// <param name="index">The column index to evaluate. Must be in the range 1 to 16384, corresponding to Excel columns A to XFD.</param>
    /// <returns>The number of characters needed to represent the column index.</returns>
    [method:MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetColumnIndexLength(uint index)
    {
        if (index <= 26u) return 1;  // Z = 26
        if (index <= 702u) return 2; // ZZ = 26 * (26 + 1)
        return 3; // Maximum column index in Excel is 16384 ("XFD")
    } // internal static int GetColumnIndexLength (uint)
} // internal static class FormattingHelper
