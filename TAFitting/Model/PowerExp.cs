
// (c) 2024 Kazuki KOHZUKI

using System.Runtime.InteropServices;

namespace TAFitting.Model;

[Guid("25345F16-17DD-41F5-AC79-2E35B99D811D")]
internal sealed class PowerExp : IFittingModel
{
    private static readonly Parameter[] parameters = [
        new Parameter { Name = "A0", InitialValue = 1e3, IsMagnitude = true },
        new Parameter {Name = "a", Constraints = ParameterConstraints.Positive, InitialValue = 1.0 },
        new Parameter {Name = "Alpha", Constraints = ParameterConstraints.Positive, InitialValue = 0.4 },
        new() { Name = "AT", InitialValue = 1e3, IsMagnitude = true },
        new() { Name = "τT", Constraints = ParameterConstraints.Positive, InitialValue = 5.0 },
    ];

    /// <inheritdoc/>
    public string Name => "Power-law + Exp";

    /// <inheritdoc/>
    public string Description => "Power-law + exponential model";

    /// <inheritdoc/>
    public string ExcelFormula => "[A0] / ((1 + [a] * $X) ^ [Alpha]) + [AT] * EXP(-$X / [τT])";

    /// <inheritdoc/>
    public IReadOnlyList<Parameter> Parameters => parameters;

    /// <inheritdoc/>
    public bool XLogScale => true;

    /// <inheritdoc/>
    public bool YLogScale => true;

    /// <inheritdoc/>
    public Func<double, double> GetFunction(IReadOnlyList<double> parameters)
    {
        var a0 = parameters[0];
        var a = parameters[1];
        var alpha = parameters[2];
        var at = parameters[3];
        var tauT = parameters[4];
        return x => a0 / Math.Pow(1 + a * x, alpha) + at * Math.Exp(-x / tauT);
    } // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)
} // internal sealed class PowerExp : IFittingModel
