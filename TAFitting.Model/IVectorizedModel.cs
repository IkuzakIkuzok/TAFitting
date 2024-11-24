﻿
// (c) 2024 Kazuki Kohzuki

using TAFitting.Data;

namespace TAFitting.Model;

/// <summary>
/// Represents a fitting model that can be vectorized.
/// </summary>
/// <typeparam name="TVector">The type of the intrinsic vector.</typeparam>
public interface IVectorizedModel<TVector> : IFittingModel where TVector : IIntrinsicVector<TVector>
{
    /// <summary>
    /// Gets the vectorized function of the model with the specified parameters.
    /// </summary>
    /// <param name="parameters">The parameters of the model.</param>
    /// <returns>A vectorized function of the model with the specified <paramref name="parameters"/>.</returns>
    public Func<TVector, TVector> GetVectorizedFunc(IReadOnlyList<double> parameters);

    /// <summary>
    /// Gets the vectorized derivatives of the model with respect to the parameters.
    /// </summary>
    /// <param name="parameters">The parameters of the model.</param>
    /// <returns>The vectorized derivatives of the model with respect to the parameters.</returns>
    public Action<TVector, TVector[]> GetVectorizedDerivatives(IReadOnlyList<double> parameters);
} // public interface IVectorizedModel<TVector> : IFittingModel where TVector : IIntrinsicVector<TVector>