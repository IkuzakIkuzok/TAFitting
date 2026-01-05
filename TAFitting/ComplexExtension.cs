
// (c) 2025 Kazuki Kohzuki

using System.Numerics;
using System.Runtime.CompilerServices;

namespace TAFitting;

/// <summary>
/// Provides extension methods for the <see cref="Complex"/> struct.
/// </summary>
internal static class ComplexExtension
{
    extension (Complex c)
    {
        /// <summary>
        /// Gets the squared magnitude of the complex number.
        /// </summary>
        /// <value>The squared magnitude of the complex number.</value>
        internal double MagnitudeSquared
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (c.Real * c.Real) + (c.Imaginary * c.Imaginary);
        }
    }
} // internal static class ComplexExtension
