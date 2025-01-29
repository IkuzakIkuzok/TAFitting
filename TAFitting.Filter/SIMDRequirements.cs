
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Filter;

/// <summary>
/// Represents SIMD requirements.
/// </summary>
[Flags]
public enum SIMDRequirements
{
    /// <summary>
    /// No SIMD requirements.
    /// </summary>
    None = 0,

    /// <summary>
    /// AVX.
    /// </summary>
    Avx = 0x00000001,

    /// <summary>
    /// AVX (x64).
    /// </summary>
    AvxX64 = 0x00000002,

    /// <summary>
    /// AVX2.
    /// </summary>
    Avx2 = 0x00000004,

    /// <summary>
    /// AVX2 (x64).
    /// </summary>
    Avx2X64 = 0x00000008,

    /// <summary>
    /// SSE.
    /// </summary>

    Sse = 0x00000010,

    /// <summary>
    /// SSE (x64).
    /// </summary>
    SseX64 = 0x00000020,

    /// <summary>
    /// SSE2.
    /// </summary>
    Sse2 = 0x00000040,

    /// <summary>
    /// SSE2 (x64).
    /// </summary>
    Sse2X64 = 0x00000080,

    /// <summary>
    /// SSE3.
    /// </summary>
    Sse3 = 0x00000100,

    /// <summary>
    /// SSE3 (x64).
    /// </summary>
    Sse3X64 = 0x00000200,

    /// <summary>
    /// SSSE3.
    /// </summary>
    Ssse3 = 0x00000400,

    /// <summary>
    /// SSSE3 (x64).
    /// </summary>
    Ssse3X64 = 0x00000800,

    /// <summary>
    /// SSE4.1.
    /// </summary>
    Sse41 = 0x00001000,

    /// <summary>
    /// SSE4.1 (x64).
    /// </summary>
    Sse41X64 = 0x00002000,

    /// <summary>
    /// SSE4.2.
    /// </summary>
    Sse42 = 0x00004000,

    /// <summary>
    /// SSE4.2 (x64).
    /// </summary>
    Sse42X64 = 0x00008000,
} // public enum SIMDRequirements
