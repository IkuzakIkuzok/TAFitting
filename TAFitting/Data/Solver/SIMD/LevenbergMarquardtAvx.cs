
// (c) 2024 Kazuki Kohzuki

using System.Runtime.Intrinsics;
using TAFitting.Model;
using Numbers = System.Collections.Generic.IReadOnlyList<double>;

namespace TAFitting.Data.Solver.SIMD;

/// <summary>
/// Represents the Levenberg-Marquardt algorithm with AVX.
/// </summary>
internal sealed class LevenbergMarquardtAvx<TVector> where TVector : IAvxVector<TVector>
{
    /// <summary>
    /// Gets a value indicating whether LMA with AVX is supported.
    /// </summary>
    internal static bool IsSupported
        => Program.Config.SolverConfig.UseSIMD && Vector256<double>.IsSupported;

    /// <summary>
    /// Gets the fitting model.
    /// </summary>
    internal IAnalyticallyDifferentiable Model { get; }

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

    /// <summary>
    /// Gets the derivative threshold.
    /// </summary>
    internal double DerivativeThreshold { get; init; } = 1e-4;

    /// <summary>
    /// Gets the parameters.
    /// </summary>
    internal Numbers Parameters => this.parameters;

    private readonly Numbers x;
    private readonly TVector y;
    private readonly double[] parameters;
    private readonly double[] incrementedParameters;
    private readonly ParameterConstraints[] constraints;

    private readonly int numberOfParameters, numberOfDataPoints;
    private TVector est_vals;
    private readonly double[,] hessian;  // Hessian matrix with the damping parameter on the diagonal
    private readonly double[] gradient;
    private readonly double[][] temp_matrix;
    private readonly double[] temp_arr;
    private readonly TVector[] derivatives;  // Cache for the partial derivatives
    private Func<double, double> func = null!;

    internal LevenbergMarquardtAvx(IAnalyticallyDifferentiable model, Numbers x, Numbers y, Numbers parameters)
    {
        if (x.Count != y.Count)
            throw new ArgumentException("The number of x and y values must be the same.");

        this.Model = model;
        this.x = x;
        this.y = TVector.Create([.. y]);

        this.numberOfParameters = parameters.Count;
        this.numberOfDataPoints = x.Count;

        this.parameters = new double[this.numberOfParameters];
        Array.Copy(parameters.ToArray(), 0, this.parameters, 0, this.numberOfParameters);
        this.constraints = model.Parameters.Select(p => p.Constraints).ToArray();

        this.incrementedParameters = new double[this.numberOfParameters];
        this.est_vals = TVector.Create(this.numberOfDataPoints);
        this.hessian = new double[this.numberOfParameters, this.numberOfParameters];
        this.gradient = new double[this.numberOfParameters];

        this.temp_matrix = new double[this.numberOfDataPoints][];
        for (var i = 0; i < this.numberOfDataPoints; ++i)
            this.temp_matrix[i] = new double[this.numberOfParameters];

        this.temp_arr = new double[this.numberOfDataPoints];

        this.derivatives = new TVector[this.numberOfParameters];
        for (var i = 0; i < this.numberOfParameters; ++i)
            this.derivatives[i] = TVector.Create(this.numberOfDataPoints);
    } // ctor (IAnalyticallyDifferentiable, Numbers, Numbers, Numbers)

    /// <summary>
    /// Fits the model to the data.
    /// </summary>
    internal void Fit()
    {
        var iterCount = 0;

        double chi2, incrementedChi2;
        do
        {
            this.func = this.Model.GetFunction(this.parameters);
            var tmp = new double[this.numberOfDataPoints];
            for (var i = 0; i < this.numberOfDataPoints; ++i)
                tmp[i] = this.func(this.x[i]);
            this.est_vals = TVector.Create(tmp);
            chi2 = CalcChi2();
            ComputeDerivatives();
            CalcHessian();
            CalcGradient();

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
        } while (!CheckStop(iterCount, chi2, incrementedChi2));
    } // internal void Fit ()

    private double CalcChi2(Numbers parameters)
    {
        var func = this.Model.GetFunction(parameters);
        for (var i = 0; i < this.numberOfDataPoints; ++i)
            this.temp_arr[i] = func(this.x[i]);
        var v_e = TVector.Create(this.temp_arr);

        var diff = this.y - v_e;
        return (diff * diff).Sum;
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
         * `hessian` and `gradient` are overwritten, but they are not needed anymore within the iteration.
         */

        // Gaussian elimination
        for (var row = 0; row < this.numberOfParameters; ++row)
        {
            var pivot = this.hessian[row, row];
            if (pivot == 0)
            {
                this.gradient[row] = 0;
            }
            else
            {
                for (var otherRow = row + 1; otherRow < this.numberOfParameters; ++otherRow)
                {
                    var ratio = this.hessian[otherRow, row] / pivot;
                    for (var col = 0; col < this.numberOfParameters; ++col)
                        this.hessian[otherRow, col] -= ratio * this.hessian[row, col];
                    this.gradient[otherRow] -= ratio * this.gradient[row];
                }
                for (var col = 0; col < this.numberOfParameters; ++col)
                    this.hessian[row, col] /= pivot;
                this.gradient[row] /= pivot;
            }
        }

        for (var i = this.numberOfParameters - 1; i > 0; --i)
        {
            var b = this.gradient[i];
            for (var j = i - 1; j >= 0; --j)
                this.gradient[j] -= this.hessian[j, i] * b;
        }

        for (var i = 0; i < this.numberOfParameters; ++i)
            this.incrementedParameters[i] = this.parameters[i] + this.gradient[i];
    } // private void SolveIncrements ()

    private void CalcHessian()
    {
        for (var row = 0; row < this.numberOfParameters; ++row)
        {
            for (var col = 0; col < this.numberOfParameters; ++col)
            {
                var h = (this.derivatives[row] * this.derivatives[col]).Sum;
                if (row == col)
                {
                    this.hessian[row, row] = h * (1 + this.Lambda);
                }
                else
                {
                    this.hessian[row, col] = h;
                    this.hessian[col, row] = h;
                }
            }
        }
    } // private void CalcHessian ()

    private void CalcGradient()
    {
        for (var row = 0; row < this.numberOfParameters; ++row)
        {
            var diff = this.y - this.est_vals;
            this.gradient[row] = (diff * this.derivatives[row]).Sum;
        }
    } // private void CalcGradient ()

    private void ComputeDerivatives()
    {
        var d = this.Model.GetDerivatives(this.parameters);
        for (var i = 0; i < this.numberOfDataPoints; ++i)
            Array.Copy(
                d(this.x[i]),
                0,
                this.temp_matrix[i],
                0,
                this.numberOfParameters
            );

        for (var i = 0; i < this.numberOfParameters; ++i)
        {
            var v = new double[this.numberOfDataPoints];
            for (var j = 0; j < this.numberOfDataPoints; ++j)
                v[j] = this.temp_matrix[j][i];
            this.derivatives[i] = TVector.Create(v);
        }
    } // private void ComputeDerivativesCacheAnalytically ()

    private void UpdateParameters()
        => Array.Copy(this.incrementedParameters, 0, this.parameters, 0, this.numberOfParameters);

    private void CheckConstraints()
    {
        for (var i = 0; i < this.numberOfParameters; ++i)
        {
            if (this.constraints[i] == ParameterConstraints.Positive && this.parameters[i] <= 0)
                this.parameters[i] = 1e-10;
            if (this.constraints[i] == ParameterConstraints.NonNegative && this.parameters[i] < 0)
                this.parameters[i] = 0;
            if (this.constraints[i] == ParameterConstraints.Integer)
                this.parameters[i] = Math.Round(this.parameters[i]);
        }
    } // private void CheckConstraints ()

    private bool CheckStop(int iterCount, double chi2, double incrementedChi2)
    {
        if (iterCount > this.MaxIteration) return true;
        return Math.Abs(chi2 - incrementedChi2) < this.MinimumDeltaChi2;
    } // private bool CheckStop (int, double, double)

    /// <summary>
    /// Checks if the number of data points is supported.
    /// </summary>
    /// <param name="dataCount">The number of data points.</param>
    /// <returns><see langword="true"/> if the number of data points is supported; otherwise, <see langword="false"/>.</returns>
    internal static bool CheckSupport(int dataCount)
    {
        if (!IsSupported) return false;
        return dataCount <= (int)(TVector.GetCapacity() * (1 + Program.Config.SolverConfig.MaxTruncateRatio));
    } // internal static bool CheckSupport (int)
} // internal sealed class LevenbergMarquardtAvx<TVector> where TVector : IAvxVector<TVector>
