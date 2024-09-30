
// (c) 2024 Kazuki KOHZUKI

using System.Runtime.InteropServices;

namespace TAFitting.Model.Polynomial;

/// <summary>
/// Represents a 1st-order polynomial model.
/// </summary>
[Guid("9C488607-39F4-48EE-ADDF-E15CB94FF86F")]
internal sealed class Polynomial1 : IFittingModel, IAnalyticallyDifferentiable
{
    private static readonly Parameter[] parameters = [
        new() { Name = "A0", InitialValue = +1e3, IsMagnitude = true },
        new() { Name = "A1", InitialValue = -3e1, IsMagnitude = true },
    ];

    /// <inheritdoc/>
    public string Name => "Poly1";

    /// <inheritdoc/>
    public string Description => "1st-order polynomial model";

    /// <inheritdoc/>
    public string ExcelFormula => "[A0] + [A1] * $X";

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
        return x => a0 + a1 * x;
    } // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)

    /// <inheritdoc/>
    public double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x)
    {
        var d_a0 = 1.0;
        var d_a1 = x;
        return [d_a0, d_a1];
    } // public double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x)
} // internal sealed class Polynomial1 : IFittingModel, IAnalyticallyDifferentiable
