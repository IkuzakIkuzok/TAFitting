
// (c) 2024 Kazuki KOHZUKI

using System.Runtime.InteropServices;

namespace TAFitting.Model.Polynomial;

/// <summary>
/// Represents a 3rd-order polynomial model.
/// </summary>
[Guid("88F0E853-73C6-450B-B5BE-F37B26196D37")]
internal sealed class Polynomial3 : IFittingModel, IAnalyticallyDifferentiable
{
    private static readonly Parameter[] parameters = [
        new() { Name = "A0", InitialValue = +1e+3, IsMagnitude = true },
        new() { Name = "A1", InitialValue = -1e+2, IsMagnitude = true },
        new() { Name = "A2", InitialValue = +1e+0, IsMagnitude = true },
        new() { Name = "A2", InitialValue = -1e-2, IsMagnitude = true },
    ];

    /// <inheritdoc/>
    public string Name => "Poly3";

    /// <inheritdoc/>
    public string Description => "3rd-order polynomial model";

    /// <inheritdoc/>
    public string ExcelFormula => "[A0] + [A1] * $X + [A2] * $X^2 + [A3] * $X^3";

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

        return (x) =>
        {
            var x2 = x * x;
            var x3 = x2 * x;
            return a0 + a1 * x + a2 * x2 + a3 * x3;
        };
    } // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)

    /// <inheritdoc/>
    public double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x)
    {
        var d_a0 = 1.0;
        var d_a1 = x;
        var d_a2 = x * x;
        var d_a3 = d_a2 * x;
        return [d_a0, d_a1, d_a2, d_a3];
    } // public double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x)
} // internal sealed class Polynomial3 : IFittingModel, IAnalyticallyDifferentiable
