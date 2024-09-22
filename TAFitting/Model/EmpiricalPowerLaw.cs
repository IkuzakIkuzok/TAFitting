
// (c) 2024 Kazuki KOHZUKI

using System.Runtime.InteropServices;

namespace TAFitting.Model;

[Guid("84FCCACE-3DDF-42F5-92BF-7C6BE37B45C7")]
internal sealed class EmpiricalPowerLaw : IFittingModel
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

    public Func<double, double> GetFunction(IList<double> parameters)
    {
        var a0 = parameters[0];
        var a = parameters[1];
        var alpha = parameters[2];
        return x => a0 / Math.Pow(1 + a * x, alpha);
    } // public Func<double, double> GetFunction (IList<double> parameters)
} // internal sealed class EmpiricalPowerLaw : IFittingModel
