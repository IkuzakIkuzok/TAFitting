
// (c) 2025 Kazuki KOHZUKI

namespace TAFitting;

internal static class ExceptionExtensions
{
    extension (IndexOutOfRangeException)
    {
        /// <summary>
        /// Throws an exception if the specified index is outside the valid range for a collection of the given length.
        /// </summary>
        /// <param name="index">The zero-based index to validate.</param>
        /// <param name="length">The total number of elements in the collection.</param>
        internal static void ThorwIfIndexOutOfRange(int index, int length)
        {
            if ((uint)index >= (uint)length)
                throw new IndexOutOfRangeException($"Index {index} is out of range for {length}.");
        } // internal static void ThorwIfIndexOutOfRange (int, int)
    } // extension (IndexOutOfRangeException)
} // internal static class ExceptionExtensions
