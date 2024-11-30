
// (c) 2024 Kazuki KOHZUKI

using System.Runtime.InteropServices;

namespace TAFitting.ModelGenerator.Generators;

/// <summary>
/// Generates exponential models.
/// </summary>
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

        builder.AppendLine($"\t/// <summary>");
        builder.AppendLine($"\t/// Represents a {n}-component exponential model.");
        builder.AppendLine($"\t/// </summary>");
        builder.AppendLine($"\tinternal partial class {className} : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>");
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
            + string.Concat(Enumerable.Range(1, n).Select(i => $" + a{i} * MathUtil.FastExp(x * t{i})")) + ";");
        builder.AppendLine("\t\t} // public Func<double, double> GetFunction(IReadOnlyList<double> parameters)");

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine("\t\tpublic Func<AvxVector2048, AvxVector2048> GetVectorizedFunc(IReadOnlyList<double> parameters)");
        builder.AppendLine("\t\t\t=> x =>");
        builder.AppendLine("\t\t\t{");
        builder.AppendLine("\t\t\t\tvar length = x.Length << 2;");
        builder.AppendLine("\t\t\t\tvar temp = AvxVector2048.Create(length);");
        builder.AppendLine("\t\t\t\tvar a0 = AvxVector2048.Create(length, parameters[0]);");
        for (var i = 1; i <= n; i++)
        {
            builder.AppendLine();
            builder.AppendLine($"\t\t\t\tvar a{i} = parameters[{2 * i - 1}];");
            builder.AppendLine($"\t\t\t\tvar t{i} = -1.0 / parameters[{2 * i}];");
            builder.AppendLine($"\t\t\t\tAvxVector2048.Multiply(x, t{i}, temp);     // temp = -x / t{i}");
            builder.AppendLine($"\t\t\t\tAvxVector2048.Exp(temp, temp);           // temp = exp(-x / t{i})");
            builder.AppendLine($"\t\t\t\tAvxVector2048.Multiply(temp, a{i}, temp);  // temp = a{i} * exp(-x / t{i})");
            builder.AppendLine($"\t\t\t\tAvxVector2048.Add(a0, temp, a0);         // a0 += a{i} * exp(-x / t{i})");
        }
        builder.AppendLine();
        builder.AppendLine("\t\t\t\treturn a0;");
        builder.AppendLine("\t\t\t};");

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
            builder.AppendLine($"\t\t\t\tvar exp{i} = MathUtil.FastExp(x * t{i});");
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

        builder.AppendLine();
        builder.AppendLine("\t\t/// <inheritdoc/>");
        builder.AppendLine("\t\tpublic Action<AvxVector2048, AvxVector2048[]> GetVectorizedDerivatives(IReadOnlyList<double> parameters)");
        
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
            builder.AppendLine($"\t\t\t\tAvxVector2048.Multiply(x, t{i}, res[{2 * i - 1}]);  // -x / t{i}");
            builder.AppendLine($"\t\t\t\tAvxVector2048.Exp(res[{2 * i - 1}], res[{2 * i - 1}]);      // exp(-x / t{i})");

            builder.AppendLine($"\t\t\t\t// res[{2 * i - 0}] = a{i} * x * exp(-x / t{i}) / (t{i} * t{i})");
            builder.AppendLine($"\t\t\t\tAvxVector2048.Multiply(res[{2 * i - 1}], a{i} * t{i} * t{i}, res[{2 * i - 0}]);  // exp(-x / t{i}) * a{i} / (t{i} * t{i})");
            builder.AppendLine($"\t\t\t\tAvxVector2048.Multiply(res[{2 * i - 0}], x, res[{2 * i - 0}]);             // x * exp(-x / t{i}) * a{i} / (t{i} * t{i})");
        }
        builder.AppendLine("\t\t\t};");

        #endregion GetDerivatives

        #endregion methods

        builder.AppendLine($"\t}} // internal partial class {className} : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>");
        builder.AppendLine("} // namespace " + nameSpace);

        return builder.ToString();
    } // override protected string Generate (string, string, int, string?)

    /// <summary>
    /// Generates the mask of the specified number of bits.
    /// </summary>
    /// <param name="n">The number of bits.</param>
    /// <returns>The 64-bit mask of the specified number of bits.</returns>
    private static ulong Mask64(int n) => (1UL << n) - 1;

    private const int TABLE_SIZE = 11;
    private const int s = 1 << TABLE_SIZE;

    /// <summary>
    /// Computes the table value at the specified index.
    /// </summary>
    /// <param name="index">The index of the table.</param>
    /// <returns>The table value at the specified index.</returns>
    private static ulong ComputeTable(int index)
    {
        var du = new BitsConverter64 { Double = Math.Pow(2, index * (1.0 / s)) };
        return du.UInt64 & Mask64(52);
    } // private static ulong ComputeTable (int)

    /// <summary>
    /// A union to convert between <see cref="double"/>, <see cref="ulong"/>, and <see cref="long"/>.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    private struct BitsConverter64
    {
        /// <summary>
        /// A <see cref="double"/> value.
        /// </summary>
        [FieldOffset(0)]
        public double Double;

        /// <summary>
        /// A <see cref="ulong"/> value.
        /// </summary>
        [FieldOffset(0)]
        public ulong UInt64;

        /// <summary>
        /// A <see cref="long"/> value.
        /// </summary>
        [FieldOffset(0)]
        public long Int64;
    } // private struct BitsConverter64

    private static IEnumerable<string> GenerateElements(int n, int elementsInLine, Func<int, string> func, string separator)
    {
        for (var i = 0; i < n; i += elementsInLine)
        {
            var start = i;
            var end = Math.Min(i + elementsInLine, n);
            var elements = string.Join(separator, Enumerable.Range(start, end - start).Select(func));
            yield return elements;
        }
    } // private static IEnumerable<string> GenerateElements (int, int, Func<int, string>, string)

    private static readonly string MathUtil = @$"
using System.Runtime.CompilerServices;
using TAFitting.Data.Solver.SIMD;

/// <summary>
/// Provides mathematical utility functions.
/// </summary>
file static class MathUtil
{{
    private static readonly ulong[] table = [
        {string.Join(",\n\t\t", GenerateElements(s, 8, i => ComputeTable(i).ToString() + "UL", ", "))}
    ];

    /// <summary>
    /// Computes the exponential function of the specified number.
    /// </summary>
    /// <param name=""x"">The number to compute the exponential function.</param>
    /// <returns>The exponential function of the specified number.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double FastExp(double x)
    {{
        if (x <= {Math.Log(Math.Pow(2, -1022))}) return 0;
        if (x >= {Math.Log(double.MaxValue)}) return double.PositiveInfinity;

        var d = x * ({2048 / Math.Log(2)}) + ({3UL << 51});
        var i = BitConverter.DoubleToUInt64Bits(d);
        var iax = table[i & 2047];
        var t = (d - ({3UL << 51})) * {Math.Log(2) / 2048} - x;
        var u = ((i + {(1UL << (TABLE_SIZE + 10)) - (1UL << TABLE_SIZE)}) >> 11) << 52;
        var y = (3.0000000027955394 - t) * (t * t) * 0.16666666685227835 - t + 1;
        i = u | iax;
        d = BitConverter.UInt64BitsToDouble(i);

        return d * y;
    }} // internal static double FastExp (double)
}} // file static class MathUtil
";
} // internal sealed class ExponentialGenerator : ISourceGenerator
