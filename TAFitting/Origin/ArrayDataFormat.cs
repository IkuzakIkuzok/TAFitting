
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Origin;

/// <summary>
/// Represents the format of the array data.
/// </summary>
internal enum ArrayDataFormat
{
    /// <summary>
    /// A 2D array with variant data.
    /// </summary>
    Array2DVariant = 0,

    /// <summary>
    /// A 1D array with numeric data.
    /// </summary>
    Array1DNumeric = 1,

    /// <summary>
    /// A 2D array with numeric data.
    /// </summary>
    Array2DNumeric = 2,

    /// <summary>
    /// A 2D array with text data.
    /// </summary>
    Array2DText = 3,

    /// <summary>
    /// A 2D array with text data containing full precision.
    /// </summary>
    Array2DTextFullPrecision = 4,

    /// <summary>
    /// A 1D array with variant data.
    /// </summary>
    Array1DVariant = 5,

    /// <summary>
    /// A 1D array with text data.
    /// </summary>
    Array1DText = 6,

    /// <summary>
    /// A 1D array with text data containing full precision.
    /// </summary>
    Array1DTextFullPrecision = 7,

    /// <summary>
    /// A 1D array with string data.
    /// </summary>
    Array1DStr = 8,

    /// <summary>
    /// A 2D array with string data.
    /// </summary>
    Array2DStr = 9,

    /// <summary>
    /// A 1D array with string data containing full precision.
    /// </summary>
    Array1DStrFullPrecision = 10,

    /// <summary>
    /// A 2D array with string data containing full precision.
    /// </summary>
    Array2DStrFullPrecision = 11,
} // internal enum ArrayDataFormat
