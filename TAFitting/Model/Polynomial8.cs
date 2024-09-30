
// (c) 2024 Kazuki KOHZUKI

using System.Runtime.InteropServices;

namespace TAFitting.Model;

/// <summary>
/// Represents an 8th-order polynomial model.
/// </summary>
[Guid("F68E43C2-6417-480C-BEB4-898B7F335757")]
internal sealed class Polynomial8 : IFittingModel, IAnalyticallyDifferentiable
{
    private static readonly Parameter[] parameters = [
        new() { Name = "A0", InitialValue = +1e+03, IsMagnitude = true },
        new() { Name = "A1", InitialValue = -1e+02, IsMagnitude = true },
        new() { Name = "A2", InitialValue = +1e+01, IsMagnitude = true },
        new() { Name = "A3", InitialValue = -1e+00, IsMagnitude = true },
        new() { Name = "A4", InitialValue = +1e-02, IsMagnitude = true },
        new() { Name = "A5", InitialValue = -1e-04, IsMagnitude = true },
        new() { Name = "A6", InitialValue = +1e-06, IsMagnitude = true },
        new() { Name = "A7", InitialValue = -1e-08, IsMagnitude = true },
        new() { Name = "A8", InitialValue = +1e-10, IsMagnitude = true },
    ];

    /// <inheritdoc/>
    public string Name => "Poly8";

    /// <inheritdoc/>
    public string Description => "8th-order polynomial model";

    /// <inheritdoc/>
    public string ExcelFormula => "[A0] + [A1] * $X + [A2] * $X^2 + [A3] * $X^3 + [A4] * $X^4 + [A5] * $X^5 + [A6] * $X^6 + [A7] * $X^7 + [A8] * $X^8";

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
        var a6 = parameters[6];
        var a7 = parameters[7];
        var a8 = parameters[8];

        return (x) =>
        {
            var x2 = x * x;
            var x3 = x2 * x;
            var x4 = x3 * x;
            var x5 = x4 * x;
            var x6 = x5 * x;
            var x7 = x6 * x;
            var x8 = x7 * x;
            return a0 + a1 * x + a2 * x2 + a3 * x3 + a4 * x4 + a5 * x5 + a6 * x6 + a7 * x7 + a8 * x8;
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
        var d_a6 = d_a5 * x;
        var d_a7 = d_a6 * x;
        var d_a8 = d_a7 * x;
        return [d_a0, d_a1, d_a2, d_a3, d_a4, d_a5, d_a6, d_a7, d_a8];
    } // public double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x)
} // internal sealed class Polynomial8 : IFittingModel, IAnalyticallyDifferentiable
