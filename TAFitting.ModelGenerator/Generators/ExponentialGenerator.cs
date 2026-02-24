
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.ModelGenerator.Generators;

/// <summary>
/// Generates exponential models.
/// </summary>
[Generator(LanguageNames.CSharp)]
internal sealed class ExponentialGenerator : ModelGeneratorBase
{
    override protected string AttributeName => AttributesGenerator.Namespace + "." + AttributesGenerator.ExponentialModelName;

    override protected string FileName => "GeneratedExponentialModels.g.cs";

    override protected void Generate(StringBuilder builder, string nameSpace, string className, int n, string? name)
    {
        builder.AppendLine();
        builder.AppendLine($"namespace {nameSpace}");
        builder.AppendLine("{");

        builder.AppendLine($"    /// <summary>");
        builder.AppendLine($"    /// Represents a {n}-component exponential model.");
        builder.AppendLine($"    /// </summary>");
        builder.AppendLine($"    internal partial class {className} : global::TAFitting.Model.IFittingModel, global::TAFitting.Model.IAnalyticallyDifferentiable, global::TAFitting.Model.IVectorizedModel");
        builder.AppendLine("    {");

        #region fields

        builder.AppendLine("        private static readonly global::TAFitting.Model.Parameters parameters = [");
        builder.AppendLine("            new() { Name = \"A0\", IsMagnitude = true },");
        for (var i = 1; i <= n; i++)
        {
            builder.AppendLine($"            new() {{ Name = \"A{i}\", InitialValue = 1e{3 - i}, IsMagnitude = true }},");
            builder.AppendLine($"            new() {{ Name = \"T{i}\", InitialValue = 5e{i - 1}, Constraints = ParameterConstraints.Positive }},");
        }
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
        builder.AppendLine("        public string Description => \"" + n + "-component exponential model\";");

        builder.AppendLine();
        builder.AppendLine("        /// <inheritdoc/>");
        builder.AppendLine(
            "        public string ExcelFormula => \"[A0]"
            + string.Concat(Enumerable.Range(1, n).Select(i => $" + [A{i}] * EXP(-$X / [T{i}])")) + "\";"
        );

        builder.AppendLine();
        builder.AppendLine("        /// <inheritdoc/>");
        builder.AppendLine("        public global::TAFitting.Model.Parameters Parameters => parameters;");

        builder.AppendLine();
        builder.AppendLine("        /// <inheritdoc/>");
        builder.AppendLine("        public bool XLogScale => false;");

        builder.AppendLine();
        builder.AppendLine("        /// <inheritdoc/>");
        builder.AppendLine("        public bool YLogScale => true;");

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
        builder.AppendLine("            var a0 = parameters[0];");
        for (var i = 1; i <= n; i++)
        {
            builder.AppendLine($"            var a{i} = parameters[{2 * i - 1}];");
            builder.AppendLine($"            var t{i} = -1.0 / parameters[{2 * i}];");
        }
        builder.AppendLine();
        builder.AppendLine("            return x => a0"
            + string.Concat(Enumerable.Range(1, n).Select(i => $" + a{i} * MathUtils.FastExp(x * t{i})")) + ";");
        builder.AppendLine("        } // public global::System.Func<double, double> GetFunction(global::System.Collections.Generic.IReadOnlyList<double> parameters)");

        GenerateGetVectorizedFunc(builder, "global::TAFitting.Data.AvxVector", n);

        #endregion GetFunction

        #region GetDerivatives

        builder.AppendLine();
        builder.AppendLine("        /// <inheritdoc/>");
        builder.AppendLine("        public global::System.Action<double, double[]> GetDerivatives(global::System.Collections.Generic.IReadOnlyList<double> parameters)");
        builder.AppendLine("        {");
        for (var i = 1; i <= n; i++)
        {
            builder.AppendLine($"            var a{i} = parameters[{2 * i - 1}];");
            builder.AppendLine($"            var t{i} = -1.0 / parameters[{2 * i}];");
        }
        builder.AppendLine();

        builder.AppendLine("            return (x, res) =>");
        builder.AppendLine("            {");
        for (var i = 1; i <= n; i++)
        {
            builder.AppendLine($"                var exp{i} = MathUtils.FastExp(x * t{i});");
        }
        builder.AppendLine();
        builder.AppendLine("                var d_a0 = 1.0;");
        for (var i = 1; i <= n; i++)
        {
            builder.AppendLine($"                var d_a{i} = exp{i};");
            builder.AppendLine($"                var d_t{i} = a{i} * x * exp{i} * (t{i} * t{i});");
        }
        builder.AppendLine();
        builder.AppendLine("                res[0] = d_a0;");
        builder.Append(string.Join("\n", Enumerable.Range(1, n).Select(i => $"                res[{2 * i - 1}] = d_a{i};\n                res[{2 * i}] = d_t{i};")));
        builder.AppendLine("\n            };");
        builder.AppendLine("        } // public global::System.Action<double, double[]> GetDerivatives (global::System.Collections.Generic.IReadOnlyList<double>)");

        GenerateGetVectorizedDerivatives(builder, "global::TAFitting.Data.AvxVector", n);

        #endregion GetDerivatives

        #endregion methods

        builder.AppendLine($"    }} // internal partial class {className} : global::TAFitting.Model.IFittingModel, global::TAFitting.Model.IAnalyticallyDifferentiable, global::TAFitting.Model.IVectorizedModel");
        builder.AppendLine("} // namespace " + nameSpace);
    } // override protected void Generate (StringBuilder, string, string, int n, string?)

    private static void GenerateGetVectorizedFunc(StringBuilder builder, string TVector, int n)
    {
        builder.AppendLine();
        builder.AppendLine("        /// <inheritdoc/>");
        builder.AppendLine($"        global::System.Action<{TVector}, {TVector}> global::TAFitting.Model.IVectorizedModel.GetVectorizedFunc(global::System.Collections.Generic.IReadOnlyList<double> parameters)");
        builder.AppendLine("            => (x, res) =>");
        builder.AppendLine("            {");
        builder.AppendLine($"                res.Load(parameters[0]);");
        for (var i = 1; i <= n; i++)
        {
            builder.AppendLine();
            builder.AppendLine($"                var a{i} = parameters[{2 * i - 1}];");
            builder.AppendLine($"                var t{i} = parameters[{2 * i}];");
            builder.AppendLine($"                {TVector}.AddExpDecay(x, a{i}, t{i}, res);  // res += a{i} * exp(-x / t{i})");
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
        builder.AppendLine("                // The first parameter is a constant term and its derivative is always 1.0.");
        builder.AppendLine("                res[0].Load(1.0);");
        for (var i = 1; i <= n; i++)
        {
            builder.AppendLine();
            builder.AppendLine($"                var a{i} = parameters[{2 * i - 1}];");
            builder.AppendLine($"                var t{i} = parameters[{2 * i - 0}];");

            builder.AppendLine($"                // res[{2 * i - 1}] = exp(-x / t{i})");
            builder.AppendLine($"                {TVector}.ExpDecay(x, 1.0, t{i}, res[{2 * i - 1}]);              // exp(-x / t{i})");

            builder.AppendLine($"                // res[{2 * i - 0}] = a{i} * x * exp(-x / t{i}) / (t{i} * t{i})");
            builder.AppendLine($"                {TVector}.Multiply(res[{2 * i - 1}], a{i} / (t{i} * t{i}), res[{2 * i - 0}]);  // exp(-x / t{i}) * a{i} / (t{i} * t{i})");
            builder.AppendLine($"                res[{2 * i - 0}] *= x;                                                                // x * exp(-x / t{i}) * a{i} / (t{i} * t{i})");
        }
        builder.AppendLine("            };");
    } // private static void GenerateGetVectorizedDerivatives (StringBuilder, string, int)
} // internal sealed class ExponentialGenerator : ISourceGenerator
