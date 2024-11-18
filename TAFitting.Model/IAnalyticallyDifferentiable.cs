
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Model;

/// <summary>
/// Interface for fitting models that can compute the analytical differentials.
/// </summary>
public interface IAnalyticallyDifferentiable : IFittingModel
{
    /*
    /// <summary>
    /// Compute the differentials of the model function with respect to the parameters.
    /// </summary>
    /// <param name="parameters">The parameters of the model.</param>
    /// <param name="x">The independent variable.</param>
    /// <returns>The differentials of the model function with respect to the parameters.</returns>
    public double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x);*/

    /// <summary>
    /// Gets the derivative functions of the model with respect to the parameters.
    /// </summary>
    /// <param name="parameters">The parameters of the model.</param>
    /// <returns>The derivative functions of the model with respect to the parameters.</returns>
    public Action<double, double[]> GetDerivatives(IReadOnlyList<double> parameters);
} // public interface IAnalyticallyDifferentiable : IFittingModel
