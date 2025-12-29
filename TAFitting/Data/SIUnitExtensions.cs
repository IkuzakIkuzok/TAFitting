
// (c) 2025 Kazuki KOHZUKI

namespace TAFitting.Data;

internal static class SIUnitExtensions
{
    extension (SIPrefix prefix)
    {
        /// <summary>
        /// Gets the SI prefix that is the reciprocal of the current prefix.
        /// </summary>
        internal SIPrefix Reciprocal
            => (SIPrefix)(-(int)prefix);
    }
} // internal static class SIUnitExtensions
