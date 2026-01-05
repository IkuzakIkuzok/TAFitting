
// (c) 2025 Kazuki Kohzuki

using System.Numerics;
using System.Runtime.CompilerServices;

namespace TAFitting.Filter.Fourier;

/*
 * Optimized FFT implementation for Tekave data (size = 2499 = 3 x 7 x 7 x 17).
 * 
 * Algorithm Stages:
 *   1. Radix-3: 2499 -> 3 x 833
 *   2. Radix-7: 833 -> 7 x 119
 *   3. Radix-7: 119 -> 7 x 17
 *   4. Radix-17: 17 -> 17 x 1
 *   5. Permutation: Final reordering of output data
 *   
 * All twiddle factors are precomputed and stored in static readonly arrays for performance.
 */

file static class TekaveFftHelper
{
    // For Radix-3 Pass (Stage 1): Stride 833, 2 sets of twiddles -> 1666 elements
    // [0..832] for W^1, [833..1665] for W^2
    private static readonly Complex[] twiddles3 = new Complex[1666];

    // For Radix-7 Pass (Stage 2): Block size 833, stride 119, 6 sets of twiddles -> 714 elements
    private static readonly Complex[] twiddles7_Stage2 = new Complex[714];

    // For Radix-7 Pass (Stage 3): Block size 119, stride 17, 6 sets of twiddles -> 102 elements
    private static readonly Complex[] twiddles7_Stage3 = new Complex[102];

    // For KernelRadix17 (Stage 4)
    private static readonly Complex[] twiddles17 = new Complex[289];

    /// <summary>
    /// Initializes static lookup tables required for optimized FFT computations in the Tekave module.
    /// </summary>
    [ModuleInitializer]
    internal static void InitializeTekave()
    {
        if (twiddles17[1] != Complex.Zero) return;

        // 2. Radix-3 Tables (N=2499)
        // stride=833. Indices: k=0..832
        for (var k = 0; k < 833; k++)
        {
            var angle = -Math.Tau * k / 2499.0;
            twiddles3[k] = Complex.FromPolarCoordinates(1.0, angle);     // W^k
            twiddles3[833 + k] = Complex.FromPolarCoordinates(1.0, angle * 2); // W^2k
        }

        // 3. Radix-7 Stage 2 Tables (Block=833)
        // stride=119. Indices: k=0..118
        // W_{833}^k, W_{833}^{2k}, ...
        FillRadix7Table(twiddles7_Stage2, 119, 833);

        // 4. Radix-7 Stage 3 Tables (Block=119)
        // stride=17. Indices: k=0..16
        // W_{119}^k, W_{119}^{2k}, ...
        FillRadix7Table(twiddles7_Stage3, 17, 119);

        for (var k = 0; k < 17; k++)
        {
            for (var n = 0; n < 17; n++)
            {
                // Forward FFT: angle = -2pi * k * n / 17
                var angle = -Math.Tau * k * n / 17.0;
                twiddles17[k * 17 + n] = Complex.FromPolarCoordinates(1.0, angle);
            }
        }
    } // internal static void InitializeTekave ()

    private static void FillRadix7Table(Span<Complex> table, int stride, int nSize)
    {
        for (var k = 0; k < stride; k++)
        {
            var baseAngle = -Math.Tau * k / nSize;
            for (var m = 1; m <= 6; m++) // W^1k ... W^6k
            {
                // Table layout: [m-1 * stride + k]
                // m=1 -> 0*stride + k
                // m=6 -> 5*stride + k
                table[(m - 1) * stride + k] = Complex.FromPolarCoordinates(1.0, baseAngle * m);
            }
        }
    } // private static void FillRadix7Table (Span<Complex>, int, int)

    internal static void ForwardTekave(Span<Complex> buffer)
    {
        // Stage 1: Radix-3 (2499 -> 3 x 833)
        PassRadix3(buffer, 833);

        // Stage 2: Radix-7 (833 -> 7 x 119)
        for (var i = 0; i < 3; i++)
            PassRadix7(buffer.Slice(i * 833, 833), 119, twiddles7_Stage2);

        // Stage 3: Radix-7 (119 -> 7 x 17)
        for (var i = 0; i < 21; i++)
            PassRadix7(buffer.Slice(i * 119, 119), 17, twiddles7_Stage3);

        // Stage 4: Radix-17 (17 -> 17 x 1)
        for (var i = 0; i < 147; i++)
            KernelRadix17(buffer.Slice(i * 17, 17));

        // Final Stage: Permutation
        Permute(buffer);
    } // internal static void ForwardTekave(Span<Complex> buffer)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PassRadix3(Span<Complex> x, int stride)
    {
        // sin(2pi/3) = sqrt(3)/2
        var c3_1 = new Complex(0, 0.86602540378443864676);

        ref var tRef = ref MemoryMarshal.GetArrayDataReference(twiddles3);

        for (var k = 0; k < stride; k++)
        {
            var x0 = x[k];
            var x1 = x[k + stride];
            var x2 = x[k + 2 * stride];

            var t1 = x1 + x2;
            var x0_new = x0 + t1;

            var diff = x1 - x2;
            var t2 = diff * c3_1;
            var t3 = x0 - 0.5 * t1;

            x[k] = x0_new;
            var out1 = t3 - t2;
            var out2 = t3 + t2;

            if (k > 0)
            {
                // W^1
                out1 *= Unsafe.Add(ref tRef, k);
                // W^2 (offset = stride)
                out2 *= Unsafe.Add(ref tRef, stride + k);
            }

            x[k + stride] = out1;
            x[k + 2 * stride] = out2;
        }
    } // private static void PassRadix3 (Span<Complex>, int)

    private static void PassRadix7(Span<Complex> x, int stride, Complex[] twiddleTable)
    {
        // const double u = Math.Tau / 7.0;
        // var (s1, c1) = Math.SinCos(u);
        // var (s2, c2) = Math.SinCos(2 * u);
        // var (s3, c3) = Math.SinCos(3 * u);
        const double c1 = +0.6234898018587335;
        const double s1 = +0.7818314824680298;
        const double c2 = -0.22252093395631434;
        const double s2 = +0.97492791218182362;
        const double c3 = -0.9009688679024191;
        const double s3 = +0.43388373911755823;

        // Offsets
        var s_1 = stride;
        var s_2 = stride * 2;
        var s_3 = stride * 3;
        var s_4 = stride * 4;
        var s_5 = stride * 5;
        var s_6 = stride * 6;

        ref var tBase = ref MemoryMarshal.GetArrayDataReference(twiddleTable);

        for (var k = 0; k < stride; k++)
        {
            // Indices
            var k1 = k + s_1;
            var k2 = k + s_2;
            var k3 = k + s_3;
            var k4 = k + s_4;
            var k5 = k + s_5;
            var k6 = k + s_6;

            var v0 = x[k];
            var v1 = x[k1]; var v6 = x[k6];
            var v2 = x[k2]; var v5 = x[k5];
            var v3 = x[k3]; var v4 = x[k4];

            var t1_6 = v1 + v6; var d1_6 = v1 - v6;
            var t2_5 = v2 + v5; var d2_5 = v2 - v5;
            var t3_4 = v3 + v4; var d3_4 = v3 - v4;

            var s_sum = v0 + t1_6 + t2_5 + t3_4;

            var r1 = v0 + t1_6 * c1 + t2_5 * c2 + t3_4 * c3;
            var r2 = v0 + t1_6 * c2 + t2_5 * c3 + t3_4 * c1;
            var r3 = v0 + t1_6 * c3 + t2_5 * c1 + t3_4 * c2;

            var i1 = d1_6 * s1 + d2_5 * s2 + d3_4 * s3;
            var i2 = d1_6 * s2 - d2_5 * s3 - d3_4 * s1;
            var i3 = d1_6 * s3 - d2_5 * s1 + d3_4 * s2;

            var j_i1 = new Complex(-i1.Imaginary, i1.Real);
            var j_i2 = new Complex(-i2.Imaginary, i2.Real);
            var j_i3 = new Complex(-i3.Imaginary, i3.Real);

            x[k] = s_sum;

            var o1 = r1 - j_i1; var o6 = r1 + j_i1;
            var o2 = r2 - j_i2; var o5 = r2 + j_i2;
            var o3 = r3 - j_i3; var o4 = r3 + j_i3;

            if (k > 0)
            {
                // Table layout: [m-1 * stride + k]
                // m=1 -> 0 + k
                o1 *= Unsafe.Add(ref tBase, k);
                // m=2 -> stride + k
                o2 *= Unsafe.Add(ref tBase, s_1 + k);
                // m=3 -> 2*stride + k
                o3 *= Unsafe.Add(ref tBase, s_2 + k);
                // m=4 -> 3*stride + k
                o4 *= Unsafe.Add(ref tBase, s_3 + k);
                // m=5 -> 4*stride + k
                o5 *= Unsafe.Add(ref tBase, s_4 + k);
                // m=6 -> 5*stride + k
                o6 *= Unsafe.Add(ref tBase, s_5 + k);
            }

            x[k1] = o1; x[k6] = o6;
            x[k2] = o2; x[k5] = o5;
            x[k3] = o3; x[k4] = o4;
        }
    } // private static void PassRadix7 (Span<Complex>, int, Complex[])

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    private static void KernelRadix17(Span<Complex> x)
    {
        var tmp = (stackalloc Complex[17]);

        ref var twiddleRef = ref MemoryMarshal.GetArrayDataReference(twiddles17);
        ref var xRefStart = ref MemoryMarshal.GetReference(x);

        for (var k = 0; k < 17; k++)
        {
            var sum = Complex.Zero;

            for (var n = 0; n < 17; n++)
            {
                // x[n]
                var input = Unsafe.Add(ref xRefStart, n);

                // twiddles17[k * 17 + n]
                var w = twiddleRef;

                sum += input * w;
                twiddleRef = ref Unsafe.Add(ref twiddleRef, 1);
            }

            tmp[k] = sum;
        }

        tmp.CopyTo(x);
    } // private static void KernelRadix17 (Span<Complex>)

    private static void Permute(Span<Complex> buffer)
    {
        var temp = (stackalloc Complex[2499]);
        buffer.CopyTo(temp);

        const int s0 = 833;
        const int s1 = 119;
        const int s2 = 17;

        for (var i = 0; i < 2499; i++)
        {
            var r = i;
            var d0 = r / s0; r %= s0;
            var d1 = r / s1; r %= s1;
            var d2 = r / s2; r %= s2;
            var d3 = r;

            var dest = d0 + d1 * 3 + d2 * 21 + d3 * 147;
            buffer[dest] = temp[i];
        }
    } // private static void Permute (Span<Complex>)
} // file static class TekaveFftHelper

internal static partial class FastFourierTransform
{
    private const int TekaveSize = 2499;

    /// <summary>
    /// Performs a forward Fourier transform on the specified buffer using the optimized algorithm for Tekave data.
    /// </summary>
    /// <param name="buffer">A span of complex numbers containing the input data to be transformed. The length of the span must be equal to 2499.</param>
    /// <exception cref="ArgumentException">Thrown if the length of buffer is not equal to 2499.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ForwardTekave(Span<Complex> buffer)
    {
        CheckBufferSize(buffer);
        TekaveFftHelper.ForwardTekave(buffer);
    } // internal static void ForwardTekave (Span<Complex>)

    private static void CheckBufferSize(Span<Complex> buffer)
    {
        if (buffer.Length != TekaveSize)
            throw new ArgumentException($"The number of points must be {TekaveSize}.", nameof(buffer));
    } // private static void CheckBufferSize (Span<Complex>)
} // internal static partial class FastFourierTransform
