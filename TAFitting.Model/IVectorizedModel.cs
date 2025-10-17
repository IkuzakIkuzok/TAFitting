
// (c) 2024 Kazuki Kohzuki

using TAFitting.Data;

namespace TAFitting.Model;

/// <summary>
/// Represents a fitting model that can be vectorized.
/// </summary>
public interface IVectorizedModel : IFittingModel
{
    /// <summary>
    /// Gets the vectorized function of the model with the specified parameters.
    /// </summary>
    /// <param name="parameters">The parameters of the model.</param>
    /// <returns>A vectorized function of the model with the specified <paramref name="parameters"/>.</returns>
    public Action<AvxVector, AvxVector> GetVectorizedFunc(IReadOnlyList<double> parameters);

    /// <summary>
    /// Gets the vectorized derivatives of the model with respect to the parameters.
    /// </summary>
    /// <param name="parameters">The parameters of the model.</param>
    /// <returns>The vectorized derivatives of the model with respect to the parameters.</returns>
    /// <remarks>
    /// The 2nd argument of the returned <see cref="Action{AvxVector, AvxVector[]}"/>, which will contain the derivatives at the end of the method,
    /// is not filled with <c>0</c> when it is passed to the method.
    /// Therefore, call <see cref="AvxVector.Load(double)"/> to fill the array with <c>0</c> if necessary.
    /// </remarks>
    public Action<AvxVector, AvxVector[]> GetVectorizedDerivatives(IReadOnlyList<double> parameters);
} // public interface IVectorizedModel<TVector> : IFittingModel
