
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Model;

/// <summary>
/// Represents a linear combination of models.
/// </summary>
internal sealed class LinearCombinationModel : IFittingModel, IAnalyticallyDifferentiable
{
    private readonly List<IAnalyticallyDifferentiable> models;
    private readonly List<Parameter> parameters = [];

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public string Description { get; }

    /// <inheritdoc/>
    public string ExcelFormula { get; }

    /// <inheritdoc/>
    public IReadOnlyList<Parameter> Parameters => this.parameters;

    /// <inheritdoc/>
    public bool XLogScale { get; }

    /// <inheritdoc/>
    public bool YLogScale { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinearCombinationModel"/> class.
    /// </summary>
    /// <param name="name">The name of the model.</param>
    /// <param name="models">The list of models to combine.</param>
    internal LinearCombinationModel(string name, IReadOnlyList<Guid> models)
    {
        if (models.Count < 2)
            throw new ArgumentException("More than 2 models are required.", nameof(models));
        this.models = models.Select(id => ModelManager.Models[id].Model).OfType<IAnalyticallyDifferentiable>().ToList();

        this.Name = name;
        this.Description = "Linear combination of " + string.Join(", ", this.models.SkipLast(1).Select(m => m.Name)) + " and " + this.models[models.Count - 1].Name;
        this.XLogScale = this.models.Any(m => m.XLogScale);
        this.YLogScale = this.models.Any(m => m.YLogScale);

        var formula = new string[this.models.Count];
        foreach ((var i, var model) in this.models.Enumerate())
        {
            var f = model.ExcelFormula;
            var parameters = model.Parameters;
            foreach (var parameter in parameters)
            {
                var oldName = parameter.Name;
                var newName = $"({i + 1}){oldName}";
                f = f.Replace($"[{oldName}]", $"[{newName}]");

                var p = new Parameter()
                {
                    Name = newName,
                    Constraints = parameter.Constraints,
                    InitialValue = parameter.InitialValue,
                    IsMagnitude = parameter.IsMagnitude,
                };
                this.parameters.Add(p);
            }
            formula[i] = $"({f})";
        }
        this.ExcelFormula = string.Join(" + ", formula);
    } // ctor (string, IReadOnlyList<Guid>)

    /// <inheritdoc/>
    public Func<double, double> GetFunction(IReadOnlyList<double> parameters)
    {
        return (x) =>
        {
            var ret = 0.0;
            var offset = 0;
            foreach (var model in this.models)
            {
                var n = model.Parameters.Count;
                var p = parameters.Skip(offset).Take(n).ToArray();
                ret += model.GetFunction(p)(x);
                offset += n;
            }
            return ret;
        };
    } // public Func<double, double> GetFunction (IReadOnlyList<double>)

    /// <inheritdoc/>
    public Action<double, double[]> GetDerivatives(IReadOnlyList<double> parameters)
    {
        var derivs = new Action<double, double[]>[this.models.Count];
        foreach ((var i, var model) in this.models.Enumerate())
        {
            var n = model.Parameters.Count;
            var p = parameters.Take(n).ToArray();
            derivs[i] = model.GetDerivatives(p);
            parameters = parameters.Skip(n).ToArray();
        }
        return (x, res) =>
        {
            var ret = new double[this.parameters.Count];
            var offset = 0;
            foreach ((var i, var model) in this.models.Enumerate())
            {
                var arr = new double[model.Parameters.Count];
                derivs[i](x, arr);
                var n = arr.Length;
                Array.Copy(arr, 0, ret, offset, n);
                offset += n;
            }
        };
    } // public Action<double, double[]> GetDerivatives (IReadOnlyList<double>)
} // internal sealed class LinearCombinationModel : IFittingModel, IAnalyticallyDifferentiable
