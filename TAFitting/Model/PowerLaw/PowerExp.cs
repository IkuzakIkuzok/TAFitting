
// (c) 2024 Kazuki KOHZUKI

using System.Runtime.InteropServices;

namespace TAFitting.Model.PowerLaw;

[Guid("25345F16-17DD-41F5-AC79-2E35B99D811D")]
internal sealed class PowerExp : IFittingModel, IAnalyticallyDifferentiable
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

    /// <inheritdoc/>
    public Func<double, double[]> GetDerivatives(IReadOnlyList<double> parameters)
    {
        var a0 = parameters[0];
        var a = parameters[1];
        var alpha = parameters[2];
        var at = parameters[3];
        var tauT = parameters[4];

        return (x) =>
        {
            var ax = a * x;
            var pow = Math.Pow(1 + ax, -alpha);
            var exp = Math.Exp(-x / tauT);

            var d_a0 = 1 / pow;
            var d_a = -a0 * x * Math.Pow(1 + ax, -1 - alpha) * alpha;
            var d_alpha = -a0 * Math.Log(1 + ax) * pow;
            var d_at = exp;
            var d_tauT = at * x * exp / (tauT * tauT);
            return [d_a0, d_a, d_alpha, d_at, d_tauT];
        };
    } // public Func<double, double[]> GetDerivatives (IReadOnlyList<double>)
} // internal sealed class PowerExp : IFittingModel, IAnalyticallyDifferentiable
