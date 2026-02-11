
// (c) 2026 Kazuki KOHZUKI

using System.Runtime.CompilerServices;

namespace TAFitting.Excel;

/// <summary>
/// Represents a fixed-size array of <see cref="ModelFunctionSegment"/> elements.
/// </summary>
[InlineArray(Capacity)]
internal struct ModelFunctionSegmentArray
{
    internal const int Capacity = 64;

    private ModelFunctionSegment _element0;
} // internal struct ModelFunctionSegmentArray
