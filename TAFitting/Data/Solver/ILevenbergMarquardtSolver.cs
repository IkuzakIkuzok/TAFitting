
// (c) 2025 Kazuki Kohzuki

using Numbers = System.Collections.Generic.IReadOnlyList<double>;

namespace TAFitting.Data.Solver;

/// <summary>
/// Represents a Levenberg-Marquardt solver.
/// </summary>
internal interface ILevenbergMarquardtSolver
{
    /// <summary>
    /// Gets the x values.
    /// </summary>
    Numbers X { get; }

    /// <summary>
    /// Gets the y values.
    /// </summary>
    Numbers Y { get; }

    /// <summary>
    /// Gets the parameters.
    /// </summary>
    Numbers Parameters { get; }

    /// <summary>
    /// Gets or sets the damping factor.
    /// </summary>
    double Lambda { get; }

    /// <summary>
    /// Initializes the solver with the specified data.
    /// </summary>
    /// <param name="y">The y values.</param>
    /// <param name="parameters">The initial parameters.</param>
    void Initialize(Numbers y, Numbers parameters);

    /// <summary>
    /// Fits the model to the data.
    /// </summary>
    void Fit();
} // internal interface ILevenbergMarquardtSolver
