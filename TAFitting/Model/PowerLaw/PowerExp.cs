
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Model.PowerLaw;

[Guid("25345F16-17DD-41F5-AC79-2E35B99D811D")]
internal sealed class PowerExp : IFittingModel, IAnalyticallyDifferentiable
{
    private static readonly Parameter[] parameters = [
        new() { Name = "A0"   , InitialValue = 1e3, IsMagnitude = true },
        new() { Name = "a"    , InitialValue = 1.0, Constraints = ParameterConstraints.Positive },
        new() { Name = "Alpha", InitialValue = 0.4, Constraints = ParameterConstraints.Positive },
        new() { Name = "AT"   , InitialValue = 1e3, IsMagnitude = true },
        new() { Name = "τT"   , InitialValue = 5.0, Constraints = ParameterConstraints.Positive },
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
    } // public Func<double, double> GetFunction (IReadOnlyList<double>)

    /// <inheritdoc/>
    public Action<double, double[]> GetDerivatives(IReadOnlyList<double> parameters)
    {
        var a0 = parameters[0];
        var a = parameters[1];
        var alpha = parameters[2];
        var at = parameters[3];
        var tauT = parameters[4];

        return (x, res) =>
        {
            var ax = a * x;
            var pow = Math.Pow(1 + ax, -alpha);
            var exp = Math.Exp(-x / tauT);

            var d_a0 = 1 / pow;
            var d_a = -a0 * x * Math.Pow(1 + ax, -1 - alpha) * alpha;
            var d_alpha = -a0 * Math.Log(1 + ax) * pow;
            var d_at = exp;
            var d_tauT = at * x * exp / (tauT * tauT);

            res[0] = d_a0;
            res[1] = d_a;
            res[2] = d_alpha;
            res[3] = d_at;
            res[4] = d_tauT;
        };
    } // public Action<double, double[]> GetDerivatives (IReadOnlyList<double>)
} // internal sealed class PowerExp : IFittingModel, IAnalyticallyDifferentiable
