﻿
// (c) 2024 Kazuki KOHZUKI

using System.Runtime.InteropServices;

namespace TAFitting.Model.Exponential;

[Guid("AD397D83-B21F-4861-BFF9-0AEC16FE0AF8")]
internal sealed class Exponential3 : IFittingModel, IAnalyticallyDifferentiable
{
    private static readonly Parameter[] parameters = [
        new() { Name = "A0", IsMagnitude = true },
        new() { Name = "A1", InitialValue = 1e3, IsMagnitude = true },
        new() { Name = "T1", InitialValue = 5e0, Constraints = ParameterConstraints.Positive },
        new() { Name = "A2", InitialValue = 1e2, IsMagnitude = true },
        new() { Name = "T2", InitialValue = 5e1, Constraints = ParameterConstraints.Positive },
        new() { Name = "A3", InitialValue = 1e1, IsMagnitude = true },
        new() { Name = "T3", InitialValue = 5e2, Constraints = ParameterConstraints.Positive },
    ];

    /// <inheritdoc/>
    public string Name => "Exp3";

    /// <inheritdoc/>
    public string Description => "3-Component exponential model";

    /// <inheritdoc/>
    public string ExcelFormula => "[A0] + [A1] * EXP(-$X / [T1]) + [A2] * EXP(-$X / [T2]) + [A3] * EXP(-$X / [T3])";

    /// <inheritdoc/>
    public IReadOnlyList<Parameter> Parameters => parameters;

    /// <inheritdoc/>
    public bool XLogScale => false;

    /// <inheritdoc/>
    public bool YLogScale => true;

    /// <inheritdoc/>
    public Func<double, double> GetFunction(IReadOnlyList<double> parameters)
    {
        var a0 = parameters[0];
        var a1 = parameters[1];
        var t1 = parameters[2];
        var a2 = parameters[3];
        var t2 = parameters[4];
        var a3 = parameters[5];
        var t3 = parameters[6];
        return x => a0 + a1 * Math.Exp(-x / t1) + a2 * Math.Exp(-x / t2) + a3 * Math.Exp(-x / t3);
    } // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)

    /// <inheritdoc/>
    public double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x)
    {
        var a1 = parameters[1];
        var t1 = parameters[2];
        var a2 = parameters[3];
        var t2 = parameters[4];
        var a3 = parameters[5];
        var t3 = parameters[6];

        var exp1 = Math.Exp(-x / t1);
        var exp2 = Math.Exp(-x / t2);
        var exp3 = Math.Exp(-x / t3);

        var d_a0 = 1.0;
        var d_a1 = exp1;
        var d_t1 = a1 * x * exp1 / (t1 * t1);
        var d_a2 = exp2;
        var d_t2 = a2 * x * exp2 / (t2 * t2);
        var d_a3 = exp3;
        var d_t3 = a3 * x * exp3 / (t3 * t3);
        return [d_a0, d_a1, d_t1, d_a2, d_t2, d_a3, d_t3];
    } // public double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x)
} // internal sealed class Exponential3 : IFittingModel, IAnalyticallyDifferentiable