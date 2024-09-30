
// (c) 2024 Kazuki KOHZUKI

using System.Runtime.InteropServices;

namespace TAFitting.Model;

/// <summary>
/// Represents a 5th-order polynomial model.
/// </summary>
[Guid("C7773F3D-44D1-48B0-BF58-746B58433CC0")]
internal sealed class Polynomial5 : IFittingModel, IAnalyticallyDifferentiable
{
    private static readonly Parameter[] parameters = [
        new() { Name = "A0", InitialValue = +1e+3, IsMagnitude = true },
        new() { Name = "A1", InitialValue = -1e+2, IsMagnitude = true },
        new() { Name = "A2", InitialValue = +1e+1, IsMagnitude = true },
        new() { Name = "A3", InitialValue = -1e-1, IsMagnitude = true },
        new() { Name = "A4", InitialValue = +1e-2, IsMagnitude = true },
        new() { Name = "A5", InitialValue = -1e-5, IsMagnitude = true },
    ];

    /// <inheritdoc/>
    public string Name => "Poly5";

    /// <inheritdoc/>
    public string Description => "5th-order polynomial model";

    /// <inheritdoc/>
    public string ExcelFormula => "[A0] + [A1] * $X + [A2] * $X^2 + [A3] * $X^3 + [A4] * $X^4 + [A5] * $X^5";

    /// <inheritdoc/>
    public IReadOnlyList<Parameter> Parameters => parameters;

    /// <inheritdoc/>
    public bool XLogScale => false;

    /// <inheritdoc/>
    public bool YLogScale => false;

    /// <inheritdoc/>
    public Func<double, double> GetFunction(IReadOnlyList<double> parameters)
    {
        var a0 = parameters[0];
        var a1 = parameters[1];
        var a2 = parameters[2];
        var a3 = parameters[3];
        var a4 = parameters[4];
        var a5 = parameters[5];

        return (x) =>
        {
            var x2 = x * x;
            var x3 = x2 * x;
            var x4 = x3 * x;
            var x5 = x4 * x;
            return a0 + a1 * x + a2 * x2 + a3 * x3 + a4 * x4 + a5 * x5;
        };
    } // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)

    /// <inheritdoc/>
    public double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x)
    {
        var d_a0 = 1.0;
        var d_a1 = x;
        var d_a2 = x * x;
        var d_a3 = d_a2 * x;
        var d_a4 = d_a3 * x;
        var d_a5 = d_a4 * x;
        return [d_a0, d_a1, d_a2, d_a3, d_a4, d_a5];
    } // public double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x)
} // internal sealed class Polynomial5 : IFittingModel, IAnalyticallyDifferentiable
