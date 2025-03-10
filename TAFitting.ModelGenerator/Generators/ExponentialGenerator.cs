
// (c) 2024 Kazuki KOHZUKI

using System.Runtime.InteropServices;

namespace TAFitting.ModelGenerator.Generators;

/// <summary>
/// Generates exponential models.
/// </summary>
[Generator(LanguageNames.CSharp)]
internal sealed class ExponentialGenerator : ModelGeneratorBase
{
    override protected string AttributeName => AttributesGenerator.Namespace + "." + AttributesGenerator.ExponentialModelName;

    override protected string FileName => "GeneratedExponentialModels.g.cs";

    override protected string AdditionalCode => @"using TAFitting.Data;";

    override protected string Generate(string nameSpace, string className, int n, string? name)
    {
        var builder = new StringBuilder();

        builder.AppendLine();
        builder.AppendLine($"namespace {nameSpace}");
        builder.AppendLine("{");

        builder.AppendLine($"\t/// <summary>");
        builder.AppendLine($"\t/// Represents a {n}-component exponential model.");
        builder.AppendLine($"\t/// </summary>");
        builder.AppendLine($"\tinternal partial class {className} : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel");
        builder.AppendLine("\t{");

        #region fields

        builder.AppendLine("\t\tprivate static readonly Parameter[] parameters = [");
        builder.AppendLine("\t\t\tnew() { Name = \"A0\", IsMagnitude = true },");
        for (var i = 1; i <= n; i++)
        {
            builder.AppendLine($"\t\t\tnew() {{ Name = \"A{i}\", InitialValue = 1e{3 - i}, IsMagnitude = true }},");
            builder.AppendLine($"\t\t\tnew() {{ Name = \"T{i}\", InitialValue = 5e{i - 1}, Constraints = ParameterConstraints.Positive }},");
        }
        builder.AppendLine("\t\t];");

        #endregion fields

        #region properties

        if (!string.IsNullOrEmpty(name))
        {
            builder.AppendLine();
            builder.AppendLine("\t\t/// <inheritdoc/>");
            builder.AppendLine($"\t\tpublic string Name => \"{name}\";");
        }

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine("\t\tpublic string Description => \"" + n + "-component exponential model\";");

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine(
            "\t\tpublic string ExcelFormula => \"[A0]"
            + string.Concat(Enumerable.Range(1, n).Select(i => $" + [A{i}] * EXP(-$X / [T{i}])")) + "\";"
        );

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine("\t\tpublic IReadOnlyList<Parameter> Parameters => parameters;");

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine("\t\tpublic bool XLogScale => false;");

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine("\t\tpublic bool YLogScale => true;");

        #endregion properties

        #region methods

        #region GetFunction

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine("\t\tpublic Func<double, double> GetFunction(IReadOnlyList<double> parameters)");
        builder.AppendLine("\t\t{");
        builder.AppendLine("\t\t\tvar a0 = parameters[0];");
        for (var i = 1; i <= n; i++)
        {
            builder.AppendLine($"\t\t\tvar a{i} = parameters[{2 * i - 1}];");
            builder.AppendLine($"\t\t\tvar t{i} = -1.0 / parameters[{2 * i}];");
        }
        builder.AppendLine();
        builder.AppendLine("\t\t\treturn x => a0"
            + string.Concat(Enumerable.Range(1, n).Select(i => $" + a{i} * MathUtils.FastExp(x * t{i})")) + ";");
        builder.AppendLine("\t\t} // public Func<double, double> GetFunction(IReadOnlyList<double> parameters)");

        GenerateGetVectorizedFunc(builder, "AvxVector", n);

        #endregion GetFunction

        #region GetDerivatives

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine("\t\tpublic Action<double, double[]> GetDerivatives(IReadOnlyList<double> parameters)");
        builder.AppendLine("\t\t{");
        for (var i = 1; i <= n; i++)
        {
            builder.AppendLine($"\t\t\tvar a{i} = parameters[{2 * i - 1}];");
            builder.AppendLine($"\t\t\tvar t{i} = -1.0 / parameters[{2 * i}];");
        }
        builder.AppendLine();

        builder.AppendLine("\t\t\treturn (x, res) =>");
        builder.AppendLine("\t\t\t{");
        for (var i = 1; i <= n; i++)
        {
            builder.AppendLine($"\t\t\t\tvar exp{i} = MathUtils.FastExp(x * t{i});");
        }
        builder.AppendLine();
        builder.AppendLine("\t\t\t\tvar d_a0 = 1.0;");
        for (var i = 1; i <= n; i++)
        {
            builder.AppendLine($"\t\t\t\tvar d_a{i} = exp{i};");
            builder.AppendLine($"\t\t\t\tvar d_t{i} = a{i} * x * exp{i} * (t{i} * t{i});");
        }
        builder.AppendLine();
        builder.AppendLine("\t\t\t\tres[0] = d_a0;");
        builder.Append(string.Join("\n", Enumerable.Range(1, n).Select(i => $"\t\t\t\tres[{2 * i - 1}] = d_a{i};\n\t\t\t\tres[{2 * i}] = d_t{i};")));
        builder.AppendLine("\n\t\t\t};");
        builder.AppendLine("\t\t} // public Action<double, double[]> GetDerivatives (IReadOnlyList<double>)");

        GenerateGetVectorizedDerivatives(builder, "AvxVector", n);

        #endregion GetDerivatives

        #endregion methods

        builder.AppendLine($"\t}} // internal partial class {className} : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel");
        builder.AppendLine("} // namespace " + nameSpace);

        return builder.ToString();
    } // override protected string Generate (string, string, int, string?)

    private static void GenerateGetVectorizedFunc(StringBuilder builder, string TVector, int n)
    {
        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine($"\t\tFunc<{TVector}, {TVector}> IVectorizedModel.GetVectorizedFunc(IReadOnlyList<double> parameters)");
        builder.AppendLine("\t\t\t=> (x) =>");
        builder.AppendLine("\t\t\t{");
        builder.AppendLine("\t\t\t\tvar length = x.Length;");
        builder.AppendLine($"\t\t\t\tvar temp = {TVector}.Create(length);");
        builder.AppendLine($"\t\t\t\tvar a0 = {TVector}.Create(length, parameters[0]);");
        for (var i = 1; i <= n; i++)
        {
            builder.AppendLine();
            builder.AppendLine($"\t\t\t\tvar a{i} = parameters[{2 * i - 1}];");
            builder.AppendLine($"\t\t\t\tvar t{i} = -1.0 / parameters[{2 * i}];");
            builder.AppendLine($"\t\t\t\t{TVector}.Multiply(x, t{i}, temp);     // temp = -x / t{i}");
            builder.AppendLine($"\t\t\t\t{TVector}.Exp(temp, temp);           // temp = exp(-x / t{i})");
            builder.AppendLine($"\t\t\t\t{TVector}.Multiply(temp, a{i}, temp);  // temp = a{i} * exp(-x / t{i})");
            builder.AppendLine($"\t\t\t\t{TVector}.Add(a0, temp, a0);         // a0 += a{i} * exp(-x / t{i})");
        }
        builder.AppendLine();
        builder.AppendLine("\t\t\t\treturn a0;");
        builder.AppendLine("\t\t\t};");
    } // private static void GenerateGetVectorizedFunc (StringBuilder, string, int)

    private static void GenerateGetVectorizedDerivatives(StringBuilder builder, string TVector, int n)
    {
        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine($"\t\tAction<{TVector}, {TVector}[]> IVectorizedModel.GetVectorizedDerivatives(IReadOnlyList<double> parameters)");

        builder.AppendLine("\t\t\t=> (x, res) =>");
        builder.AppendLine("\t\t\t{");
        builder.AppendLine("\t\t\t\t// The first parameter is a constant term and its derivative is always 1.0.");
        builder.AppendLine("\t\t\t\tres[0].Load(1.0);");
        for (var i = 1; i <= n; i++)
        {
            builder.AppendLine();
            builder.AppendLine($"\t\t\t\tvar a{i} = parameters[{2 * i - 1}];");
            builder.AppendLine($"\t\t\t\tvar t{i} = -1.0 / parameters[{2 * i - 0}];");

            builder.AppendLine($"\t\t\t\t// res[{2 * i - 1}] = exp(-x / t{i})");
            builder.AppendLine($"\t\t\t\t{TVector}.Multiply(x, t{i}, res[{2 * i - 1}]);  // -x / t{i}");
            builder.AppendLine($"\t\t\t\t{TVector}.Exp(res[{2 * i - 1}], res[{2 * i - 1}]);      // exp(-x / t{i})");

            builder.AppendLine($"\t\t\t\t// res[{2 * i - 0}] = a{i} * x * exp(-x / t{i}) / (t{i} * t{i})");
            builder.AppendLine($"\t\t\t\t{TVector}.Multiply(res[{2 * i - 1}], a{i} * t{i} * t{i}, res[{2 * i - 0}]);  // exp(-x / t{i}) * a{i} / (t{i} * t{i})");
            builder.AppendLine($"\t\t\t\t{TVector}.Multiply(res[{2 * i - 0}], x, res[{2 * i - 0}]);             // x * exp(-x / t{i}) * a{i} / (t{i} * t{i})");
        }
        builder.AppendLine("\t\t\t};");
    } // private static void GenerateGetVectorizedDerivatives (StringBuilder, string, int)
} // internal sealed class ExponentialGenerator : ISourceGenerator
