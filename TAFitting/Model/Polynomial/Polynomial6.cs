
// (c) 2024 Kazuki KOHZUKI

using System.Runtime.InteropServices;

namespace TAFitting.Model.Polynomial;

/// <summary>
/// Represents a 6th-order polynomial model.
/// </summary>
[Guid("82F9F05F-91FE-46B4-9CC2-3F792DF5DB83")]
internal sealed class Polynomial6 : IFittingModel, IAnalyticallyDifferentiable
{
    private static readonly Parameter[] parameters = [
        new() { Name = "A0", InitialValue = +1e+3, IsMagnitude = true },
        new() { Name = "A1", InitialValue = -1e-2, IsMagnitude = true },
        new() { Name = "A2", InitialValue = +1e+1, IsMagnitude = true },
        new() { Name = "A3", InitialValue = -1e+0, IsMagnitude = true },
        new() { Name = "A4", InitialValue = +1e-2, IsMagnitude = true },
        new() { Name = "A5", InitialValue = -1e-4, IsMagnitude = true },
        new() { Name = "A6", InitialValue = +1e-6, IsMagnitude = true },
    ];

    /// <inheritdoc/>
    public string Name => "Poly6";

    /// <inheritdoc/>
    public string Description => "6th-order polynomial model";

    /// <inheritdoc/>
    public string ExcelFormula => "[A0] + [A1] * $X + [A2] * $X^2 + [A3] * $X^3 + [A4] * $X^4 + [A5] * $X^5 + [A6] * $X^6";

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

        return (x) =>
        {
            var x2 = x * x;
            var x3 = x2 * x;
            var x4 = x3 * x;
            var x5 = x4 * x;
            var x6 = x5 * x;
            return a0 + a1 * x + a2 * x2 + a3 * x3 + a4 * x4 + a5 * x5 + a6 * x6;
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
        return [d_a0, d_a1, d_a2, d_a3, d_a4, d_a5, d_a6];
    } // public double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x)
} // internal sealed class Polynomial6 : IFittingModel, IAnalyticallyDifferentiable
