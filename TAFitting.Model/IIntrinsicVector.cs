
// (c) 2024 Kazuki Kohzuki

using System.Numerics;

#pragma warning disable IDE0130

namespace TAFitting.Data;

#pragma warning restore

/// <summary>
/// An AVX vector.
/// </summary>
public interface IIntrinsicVector<TSelf>
    : IAdditionOperators<TSelf, TSelf, TSelf>, ISubtractionOperators<TSelf, TSelf, TSelf>, IMultiplyOperators<TSelf, TSelf, TSelf>, IDivisionOperators<TSelf, TSelf, TSelf>
    where TSelf : IIntrinsicVector<TSelf>
{
    /// <summary>
    /// Gets a value indicating whether the vector is readonly.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the vector is readonly; otherwise, <see langword="false"/>.
    /// </value>
    bool IsReadonly { get; }

    /// <summary>
    /// Gets the sum of the elements.
    /// </summary>
    double Sum { get; }

    /// <summary>
    /// Loads the specified values.
    /// </summary>
    /// <param name="values">The values.</param>
    abstract void Load(double[] values);

    /// <summary>
    /// Loads the specified value to all elements.
    /// </summary>
    /// <param name="value">The value.</param>
    abstract void Load(double value);

    /// <summary>
    /// Creates a new instance of the <see cref="IIntrinsicVector{TSelf}"/> class
    /// with the specified values.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>A new instance of the <see cref="IIntrinsicVector{TSelf}"/> class with the <paramref name="values"/>.</returns>
    abstract static TSelf Create(double[] values);

    /// <summary>
    /// Creates a new instance of the <see cref="IIntrinsicVector{TSelf}"/> class
    /// containing the specified value in all elements.
    /// </summary>
    /// <param name="length">The length.</param>
    /// <param name="value">The value.</param>
    /// <returns>A new instance of the <see cref="IIntrinsicVector{TSelf}"/> class with the <paramref name="value"/>.</returns>
    abstract static TSelf Create(int length, double value);

    /// <summary>
    /// Creates a new instance of the <see cref="IIntrinsicVector{TSelf}"/> class
    /// with the specified length.
    /// </summary>
    /// <param name="length">The length.</param>
    /// <returns>A new instance of the <see cref="IIntrinsicVector{TSelf}"/> class with the <paramref name="length"/>.</returns>
    abstract static TSelf Create(int length);

    /// <summary>
    /// Creates a new instance of the <see cref="IIntrinsicVector{TSelf}"/> class
    /// with the specified values and makes it read-only.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>A new read-only instance of the <see cref="IIntrinsicVector{TSelf}"/> class with the <paramref name="values"/>.</returns>
    abstract static TSelf CreateReadonly(double[] values);

    /// <summary>
    /// Creates a new instance of the <see cref="IIntrinsicVector{TSelf}"/> class
    /// with the specified value in all elements and makes it read-only.
    /// </summary>
    /// <param name="length">The length.</param>
    /// <param name="value">The value.</param>
    /// <returns>A new read-only instance of the <see cref="IIntrinsicVector{TSelf}"/> class with the <paramref name="value"/>.</returns>
    abstract static TSelf CreateReadonly(int length, double value);

    /// <summary>
    /// Gets the capacity.
    /// </summary>
    /// <returns>The capacity.</returns>
    abstract static int GetCapacity();

    /// <summary>
    /// Gets a value indicating whether the current hardware supports AVX.
    /// </summary>
    /// <returns><see langword="true"/> if the current hardware supports AVX; otherwise, <see langword="false"/>.</returns>
    abstract static bool CheckSupported();

    /// <summary>
    /// Adds two vectors and stores the result in the specified vector.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <param name="result">The result vector.</param>
    abstract static void Add(TSelf left, TSelf right, TSelf result);

    /// <summary>
    /// Adds a vector and a scalar and stores the result in the specified vector.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right scalar.</param>
    /// <param name="result">The result vector.</param>
    abstract static void Add(TSelf left, double right, TSelf result);

    /// <summary>
    /// Adds a vector and a scalar and stores the result in the specified vector.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right scalar.</param>
    /// <param name="result">The result vector.</param>
    abstract static void Subtract(TSelf left, TSelf right, TSelf result);

    /// <summary>
    /// Subtracts a scalar from a vector and stores the result in the specified vector.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right scalar.</param>
    /// <param name="result">The result vector.</param>
    abstract static void Subtract(TSelf left, double right, TSelf result);

    /// <summary>
    /// Subtracts a vector from a scalar and stores the result in the specified vector.
    /// </summary>
    /// <param name="left">The left scaler.</param>
    /// <param name="right">The right vector.</param>
    /// <param name="result">The result vector.</param>
    abstract static void Subtract(double left, TSelf right, TSelf result);

    /// <summary>
    /// Multiplies two vectors and stores the result in the specified vector.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <param name="result">The result vector.</param>
    abstract static void Multiply(TSelf left, TSelf right, TSelf result);

    /// <summary>
    /// Multiplies a vector and a scalar and stores the result in the specified vector.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right scalar.</param>
    /// <param name="result">The result vector.</param>
    abstract static void Multiply(TSelf left, double right, TSelf result);

    /// <summary>
    /// Divides two vectors and stores the result in the specified vector.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <param name="result">The result vector.</param>
    abstract static void Divide(TSelf left, TSelf right, TSelf result);

    /// <summary>
    /// Divides a vector by a scalar and stores the result in the specified vector.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right scalar.</param>
    /// <param name="result">The result vector.</param>
    abstract static void Divide(TSelf left, double right, TSelf result);

    /// <summary>
    /// Divides a scalar by a vector and stores the result in the specified vector.
    /// </summary>
    /// <param name="left">The left scaler.</param>
    /// <param name="right">The right vector.</param>
    /// <param name="result">The result vector.</param>
    abstract static void Divide(double left, TSelf right, TSelf result);

    /// <summary>
    /// Computes the inner product of two vectors.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>The inner product of the two vectors.</returns>
    abstract static double InnerProduct(TSelf left, TSelf right);

    /// <summary>
    /// Computes the exponential of the specified vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="result">The result vector.</param>
    abstract static void Exp(TSelf vector, TSelf result);
} // public interface IIntrinsicVector<TSelf>
