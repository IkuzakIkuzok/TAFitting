﻿
// (c) 2025 Kazuki Kohzuki

using System.Diagnostics;
using System.Numerics;

namespace TAFitting.Filter.Fourier;

/*
 * Because the forward FFT is called twice in the inverse FFT, normalization is carried out in the inverse FFT.
 * (Other options are to normalize in the forward FFT by N or to normalize in both FFTs by √N.)
 */

/// <summary>
/// Provides methods for fast Fourier transform.
/// </summary>
internal static class FastFourierTransform
{
    /// <summary>
    /// Performs a forward Fourier transform on the specified buffer using the Split-Radix FFT algorithm.
    /// </summary>
    /// <param name="buffer">The buffer to transform.</param>
    /// <remarks>
    /// This is a transport of the C code by Takuya Ooura
    /// (<see href="https://www.kurims.kyoto-u.ac.jp/~ooura/fftman/ftmn1_24.html"/>).
    /// </remarks>
    internal static void Forward(Span<Complex> buffer)
    {
        var n = buffer.Length;

        Debug.Assert(((n & (n - 1)) == 0) && (n > 0), "The number of points must be a power of 2.");

        // scrambler
        var l = 0;
        for (var j = 1; j < n - 1; ++j)
        {
            for (var k = n >> 1; k > (l ^= k); k >>= 1) ;
            if (j < l)
                (buffer[j], buffer[l]) = (buffer[l], buffer[j]);
        }

        // L shaped butterflies
        var theta = -Math.PI / (n << 1);
        int mq;
        for (var m = 4; m <= n; m <<= 1)
        {
            mq = m >> 2;

            // W == 1
            for (var k = mq; k >= 1; k >>= 2)
            {
                for (var j = mq - k; j < mq - (k >> 1); ++j)
                {
                    var j1 = j + mq;
                    var j2 = j1 + mq;
                    var j3 = j2 + mq;
                    var x1 = buffer[j] - buffer[j1];
                    buffer[j] += buffer[j1];
                    var x3 = buffer[j3] - buffer[j2];
                    buffer[j2] += buffer[j3];
                    buffer[j1] = new(x1.Real - x3.Imaginary, x1.Imaginary + x3.Real);
                    buffer[j3] = new(x1.Real + x3.Imaginary, x1.Imaginary - x3.Real);
                }
            }
            if (m == n) continue;

            // W == exp(-pi*i/4)
            var irev = n >> 1;
            var w1r = Math.Cos(theta * irev);
            for (var k = mq; k >= 1; k >>= 2)
            {
                for (var j = m + mq - k; j < m + mq - (k >> 1); ++j)
                {
                    var j1 = j + mq;
                    var j2 = j1 + mq;
                    var j3 = j2 + mq;
                    var x1 = buffer[j] - buffer[j1];
                    buffer[j] += buffer[j1];
                    var x3 = buffer[j3] - buffer[j2];
                    buffer[j2] += buffer[j3];
                    var x0r = x1.Real - x3.Imaginary;
                    var x0i = x1.Imaginary + x3.Real;
                    buffer[j1] = w1r * new Complex(x0r + x0i, x0i - x0r);
                    x0r = x1.Real + x3.Imaginary;
                    x0i = x1.Imaginary - x3.Real;
                    buffer[j3] = w1r * new Complex(-x0r + x0i, -x0i - x0r);
                }
            }

            // W != 1, exp(-pi*i/4)
            for (var i = (m << 1); i < n; i += m)
            {
                for (var k = n >> 1; k > (irev ^= k); k >>= 1) ;
                var w1 = Complex.FromPolarCoordinates(1, theta * irev);
                var w3 = Complex.FromPolarCoordinates(1, 3 * theta * irev);
                for (var k = mq; k >= 1; k >>= 2)
                {
                    for (var j = i + mq - k; j < i + mq - (k >> 1); ++j)
                    {
                        var j1 = j + mq;
                        var j2 = j1 + mq;
                        var j3 = j2 + mq;
                        var x1 = buffer[j] - buffer[j1];
                        buffer[j] += buffer[j1];
                        var x3 = buffer[j3] - buffer[j2];
                        buffer[j2] += buffer[j3];
                        var x0r = x1.Real - x3.Imaginary;
                        var x0i = x1.Imaginary + x3.Real;
                        buffer[j1] = new(w1.Real * x0r - w1.Imaginary * x0i, w1.Real * x0i + w1.Imaginary * x0r);
                        x0r = x1.Real + x3.Imaginary;
                        x0i = x1.Imaginary - x3.Real;
                        buffer[j3] = new(w3.Real * x0r - w3.Imaginary * x0i, w3.Real * x0i + w3.Imaginary * x0r);
                    }
                }
            }
        }

        // radix 2 butterflies
        mq = n >> 1;
        for (var k = mq; k >= 1; k >>= 2)
        {
            for (var j = mq - k; j < mq - (k >> 1); ++j)
            {
                var j1 = j + mq;
                var x0 = buffer[j] - buffer[j1];
                buffer[j] += buffer[j1];
                buffer[j1] = x0;
            }
        }
    } // internal static void Forward (Span<Complex>)

    /// <summary>
    /// Performs an inverse Fourier transform on the specified buffer using the Split-Radix FFT algorithm.
    /// </summary>
    /// <param name="buffer">The buffer to transform.</param>
    internal static void Inverse(Span<Complex> buffer)
    {
        /*
         * Inverse FFT:
         * 1. Conjugate the input.
         * 2. Forward FFT.
         * 3. Conjugate the output (and normalize).
         */

        for (var i = 0; i < buffer.Length; ++i)
            buffer[i] = Complex.Conjugate(buffer[i]);

        Forward(buffer);

        for (var i = 0; i < buffer.Length; ++i)
            buffer[i] = Complex.Conjugate(buffer[i]) / buffer.Length;
    } // internal static void Inverse (Span<Complex>)

    /// <summary>
    /// Performs an inverse Fourier transform on the specified buffer using the Split-Radix FFT algorithm.
    /// </summary>
    /// <param name="fft">The buffer to transform.</param>
    /// <returns>The real part of the inverse Fourier transform.</returns>
    /// <remarks>
    /// The inverse FFT is performed on <paramref name="fft"/> in place,
    /// and its elements are modified by calling this method.
    /// </remarks>
    public static double[] InverseReal(Complex[] fft)
    {
        for (var i = 0; i < fft.Length; ++i)
            fft[i] = Complex.Conjugate(fft[i]);

        Forward(fft);

        // Actual inverse FFT is the complex conjugate of the forward FFT divided by the number of points.
        // However, we only need the real part of the result.
        // Therefore, computation of the complex conjugate is omitted.
        var result = new double[fft.Length];
        for (var i = 0; i < fft.Length; ++i)
            result[i] = fft[i].Real / fft.Length;
        return result;
    } // public static double[] InverseReal (Complex[])

    /// <summary>
    /// Returns the frequency scale.
    /// </summary>
    /// <param name="length">The number of points.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="positiveOnly">A value indicating whether to return only positive frequencies.</param>
    /// <returns>The frequency scale.</returns>
    internal static double[] FrequencyScale(int length, double sampleRate, bool positiveOnly)
    {
        var freq = new double[length];

        if (positiveOnly)
        {
            var a = sampleRate / (length - 1) / 2;
            for (var i = 0; i < length; ++i)
                freq[i] = a * i;
            return freq;
        }
   
        var b = sampleRate / length;
        var half = length >> 1;
        for (var i = 0; i < half; ++i)
            freq[i] = b * i;
        for (var i = half; i < length; ++i)
            freq[i] = b * (i - length);
        return freq;
    } // internal static double[] FrequencyScale (int, double, bool)

    internal static bool CheckEvenlySpaced(IReadOnlyList<double> values, double threshold = 1e-6)
    {
        var n = values.Count;
        var dt = values[1] - values[0];
        for (var i = 2; i < n; ++i)
            if (Math.Abs(values[i] - values[i - 1] - dt) > threshold)
                return false;
        return true;
    } // internal static bool CheckEvenlySpaced (IReadOnlyList<double>, [double])
} // internal static class FastFourierTransform
