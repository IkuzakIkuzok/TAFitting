
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.ModelGenerator.Generators;

[Generator(LanguageNames.CSharp)]
internal sealed class PolynomialGenerator : ModelGeneratorBase
{
    override protected string AttributeName => AttributesGenerator.PolynomialModelName;

    override protected string FileName => "GeneratedPolynomialModels.g.cs";

    override protected string AdditionalCode => "using TAFitting.Data.Solver.SIMD;";

    override protected string Generate(string nameSpace, string className, int n, string? name)
    {
        var builder = new StringBuilder();

        builder.AppendLine();
        builder.AppendLine($"namespace {nameSpace}");
        builder.AppendLine("{");

        builder.AppendLine();
        builder.AppendLine($"\t/// <summary>");
        builder.AppendLine($"\t/// Represents a {n}{GetSuffix(n)}-order polynomial model.");
        builder.AppendLine($"\t/// </summary>");
        builder.AppendLine($"\tinternal partial class {className} : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>");
        builder.AppendLine("\t{");

        #region fields

        builder.AppendLine("\t\tprivate static readonly Parameter[] parameters = [");
        for (var i = 0; i <= n; i++)
            builder.AppendLine($"\t\t\tnew() {{ Name = \"A{i}\", InitialValue = {Math.Pow(-1, i)}e+{n - i}, IsMagnitude = true }},");
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
        builder.AppendLine("\t\tpublic string Description => \"" + n + GetSuffix(n) + "-order polynomial model\";");

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine(
            "\t\tpublic string ExcelFormula => \"[A0] + [A1] * $X"
            + string.Concat(Enumerable.Range(2, n - 1).Select(i => $" + [A{i}] * $X^{i}")) + "\";"
        );

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine("\t\tpublic IReadOnlyList<Parameter> Parameters => parameters;");

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine("\t\tpublic bool XLogScale => false;");

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine("\t\tpublic bool YLogScale => false;");

        #endregion properties

        #region methods

        #region GetFunction

        static string GetTerm(int i) => $"a{i} * x{i}";

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine("\t\tpublic Func<double, double> GetFunction(IReadOnlyList<double> parameters)");
        builder.AppendLine("\t\t{");
        for (var i = 0; i <= n; i++)
            builder.AppendLine($"\t\t\tvar a{i} = parameters[{i}];");
        builder.AppendLine();

        if (n == 1)
        {
            builder.AppendLine("\t\t\treturn x => a0 + a1 * x;");
        }
        else if (n == 2)
        {
            builder.AppendLine("\t\t\treturn x => a0 + a1 * x + a2 * x * x;");
        }
        else
        {
            builder.AppendLine("\t\t\treturn (x) =>");
            builder.AppendLine("\t\t\t{");
            for (var i = 1; i <= n; i++)
            {
                if (i == 1)
                    builder.AppendLine($"\t\t\t\tvar x1 = x;");
                else
                    builder.AppendLine($"\t\t\t\tvar x{i} = x{i - 1} * x;");
            }

            builder.AppendLine($"\t\t\t\treturn a0 + {string.Join(" + ", Enumerable.Range(1, n).Select(GetTerm))};");
            builder.AppendLine("\t\t\t};");
        } // if (n == 1)
        builder.AppendLine("\t\t} // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)");

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine("\t\tpublic Func<AvxVector2048, AvxVector2048> GetVectorizedFunc(IReadOnlyList<double> parameters)");
        builder.AppendLine("\t\t\t=> x => ");
        builder.AppendLine("\t\t\t{");
        builder.AppendLine("\t\t\t\tvar length = x.Length << 2;");
        if (n == 1)
        {
            builder.AppendLine("\t\t\t\tvar a = new AvxVector2048(length, parameters[1]);");
            builder.AppendLine("\t\t\t\tAvxVector2048.Multiply(a, x, a);");
            builder.AppendLine("\t\t\t\tAvxVector2048.Add(a, parameters[0], a);");
            builder.AppendLine("\t\t\t\treturn a;");
        }
        else
        {
            builder.AppendLine("\t\t\t\tvar temp = new AvxVector2048(length);");
            builder.AppendLine("\t\t\t\tvar temp_x = new AvxVector2048(length, 1.0);");
            builder.AppendLine("\t\t\t\tvar a0 = new AvxVector2048(length, parameters[0]);");
            for (var i = 1; i <= n; i++)
            {
                builder.AppendLine();
                builder.AppendLine($"\t\t\t\tvar a{i} = parameters[{i}];");
                builder.AppendLine("\t\t\t\tAvxVector2048.Multiply(x, temp_x, temp_x);");
                builder.AppendLine($"\t\t\t\tAvxVector2048.Multiply(temp_x, a{i}, temp);");
                builder.AppendLine($"\t\t\t\tAvxVector2048.Add(temp, a0, a0);");
            }
            builder.AppendLine();
            builder.AppendLine("\t\t\t\treturn a0;");
        }
        builder.AppendLine("\t\t\t};");

        #endregion GetFunction

        #region GetDerivatives

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine("\t\tpublic Action<double, double[]> GetDerivatives(IReadOnlyList<double> parameters)");
        builder.AppendLine("\t\t\t=> Derivatives;");

        builder.AppendLine();
        builder.AppendLine("\t\tprivate void Derivatives(double x, double[] res)");
        builder.AppendLine("\t\t{");
        builder.AppendLine("\t\t\tvar d_a0 = 1.0;");
        for (var i = 1; i <= n; i++)
        {
            if (i == 1)
                builder.AppendLine($"\t\t\tvar d_a{i} = x;");
            else
                builder.AppendLine($"\t\t\tvar d_a{i} = d_a{i - 1} * x;");
        }
        builder.AppendLine();
        builder.AppendLine("\t\t\tres[0] = d_a0;");
        builder.Append(string.Join("\n", Enumerable.Range(1, n).Select(i => $"\t\t\tres[{i}] = d_a{i};")));
        builder.AppendLine("\n\t\t} // private void Derivatives (double, double[])");

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine("\t\tpublic Action<AvxVector2048, AvxVector2048[]> GetVectorizedDerivatives(IReadOnlyList<double> parameters)");
        builder.AppendLine("\t\t\t=> (x, res) =>");
        builder.AppendLine("\t\t\t{");
        builder.AppendLine("\t\t\t\tres[0].Load(1.0);");
        for (var i = 1; i <= n; i++)
        {
            if (i == 1)
                builder.AppendLine($"\t\t\t\tres[{i}] = x;");
            else
                builder.AppendLine($"\t\t\t\tAvxVector2048.Multiply(res[{i - 1}], x, res[{i}]);");
        }
        builder.AppendLine("\t\t\t};");

        #endregion GetDerivatives

        #endregion methods

        builder.AppendLine($"\t}} // internal partial class {className} : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>");
        builder.AppendLine("} // namespace" + nameSpace);

        return builder.ToString();
    } // override protected string Generate (string, string, int, string?)
} // internal sealed class PolynomialGenerator : ModelGeneratorBase
