
// (c) 2024 Kazuki KOHZUKI

using System.Runtime.InteropServices;

namespace TAFitting.Model;

/// <summary>
/// Represents a 2nd-order polynomial model.
/// </summary>
[Guid("99C057E2-D53E-4110-9610-FEF403D75527")]
internal sealed class Polynomial2 : IFittingModel, IAnalyticallyDifferentiable
{
    private static readonly Parameter[] parameters = [
        new() { Name = "A0", InitialValue = +1e3, IsMagnitude = true },
        new() { Name = "A1", InitialValue = -1e2, IsMagnitude = true },
        new() { Name = "A2", InitialValue = +1e0, IsMagnitude = true },
    ];

    /// <inheritdoc/>
    public string Name => "Poly2";

    /// <inheritdoc/>
    public string Description => "2nd-order polynomial model";

    /// <inheritdoc/>
    public string ExcelFormula => "[A0] + [A1] * $X + [A2] * $X^2";

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
        return x => a0 + a1 * x + a2 * x * x;
    } // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)

    /// <inheritdoc/>
    public double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x)
    {
        var d_a0 = 1.0;
        var d_a1 = x;
        var d_a2 = x * x;
        return [d_a0, d_a1, d_a2];
    } // public double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x)
} // internal sealed class Polynomial2 : IFittingModel, IAnalyticallyDifferentiable
