
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.ModelGenerator.Generators;

/// <summary>
/// Generates polynomial models.
/// </summary>
[Generator(LanguageNames.CSharp)]
internal sealed class PolynomialGenerator : ModelGeneratorBase
{
    override protected string AttributeName => AttributesGenerator.Namespace + "." + AttributesGenerator.PolynomialModelName;

    override protected string FileName => "GeneratedPolynomialModels.g.cs";

    override protected void Generate(StringBuilder builder, string nameSpace, string className, int n, string? name)
    {
        builder.AppendLine();
        builder.AppendLine($"namespace {nameSpace}");
        builder.AppendLine("{");

        builder.AppendLine($"\t/// <summary>");
        builder.AppendLine($"\t/// Represents a {n}{GetSuffix(n)}-order polynomial model.");
        builder.AppendLine($"\t/// </summary>");
        builder.AppendLine($"\tinternal partial class {className} : global::TAFitting.Model.IFittingModel, global::TAFitting.Model.IAnalyticallyDifferentiable, global::TAFitting.Model.IVectorizedModel");
        builder.AppendLine("\t{");

        #region fields

        builder.AppendLine("\t\tprivate static readonly global::TAFitting.Model.Parameters parameters = [");
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
        builder.AppendLine("\t\tpublic global::TAFitting.Model.Parameters Parameters => parameters;");

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
        builder.AppendLine("\t\tpublic global::System.Func<double, double> GetFunction(global::System.Collections.Generic.IReadOnlyList<double> parameters)");
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
        builder.AppendLine("\t\t} // public global::System.Func<double, double> GetFunction (global::System.Collections.Generic.IReadOnlyList<double>)");

        GenerateGetVectorizedFunc(builder, "global::TAFitting.Data.AvxVector", n);

        #endregion GetFunction

        #region GetDerivatives

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine("\t\tpublic global::System.Action<double, double[]> GetDerivatives(global::System.Collections.Generic.IReadOnlyList<double> parameters)");
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

        GenerateGetVectorizedDerivatives(builder, "global::TAFitting.Data.AvxVector", n);

        #endregion GetDerivatives

        #endregion methods

        builder.AppendLine($"\t}} // internal partial class {className} : global::TAFitting.Model.IFittingModel, global::TAFitting.Model.IAnalyticallyDifferentiable, global::TAFitting.Model.IVectorizedModel");
        builder.AppendLine("} // namespace" + nameSpace);
    } // override protected void Generate (StringBuilder, string, string, int n, string?)

    private static void GenerateGetVectorizedFunc(StringBuilder builder, string TVector, int n)
    {
        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine($"\t\tglobal::System.Action<{TVector}, {TVector}> global::TAFitting.Model.IVectorizedModel.GetVectorizedFunc(global::System.Collections.Generic.IReadOnlyList<double> parameters)");
        builder.AppendLine("\t\t\t=> (x, res) =>");
        builder.AppendLine("\t\t\t{");

        // Horner's method

        var comment = new StringBuilder($"a{n}");
        builder.AppendLine($"\t\t\t\tres.Load(parameters[{n}]);  // a{n}");

        for (var i = n - 1; i >= 0; --i)
        {
            builder.AppendLine();
            if (i < n - 1)
            {
                comment.Insert(0, "(");
                comment.Append($") * x");
            }
            else
            {
                comment.Append(" * x");
            }
            builder.AppendLine($"\t\t\t\t{TVector}.Multiply(x, res, res);         // {comment}");

            comment.Append($" + a{i}");
            builder.AppendLine($"\t\t\t\t{TVector}.Add(res, parameters[{i}], res);  // {comment}");
        }

        builder.AppendLine("\t\t\t};");
    } // private static void GenerateGetVectorizedFunc (StringBuilder, string, int)

    private static void GenerateGetVectorizedDerivatives(StringBuilder builder, string TVector, int n)
    {
        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine($"\t\tglobal::System.Action<{TVector}, {TVector}[]> global::TAFitting.Model.IVectorizedModel.GetVectorizedDerivatives(global::System.Collections.Generic.IReadOnlyList<double> parameters)");
        builder.AppendLine("\t\t\t=> (x, res) =>");
        builder.AppendLine("\t\t\t{");
        builder.AppendLine("\t\t\t\tres[0].Load(1.0);");
        for (var i = 1; i <= n; i++)
        {
            if (i == 1)
                builder.AppendLine($"\t\t\t\tres[{i}] = x;");
            else
                builder.AppendLine($"\t\t\t\t{TVector}.Multiply(res[{i - 1}], x, res[{i}]);  // x^{i} = x^{i - 1} * x");
        }
        builder.AppendLine("\t\t\t};");
    } // private static void GenerateGetVectorizedDerivatives (StringBuilder, string, int)
} // internal sealed class PolynomialGenerator : ModelGeneratorBase
