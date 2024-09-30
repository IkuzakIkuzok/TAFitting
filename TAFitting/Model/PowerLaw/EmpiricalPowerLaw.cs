﻿
// (c) 2024 Kazuki KOHZUKI

using System.Runtime.InteropServices;

namespace TAFitting.Model.PowerLaw;

[Guid("84FCCACE-3DDF-42F5-92BF-7C6BE37B45C7")]
internal sealed class EmpiricalPowerLaw : IFittingModel, IAnalyticallyDifferentiable
{
    private static readonly Parameter[] parameters = [
        new Parameter { Name = "A0", InitialValue = 1e3, IsMagnitude = true },
        new Parameter {Name = "a", Constraints = ParameterConstraints.Positive, InitialValue = 1.0 },
        new Parameter {Name = "Alpha", Constraints = ParameterConstraints.Positive, InitialValue = 0.4 },
    ];

    /// <inheritdoc/>
    public string Name => "Empirical Power-Law";

    /// <inheritdoc/>
    public string Description => "Empirical power-law model";

    public string ExcelFormula => "[A0] / ((1 + [a] * $X) ^ [Alpha])";

    /// <inheritdoc/>
    public IReadOnlyList<Parameter> Parameters => parameters;

    /// <inheritdoc/>
    public bool XLogScale => true;

    /// <inheritdoc/>
    public bool YLogScale => true;

    public Func<double, double> GetFunction(IReadOnlyList<double> parameters)
    {
        var a0 = parameters[0];
        var a = parameters[1];
        var alpha = parameters[2];
        return x => a0 / Math.Pow(1 + a * x, alpha);
    } // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)

    /// <inheritdoc/>
    public double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x)
    {
        var a0 = parameters[0];
        var a = parameters[1];
        var alpha = parameters[2];

        var ax = a * x;
        var pow = Math.Pow(1 + ax, -alpha);

        var d_a0 = 1 / pow;
        var d_a = -a0 * x * Math.Pow(1 + ax, -1 - alpha) * alpha;
        var d_alpha = -a0 * Math.Log(1 + ax) * pow;
        return [d_a0, d_a, d_alpha];
    } // public double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x)
} // internal sealed class EmpiricalPowerLaw : IFittingModel, IAnalyticallyDifferentiable