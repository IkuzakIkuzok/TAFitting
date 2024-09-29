
// (c) 2024 Kazuki KOHZUKI

using TAFitting.Model;
using Numbers = System.Collections.Generic.IReadOnlyList<double>;

namespace TAFitting.Data.Solver;

/// <summary>
/// Represents the Levenberg-Marquardt solver.
/// </summary>
internal sealed class LevenbergMarquardt
{
    /// <summary>
    /// Gets the fitting model.
    /// </summary>
    internal IFittingModel Model { get; }

    /// <summary>
    /// Gets the x values.
    /// </summary>
    internal Numbers X => this.x;

    /// <summary>
    /// Gets the y values.
    /// </summary>
    internal Numbers Y => this.y;

    /// <summary>
    /// Gets the parameters.
    /// </summary>
    internal Numbers Parameters => this.parameters;

    /// <summary>
    /// Gets the lambda (damping parameter).
    /// </summary>
    internal double Lambda { get; private set; } = 0.001;

    /// <summary>
    /// Gets the maximum iteration count.
    /// </summary>
    internal int MaxIteration { get; init; } = 100;

    /// <summary>
    /// Gets the minimum delta chi-squared.
    /// </summary>
    internal double MinimumDeltaChi2 { get; init; } = 1e-30;

    private IEnumerable<double> Diffs
    {
        get
        {
            var func = this.Model.GetFunction(this.parameters);
            return this.x.Zip(this.y, (xi, yi) => yi - func(xi));
        }
    }
    private readonly Numbers x, y;
    private readonly double[] parameters;
    private readonly double[] incrementedParameters;
    private readonly ParameterConstraints[] constraints;

    private readonly int numberOfParameters, numberOfDataPoints;
    private readonly double[,] alpha;
    private readonly double[] beta;
    private readonly double[] temp;
    private readonly double[,] derivatives;

    /// <summary>
    /// Initializes a new instance of the <see cref="LevenbergMarquardt"/> class.
    /// </summary>
    /// <param name="model">The fitting model.</param>
    /// <param name="x">The x values.</param>
    /// <param name="y">The y values.</param>
    /// <exception cref="ArgumentException">The number of <paramref name="x"/> and <paramref name="y"/> values must be the same.</exception>
    internal LevenbergMarquardt(IFittingModel model, Numbers x, Numbers y)
    {
        if (x.Count != y.Count)
            throw new ArgumentException("The number of x and y values must be the same.");

        this.Model = model;
        this.x = x;
        this.y = y;

        this.parameters = model.Parameters.Select(p => p.InitialValue).ToArray();
        this.constraints = model.Parameters.Select(p => p.Constraints).ToArray();
        
        this.numberOfParameters = this.Parameters.Count;
        this.numberOfDataPoints = this.x.Count;
        this.incrementedParameters = new double[this.numberOfParameters];
        this.alpha = new double[this.numberOfParameters, this.numberOfParameters];
        this.beta = new double[this.numberOfParameters];
        this.temp = new double[this.numberOfParameters];
        this.derivatives = new double[this.numberOfDataPoints, this.numberOfParameters];
    } // ctor (IFittingModel)

    /// <summary>
    /// Initializes a new instance of the <see cref="LevenbergMarquardt"/> class
    /// with the specified initial parameters.
    /// </summary>
    /// <param name="model">The fitting model.</param>
    /// <param name="x">The x values.</param>
    /// <param name="y">The y values.</param>
    /// <param name="parameters">The initial parameters.</param>
    /// <exception cref="ArgumentException">The number of <paramref name="x"/> and <paramref name="y"/> values must be the same.</exception>
    internal LevenbergMarquardt(IFittingModel model, Numbers x, Numbers y, Numbers parameters) : this(model, x, y)
    {
        Array.Copy(parameters.ToArray(), 0, this.parameters, 0, this.numberOfParameters);
    } // ctor (IFittingModel, Numbers, Numbers, Numbers)

    /// <summary>
    /// Fits the model to the data.
    /// </summary>
    internal void Fit()
    {
        var iterCount = 0;

        double chi2, incrementedChi2;
        do
        {
            chi2 = CalcChi2();
            CalcDerivativesCache();
            CalcAlpha();
            CalcBeta();

            SolveIncrements();

            incrementedChi2 = CalcIncrementedChi2();

            CheckConstraints();
            if (double.IsNaN(incrementedChi2)) break;
            if (incrementedChi2 >= chi2)
            {
                this.Lambda *= 10;
            }
            else
            {
                this.Lambda /= 10;
                UpdateParameters();
            }

            ++iterCount;
        } while (!Stop(iterCount, chi2, incrementedChi2));
    } // internal void Fit ()

    private double CalcChi2(Numbers parameters)
    {
        var func = this.Model.GetFunction(parameters);
        return this.x.Zip(this.y, (xi, yi) => yi - func(xi)).Select(diff => diff * diff).Sum();
    } // private double CalcChi2 (Numbers)

    private double CalcChi2()
        => CalcChi2(this.parameters);

    private double CalcIncrementedChi2()
        => CalcChi2(this.incrementedParameters);

    private void SolveIncrements()
    {
        /*
         * We need Δ=α⁻¹β for the increments.
         * However, calculating the inverse of a matrix is computationally expensive.
         * Therefore, we solve the linear equation αΔ=β instead.
         * `alpha` and `beta` are overwritten, but they are not needed anymore within the iteration.
         */
        
        // sweeping-out method
        for (var row = 0; row < this.numberOfParameters; ++row)
        {
            var pivot = this.alpha[row, row];
            if (pivot == 0)
            {
                this.beta[row] = 0;
            }
            else
            {
                for (var otherRow = 0; otherRow < this.numberOfParameters; ++otherRow)
                {
                    if (row == otherRow) continue;
                    var ratio = this.alpha[otherRow, row] / pivot;
                    for (var col = 0; col < this.numberOfParameters; ++col)
                        this.alpha[otherRow, col] -= ratio * this.alpha[row, col];
                    this.beta[otherRow] -= ratio * this.beta[row];
                }
                for (var col = 0; col < this.numberOfParameters; ++col)
                    this.alpha[row, col] /= pivot;
                this.beta[row] /= pivot;
            }
        }

        for (var i = 0; i < this.numberOfParameters; ++i)
            this.incrementedParameters[i] = this.parameters[i] + this.beta[i];
    } // private void SolveIncrements ()

    private void CalcAlpha()
    {
        for (var i = 0; i < this.numberOfParameters; ++i)
            for (var j = 0; j < this.numberOfParameters; ++j)
                this.alpha[i, j] = CalcAlphaElement(i, j);
    } // private void CalcAlpha ()
    
    private double CalcAlphaElement(int row, int col)
    {
        var res = this.x.Select((_, i) => this.derivatives[i, row] * this.derivatives[i, col]).Sum();
        if (row == col) res *= 1 + this.Lambda;
        return res;
    } // private double CalcAlphaElement (int, int)

    private void CalcBeta()
    {
        for (var i = 0; i < this.numberOfParameters; ++i)
            this.beta[i] = CalcBetaElement(i);
    } // private void CalcBeta ()

    private double CalcBetaElement(int row)
        => this.Diffs.Select((diff, i) => diff * this.derivatives[i, row]).Sum();

    private void CalcDerivativesCache()
    {
        for (var i = 0; i < this.numberOfDataPoints; ++i)
            for (var j = 0; j < this.numberOfParameters; ++j)
                this.derivatives[i, j] = CalcPartialDerivative(this.x[i], j);
    } // private void CalcDerivativesCache ()

    private double CalcPartialDerivative(double x, int row)
    {
        var EPS = 1e-10;
        var last_diff = 0.0;
        var diff = double.PositiveInfinity;
        var err = double.PositiveInfinity;
        var last_err = double.PositiveInfinity;
        var eps = 1.0;
        var step = 1.1;

        var y0 = this.Model.GetFunction(this.parameters)(x);
        while (Math.Abs(last_diff - diff) > EPS)
        {
            if (eps <= EPS) break;

            Array.Copy(this.parameters, 0, this.temp, 0, this.numberOfParameters);
            this.temp[row] += eps;
            var d = this.Model.GetFunction(this.temp)(x) - y0;
            if (d == 0 || !double.IsFinite(d / eps)) break;
            err = Math.Abs((last_diff - diff) / (step - 1));
            if (err > last_err) break;
            last_err = err;
            last_diff = diff;
            diff = d / eps;
            eps /= step;
        }
        return last_diff;

        /*var delta = this.parameters[row] * 1e-4;
        Array.Copy(this.parameters, 0, this.temp, 0, this.numberOfParameters);
        this.temp[row] += delta;
        var yPlus = this.Model.GetFunction(this.temp)(x);

        this.temp[row] -= 2 * delta;
        var yMinus = this.Model.GetFunction(this.temp)(x);

        return (yPlus - yMinus) / (2 * delta);*/
    } // private double CalcPartialDerivative (Func<double, double>, int)

    private void UpdateParameters()
        => Array.Copy(this.incrementedParameters, 0, this.parameters, 0, this.numberOfParameters);

    private void CheckConstraints()
    {
        for (var i = 0; i < this.numberOfParameters; ++i)
        {
            if (this.constraints[i] == ParameterConstraints.Positive && this.parameters[i] < 0)
                this.parameters[i] = 0;
            if (this.constraints[i] == ParameterConstraints.NonNegative && this.parameters[i] < 0)
                this.parameters[i] = 0;
            if (this.constraints[i] == ParameterConstraints.Integer)
                this.parameters[i] = Math.Round(this.parameters[i]);
        }
    } // private void CheckConstraints ()

    private bool Stop(int iterCount, double chi2, double incrementedChi2)
    {
        if (iterCount > this.MaxIteration) return true;
        return Math.Abs(chi2 - incrementedChi2) < this.MinimumDeltaChi2;
    } // private bool Stop (int, double, double)
} // internal sealed class LevenbergMarquardt
