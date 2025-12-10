
// (c) 2025 Kazuki Kohzuki

using System.Numerics;
using System.Runtime.CompilerServices;

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
    /// Performs a forward Fourier transform on the specified buffer.
    /// </summary>
    /// <param name="buffer">The buffer to transform.</param>
    /// <remarks>
    /// This method uses the Cooley-Tukey FFT algorithm for the number of points that is not a power of 2;
    /// otherwise, it uses the Split-Radix FFT algorithm.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Forward(Span<Complex> buffer)
    {
        var n = buffer.Length;
        if (CheckPowerOfTwo(n))
        {
            // Split-Radix FFT is significanly faster than Cooley-Tukey FFT for n = 2^k.
            ForwardSplitRadix(buffer);
            return;
        }

        ForwardCooleyTukey(buffer);
    } // internal static void Forward (Span<Complex>)

    /// <summary>
    /// Performs a forward Fourier transform on the specified buffer using the Cooley-Tukey FFT algorithm.
    /// </summary>
    /// <param name="buffer">The buffer to transform.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ForwardCooleyTukey(Span<Complex> buffer)
    {
        var n = buffer.Length;
        var theta = -2 * Math.PI / n;

        // Max stackalloc size is limited to 256 KiB, which is far less than the stack size (1 MiB).
        var temp = n <= 0x4000 ? stackalloc Complex[n] : new Complex[n];

        ForwardCooleyTukey(n, buffer, theta, temp);
    } // internal static void ForwardCooleyTukey (Span<Complex>)

    /// <summary>
    /// Performs a forward Fourier transform on the specified buffer using the Cooley-Tukey FFT algorithm.
    /// </summary>
    /// <param name="n">The number of points.</param>
    /// <param name="buffer">The buffer to transform.</param>
    /// <param name="theta">The angle increment.</param>
    /// <param name="temp">The temporary buffer.</param>
    /// <remarks>
    /// This is a transport of the C code by Takuya Ooura
    /// (<see href="https://www.kurims.kyoto-u.ac.jp/~ooura/fftman/ftmn1_23.html"/>).
    /// </remarks>
    private static void ForwardCooleyTukey(int n, Span<Complex> buffer, double theta, Span<Complex> temp)
    {
        if (n <= 1) return;

        int radix;
        Complex w;

        // factorization
        for (radix = 2; radix * radix <= n; ++radix)
            if (n % radix == 0) break;
        if (n % radix != 0) radix = n;
        var n_radix = n / radix;

        // butterflies
        for (var j = 0; j < n_radix; ++j)
        {
            for (var m = 0; m < radix; ++m)
            {
                var xr = buffer[j].Real;
                var xi = buffer[j].Imaginary;
                for (var r = n_radix; r < n; r += n_radix)
                {
                    w = Complex.FromPolarCoordinates(1, theta * m * r);
                    var y = buffer[r + j];
                    xr += w.Real * y.Real - w.Imaginary * y.Imaginary;
                    xi += w.Real * y.Imaginary + w.Imaginary * y.Real;
                }
                w = Complex.FromPolarCoordinates(1, theta * m * j);
                var tr = xr * w.Real - xi * w.Imaginary;
                var ti = xi * w.Real + xr * w.Imaginary;
                temp[m * n_radix + j] = new(tr, ti);
            }
        }

        if (n_radix > 1)
        {
            for (var r = 0; r < n; r += n_radix)
            {
                ForwardCooleyTukey(
                    n     : n_radix,
                    buffer: temp[r..],
                    theta : theta * radix,
                    temp  : buffer
                );
            }
        }

        for (var j = 0; j < n_radix; ++ j)
        {
            for (var m = 0; m < radix; ++m)
                buffer[radix * j + m] = temp[m * n_radix + j];
        }
    } // private static void ForwardCooleyTukey (Span<Complex>, double, Span<Complex>)

    /// <summary>
    /// Performs a forward Fourier transform on the specified buffer using the Split-Radix FFT algorithm.
    /// </summary>
    /// <param name="buffer">The buffer to transform.</param>
    /// <exception cref="ArgumentException">The number of points must be a power of 2.</exception>
    /// <remarks>
    /// This is a transport of the C code by Takuya Ooura
    /// (<see href="https://www.kurims.kyoto-u.ac.jp/~ooura/fftman/ftmn1_24.html"/>).
    /// </remarks>
    internal static void ForwardSplitRadix(Span<Complex> buffer)
    {
        var n = buffer.Length;

        if (!CheckPowerOfTwo(n))
            throw new ArgumentException("The number of points must be a power of 2.");

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
    } // internal static void ForwardSplitRadix (Span<Complex>)

    /// <summary>
    /// Performs an inverse Fourier transform on the specified buffer.
    /// </summary>
    /// <param name="buffer">The buffer to transform.</param>
    internal static void Inverse(Span<Complex> buffer)
    {
        /*
         * Inverse FFT:
         * 1. Conjugate the input.
         * 2. ForwardSplitRadix FFT.
         * 3. Conjugate the output (and normalize).
         */

        for (var i = 0; i < buffer.Length; ++i)
            buffer[i] = Complex.Conjugate(buffer[i]);

        Forward(buffer);

        for (var i = 0; i < buffer.Length; ++i)
            buffer[i] = Complex.Conjugate(buffer[i]) / buffer.Length;
    } // internal static void Inverse (Span<Complex>)

    /// <summary>
    /// Performs an inverse Fourier transform on the specified buffer.
    /// </summary>
    /// <param name="fft">The buffer to transform.</param>
    /// <returns>The real part of the inverse Fourier transform.</returns>
    /// <remarks>
    /// The inverse FFT is performed on <paramref name="fft"/> in place,
    /// and its elements are modified by calling this method.
    /// </remarks>
    public static double[] InverseReal(Span<Complex> fft)
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
    } // public static double[] InverseReal (Span<Complex>)

    
    /// <summary>
    /// Populates the specified frequency span with frequency bin values corresponding to the given sample rate, using either a full or positive-only scale.
    /// </summary>
    /// <param name="freq">The span to be filled with calculated frequency bin values. Each element will be set to the frequency corresponding to its index.</param>
    /// <param name="sampleRate">The sample rate, in hertz, used to determine the frequency bin spacing.</param>
    /// <param name="positiveOnly">If <see langword="true"/>, only positive frequency values are calculated; otherwise, both positive and negative frequencies are included.</param>
    internal static void FrequencyScale(Span<double> freq, double sampleRate, bool positiveOnly)
    {
        var length = freq.Length;
        if (length == 0) return;

        if (positiveOnly)
        {
            var a = sampleRate / (length - 1) / 2;
            for (var i = 0; i < length; ++i)
                freq[i] = a * i;
            return;
        }
   
        var b = sampleRate / length;
        var half = length >> 1;
        for (var i = 0; i < half; ++i)
            freq[i] = b * i;
        for (var i = half; i < length; ++i)
            freq[i] = b * (i - length);
    } // internal static void FrequencyScale (Span<double>, double, bool)

    /// <summary>
    /// Checks whether the specified values are evenly spaced.
    /// </summary>
    /// <param name="values">The values to check.</param>
    /// <param name="threshold">The threshold for the difference between the values.</param>
    /// <returns><see langword="true"/> if the values are evenly spaced; otherwise, <see langword="false"/>.</returns>
    internal static bool CheckEvenlySpaced(IReadOnlyList<double> values, double threshold = 1e-6)
    {
        var n = values.Count;
        var dt = values[1] - values[0];
        for (var i = 2; i < n; ++i)
            if (Math.Abs(values[i] - values[i - 1] - dt) > threshold)
                return false;
        return true;
    } // internal static bool CheckEvenlySpaced (IReadOnlyList<double>, [double])

    /// <summary>
    /// Checks whether the specified number is a power of two.
    /// </summary>
    /// <param name="n">The number to check.</param>
    /// <returns><see langword="true"/> if the number is a power of two; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool CheckPowerOfTwo(int n)
        => ((n & (n - 1)) == 0) && (n > 0);
} // internal static class FastFourierTransform
