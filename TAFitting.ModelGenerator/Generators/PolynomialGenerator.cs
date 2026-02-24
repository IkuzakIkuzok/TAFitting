
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

        builder.AppendLine($"    /// <summary>");
        builder.AppendLine($"    /// Represents a {n}{GetSuffix(n)}-order polynomial model.");
        builder.AppendLine($"    /// </summary>");
        builder.AppendLine($"    internal partial class {className} : global::TAFitting.Model.IFittingModel, global::TAFitting.Model.IAnalyticallyDifferentiable, global::TAFitting.Model.IVectorizedModel");
        builder.AppendLine("    {");

        #region fields

        builder.AppendLine("        private static readonly global::TAFitting.Model.Parameters parameters = [");
        for (var i = 0; i <= n; i++)
            builder.AppendLine($"            new() {{ Name = \"A{i}\", InitialValue = {Math.Pow(-1, i)}e+{n - i}, IsMagnitude = true }},");
        builder.AppendLine("        ];");

        #endregion fields

        #region properties

        if (!string.IsNullOrEmpty(name))
        {
            builder.AppendLine();
            builder.AppendLine("        /// <inheritdoc/>");
            builder.AppendLine($"        public string Name => \"{name}\";");
        }

        builder.AppendLine();
        builder.AppendLine("        /// <inheritdoc/>");
        builder.AppendLine("        public string Description => \"" + n + GetSuffix(n) + "-order polynomial model\";");

        builder.AppendLine();
        builder.AppendLine("        /// <inheritdoc/>");
        builder.AppendLine(
            "        public string ExcelFormula => \"[A0] + [A1] * $X"
            + string.Concat(Enumerable.Range(2, n - 1).Select(i => $" + [A{i}] * $X^{i}")) + "\";"
        );

        builder.AppendLine();
        builder.AppendLine("        /// <inheritdoc/>");
        builder.AppendLine("        public global::TAFitting.Model.Parameters Parameters => parameters;");

        builder.AppendLine();
        builder.AppendLine("        /// <inheritdoc/>");
        builder.AppendLine("        public bool XLogScale => false;");

        builder.AppendLine();
        builder.AppendLine("        /// <inheritdoc/>");
        builder.AppendLine("        public bool YLogScale => false;");

        #endregion properties

        #region methods

        builder.AppendLine();
        builder.AppendLine($@"
        [global::System.Runtime.CompilerServices.ModuleInitializer]
        public static void Register()
        {{
            global::TAFitting.Model.ModelManager.AddType(typeof(global::{nameSpace}.{className}));
        }} // public static void Register ()");

        #region GetFunction

        builder.AppendLine();
        builder.AppendLine("        /// <inheritdoc/>");
        builder.AppendLine("        public global::System.Func<double, double> GetFunction(global::System.Collections.Generic.IReadOnlyList<double> parameters)");
        builder.AppendLine("        {");
        for (var i = 0; i <= n; i++)
            builder.AppendLine($"            var a{i} = parameters[{i}];");
        builder.AppendLine();

        if (n > 1)
            builder.AppendLine("            //Horner's method");
        var func = new StringBuilder($"a{n}");
        for (var i = n - 1; i >= 0; --i)
        {
            if (i < n - 1)
            {
                func.Insert(0, "(");
                func.Append($")");
            }
            func.Append($" * x + a{i}");
        }
        builder.AppendLine($"            return (x) => {func};");
        builder.AppendLine("        } // public global::System.Func<double, double> GetFunction (global::System.Collections.Generic.IReadOnlyList<double>)");

        GenerateGetVectorizedFunc(builder, "global::TAFitting.Data.AvxVector", n);

        #endregion GetFunction

        #region GetDerivatives

        builder.AppendLine();
        builder.AppendLine("        /// <inheritdoc/>");
        builder.AppendLine("        public global::System.Action<double, double[]> GetDerivatives(global::System.Collections.Generic.IReadOnlyList<double> parameters)");
        builder.AppendLine("            => Derivatives;");

        builder.AppendLine();
        builder.AppendLine("        private void Derivatives(double x, double[] res)");
        builder.AppendLine("        {");
        builder.AppendLine("            var d_a0 = 1.0;");
        for (var i = 1; i <= n; i++)
        {
            if (i == 1)
                builder.AppendLine($"            var d_a{i} = x;");
            else
                builder.AppendLine($"            var d_a{i} = d_a{i - 1} * x;");
        }
        builder.AppendLine();
        builder.AppendLine("            res[0] = d_a0;");
        builder.Append(string.Join("\n", Enumerable.Range(1, n).Select(i => $"            res[{i}] = d_a{i};")));
        builder.AppendLine("\n        } // private void Derivatives (double, double[])");

        GenerateGetVectorizedDerivatives(builder, "global::TAFitting.Data.AvxVector", n);

        #endregion GetDerivatives

        #endregion methods

        builder.AppendLine($"    }} // internal partial class {className} : global::TAFitting.Model.IFittingModel, global::TAFitting.Model.IAnalyticallyDifferentiable, global::TAFitting.Model.IVectorizedModel");
        builder.AppendLine("} // namespace" + nameSpace);
    } // override protected void Generate (StringBuilder, string, string, int n, string?)

    private static void GenerateGetVectorizedFunc(StringBuilder builder, string TVector, int n)
    {
        builder.AppendLine();
        builder.AppendLine("        /// <inheritdoc/>");
        builder.AppendLine($"        global::System.Action<{TVector}, {TVector}> global::TAFitting.Model.IVectorizedModel.GetVectorizedFunc(global::System.Collections.Generic.IReadOnlyList<double> parameters)");
        builder.AppendLine("            => (x, res) =>");
        builder.AppendLine("            {");

        // Horner's method

        var comment = new StringBuilder($"a{n}");
        builder.AppendLine($"                res.Load(parameters[{n}]);  // a{n}");

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
            comment.Append($" + a{i}");
            builder.AppendLine($"                // {comment}");
            builder.AppendLine($"                res *= x; res += parameters[{i}];");
        }

        builder.AppendLine("            };");
    } // private static void GenerateGetVectorizedFunc (StringBuilder, string, int)

    private static void GenerateGetVectorizedDerivatives(StringBuilder builder, string TVector, int n)
    {
        builder.AppendLine();
        builder.AppendLine("        /// <inheritdoc/>");
        builder.AppendLine($"        global::System.Action<{TVector}, {TVector}[]> global::TAFitting.Model.IVectorizedModel.GetVectorizedDerivatives(global::System.Collections.Generic.IReadOnlyList<double> parameters)");
        builder.AppendLine("            => (x, res) =>");
        builder.AppendLine("            {");
        builder.AppendLine("                res[0].Load(1.0);");
        for (var i = 1; i <= n; i++)
        {
            if (i == 1)
                builder.AppendLine($"                res[{i}] = x;");
            else
                builder.AppendLine($"                {TVector}.Multiply(res[{i - 1}], x, res[{i}]);  // x^{i} = x^{i - 1} * x");
        }
        builder.AppendLine("            };");
    } // private static void GenerateGetVectorizedDerivatives (StringBuilder, string, int)
} // internal sealed class PolynomialGenerator : ModelGeneratorBase
