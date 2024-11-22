
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.ModelGenerator.Generators;

[Generator(LanguageNames.CSharp)]
internal sealed class ExponentialGenerator : ModelGeneratorBase
{
    override protected string AttributeName => AttributesGenerator.ExponentialModelName;

    override protected string FileName => "GeneratedExponentialModels.g.cs";

    override protected string AdditionalCode => MathUtil;

    override protected string Generate(string nameSpace, string className, int n, string? name)
    {
        var builder = new StringBuilder();

        builder.AppendLine();
        builder.AppendLine($"namespace {nameSpace}");
        builder.AppendLine("{");

        builder.AppendLine();
        builder.AppendLine($"\t/// <summary>");
        builder.AppendLine($"\t/// Represents a {n}-component exponential model.");
        builder.AppendLine($"\t/// </summary>");
        builder.AppendLine($"\tinternal partial class {className} : IFittingModel, IAnalyticallyDifferentiable");
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
            builder.AppendLine($"\t\t\tvar t{i} = parameters[{2 * i}];");
        }
        builder.AppendLine();
        builder.AppendLine("\t\t\treturn x => a0"
            + string.Concat(Enumerable.Range(1, n).Select(i => $" + a{i} * MathUtil.FastExp(-x / t{i})")) + ";");
        builder.AppendLine("\t\t} // public Func<double, double> GetFunction(IReadOnlyList<double> parameters)");

        #endregion GetFunction

        #region GetDerivatives

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine("\t\tpublic Action<double, double[]> GetDerivatives(IReadOnlyList<double> parameters)");
        builder.AppendLine("\t\t{");
        for (var i = 1; i <= n; i++)
        {
            builder.AppendLine($"\t\t\tvar a{i} = parameters[{2 * i - 1}];");
            builder.AppendLine($"\t\t\tvar t{i} = parameters[{2 * i}];");
        }
        builder.AppendLine();

        builder.AppendLine("\t\t\treturn (x, res) =>");
        builder.AppendLine("\t\t\t{");
        for (var i = 1; i <= n; i++)
        {
            builder.AppendLine($"\t\t\t\tvar exp{i} = MathUtil.FastExp(-x / t{i});");
        }
        builder.AppendLine();
        builder.AppendLine("\t\t\t\tvar d_a0 = 1.0;");
        for (var i = 1; i <= n; i++)
        {
            builder.AppendLine($"\t\t\t\tvar d_a{i} = exp{i};");
            builder.AppendLine($"\t\t\t\tvar d_t{i} = a{i} * x * exp{i} / (t{i} * t{i});");
        }
        builder.AppendLine();
        builder.AppendLine("\t\t\t\tres[0] = d_a0;");
        builder.Append(string.Join("\n", Enumerable.Range(1, n).Select(i => $"\t\t\t\tres[{2 * i - 1}] = d_a{i};\n\t\t\t\tres[{2 * i}] = d_t{i};")));
        builder.AppendLine("\n\t\t\t};");
        builder.AppendLine("\t\t} // public Action<double, double[]> GetDerivatives (IReadOnlyList<double>)");

        #endregion GetDerivatives

        #endregion methods

        builder.AppendLine($"\t}} // internal partial class {className} : IFittingModel, IAnalyticallyDifferentiable");
        builder.AppendLine("} // namespace " + nameSpace);

        return builder.ToString();
    } // override protected string Generate (string, string, int, string?)

    private static readonly string MathUtil = @$"
using System.Runtime.CompilerServices;

file static class MathUtil
{{
    private const int TABLE_SIZE = 11;
    private const int s = 1 << TABLE_SIZE;

    private static readonly ulong[] table;

    static MathUtil()
    {{
        table = new ulong[s];
    }} // cctor ()

    [ModuleInitializer]
    internal static void MakeTable()
    {{
        var di = new DoubleUInt64();
        for (var i = 0UL; i < s; i++)
        {{
            di.Double = Math.Pow(2, i * (1.0 / s));
            table[i] = di.UInt64 & Mask64(52);
        }}
    }} // private static void MakeTable ()

    private static ulong Mask64(int n) => (1UL << n) - 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double FastExp(double x)
    {{
        if (x <= {Math.Log(double.Epsilon)}) return 0;
        if (x >= {Math.Log(double.MaxValue)}) return double.PositiveInfinity;

        var d = x * ({2048 / Math.Log(2)}) + ({3UL << 51});
        var i = BitConverter.DoubleToUInt64Bits(d);
        var iax = table[i & 2047];
        var t = (d - ({3UL << 51})) * {Math.Log(2) / 2048} - x;
        var u = ((i + 2095104) >> 11) << 52;
        var y = (3.0000000027955394 - t) * (t * t) * 0.16666666685227835 - t + 1;
        i = u | iax;
        d = BitConverter.UInt64BitsToDouble(i);

        return d * y;
    }} // internal static double FastExp (double)

    [StructLayout(LayoutKind.Explicit)]
    private struct DoubleUInt64
    {{
        [FieldOffset(0)]
        public double Double = 0;

        [FieldOffset(0)]
        public ulong UInt64 = 0;

        public DoubleUInt64() {{ }}
    }} // private struct DoubleUInt64
}} // file static class MathUtil
";
} // internal sealed class ExponentialGenerator : ISourceGenerator
