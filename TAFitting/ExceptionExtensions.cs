
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

    extension (InvalidOperationException)
    {
        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> with the specified message if the given condition is true.
        /// </summary>
        /// <param name="condition">The condition to evaluate. If <see langword="true"/>, an exception is thrown.</param>
        /// <param name="message">The exception message to use if the condition is met.</param>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="condition"/> is <see langword="true"/>.</exception>
        internal static void ThrowIf(bool condition, string message)
        {
            if (condition)
                throw new InvalidOperationException(message);
        } // internal static void ThrowIf (bool, string)
    }
} // internal static class ExceptionExtensions
