
// (c) 2024 Kazuki KOHZUKI

using TAFitting.Data;

namespace TAFitting.Model.PowerLaw;

[Guid("84FCCACE-3DDF-42F5-92BF-7C6BE37B45C7")]
internal sealed class EmpiricalPowerLaw : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel
{
    private static readonly Parameters parameters = [
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
    public Parameters Parameters => parameters;

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
    } // public Func<double, double> GetFunction (IReadOnlyList<double>)

    /// <inheritdoc/>
    public Action<double, double[]> GetDerivatives(IReadOnlyList<double> parameters)
    {
        var a0 = parameters[0];
        var a = parameters[1];
        var alpha = parameters[2];

        return (x, res) =>
        {
            var ax = a * x;
            var pow = Math.Pow(1 + ax, -alpha);

            var d_a0 = pow;
            var d_a = -a0 * x * Math.Pow(1 + ax, -1 - alpha) * alpha;
            var d_alpha = -a0 * Math.Log(1 + ax) * pow;

            res[0] = d_a0;
            res[1] = d_a;
            res[2] = d_alpha;
        };
    } // public Action<double, double[]> GetDerivatives (IReadOnlyList<double>)

    /// <inheritdoc/>
    Action<AvxVector, AvxVector> IVectorizedModel.GetVectorizedFunc(IReadOnlyList<double> parameters)
        => (x, res) =>
        {
            var a0 = parameters[0];
            var a = parameters[1];
            var alpha = parameters[2];

            AvxVector.Multiply(x, a, res);
            AvxVector.Add(res, 1, res);
            AvxVector.Power(res, alpha, res);
            AvxVector.Divide(a0, res, res);
        };

    /// <inheritdoc/>
    Action<AvxVector, AvxVector[]> IVectorizedModel.GetVectorizedDerivatives(IReadOnlyList<double> parameters)
    {
        var a0 = parameters[0];
        var a = parameters[1];
        var alpha = parameters[2];

        return (x, res) =>
        {
            var temp = new AvxVector(x.Length); 
            AvxVector.Multiply(x, a, temp);
            AvxVector.Add(temp, 1, temp);

            AvxVector.Power(temp, -alpha, res[0]);           // (1 + ax)^-alpha

            AvxVector.Power(temp, -1 - alpha, res[1]);       // (1 + ax)^(-1 - alpha)
            AvxVector.Multiply(res[1], a0 * alpha, res[1]);  // a0 * alpha * (1 + ax)^(-1 - alpha)
            AvxVector.Multiply(res[1], x, res[1]);           // a0 * alpha * x * (1 + ax)^(-1 - alpha)

            AvxVector.Ln(temp, res[2]);                      // ln(1 + ax)
            AvxVector.Multiply(res[2], -a0, res[2]);         // -a0 * ln(1 + ax)
            AvxVector.Multiply(res[2], res[0], res[2]);      // -a0 * ln(1 + ax) / (1 + ax)^alpha
        };
    } // public Action<AvxVector, AvxVector[]> GetVectorizedDerivatives (IReadOnlyList<double>)
} // internal sealed class EmpiricalPowerLaw : IFittingModel, IAnalyticallyDifferentiable
