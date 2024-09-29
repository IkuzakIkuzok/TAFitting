
// (c) 2024 Kazuki KOHZUKI

using System.Runtime.InteropServices;

namespace TAFitting.Model;

[Guid("5C8EAF4E-C682-4524-BE0B-B0A1970E461B")]
internal sealed class Exponential1Component : IFittingModel, IAnalyticallyDifferentiable
{
    private static readonly Parameter[] parameters = [
        new() { Name = "A0", IsMagnitude = true },
        new() { Name = "A1", InitialValue = 1e3, IsMagnitude = true },
        new() { Name = "T1", Constraints = ParameterConstraints.Positive, InitialValue = 5.0 },
    ];

    /// <inheritdoc/>
    public string Name => "Exp1";

    /// <inheritdoc/>
    public string Description => "1-Component exponential model";

    /// <inheritdoc/>
    public string ExcelFormula => "[A0] + [A1] * EXP(-$X / [T1])";

    /// <inheritdoc/>
    public IReadOnlyList<Parameter> Parameters => parameters;

    /// <inheritdoc/>
    public bool XLogScale => false;

    /// <inheritdoc/>
    public bool YLogScale => true;

    public Func<double, double> GetFunction(IReadOnlyList<double> parameters)
    {
        var a0 = parameters[0];
        var a1 = parameters[1];
        var t1 = parameters[2];
        return x => a0 + a1 * Math.Exp(-x / t1);
    } // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)

    /// <inheritdoc/>
    public double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x)
    {
        var a1 = parameters[1];
        var t1 = parameters[2];

        var exp = Math.Exp(-x / t1);

        var d_a0 = 1.0;
        var d_a1 = exp;
        var d_t1 = a1 * x * exp / (t1 * t1);
        return [d_a0, d_a1, d_t1];
    } // public double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x)
} // internal sealed class Exponential1Component, IAnalyticallyDifferentiable
