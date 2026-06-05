
// (c) 2024-2026 Kazuki Kohzuki

using TAFitting.Data;

namespace TAFitting.Model;

/// <summary>
/// Represents a fitting model that can be vectorized.
/// </summary>
public interface IVectorizedModel : IFittingModel
{
    /*/// <summary>
    /// Gets the vectorized function of the model with the specified parameters.
    /// </summary>
    /// <param name="parameters">The parameters of the model.</param>
    /// <returns>A vectorized function of the model with the specified <paramref name="parameters"/>.</returns>
    public Action<AvxVector, AvxVector> GetVectorizedFunc(IReadOnlyList<double> parameters);*/

    /// <summary>
    /// Calculates the vectorized function of the model with the specified parameters and input vector, and stores the result in the output vector.
    /// </summary>
    /// <param name="parameters">The parameters of the model.</param>
    /// <param name="x">The input vector.</param>
    /// <param name="result">The output vector.</param>
    void CalculateFunc(IReadOnlyList<double> parameters, AvxVector x, AvxVector result);

    /// <summary>
    /// Calculates the vectorized derivatives of the model with the specified parameters and input vector, and stores the results in the output vectors.
    /// </summary>
    /// <param name="parameters">The parameters of the model.</param>
    /// <param name="x">The input vector.</param>
    /// <param name="results">The output vectors.</param>
    void CalculateDerivatives(IReadOnlyList<double> parameters, AvxVector x, AvxVector[] results);
} // public interface IVectorizedModel<TVector> : IFittingModel
