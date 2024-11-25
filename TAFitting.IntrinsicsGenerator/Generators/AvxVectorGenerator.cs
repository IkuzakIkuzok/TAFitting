﻿
// (c) 2024 Kazuki Kohzuki

using System.Runtime.InteropServices;

namespace TAFitting.IntrinsicsGenerator.Generators;

[Generator(LanguageNames.CSharp)]
internal sealed class AvxVectorGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new AttributeSyntaxReceiver(AvxVectorAttributesGenerator.AttributeName));
    } // public void Initialize (GeneratorInitializationContext)

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not AttributeSyntaxReceiver receiver) return;

        if (receiver.Vectors.Count == 0) return;

        var builder = new StringBuilder(AvxVectorSource);
        foreach (var declaration in receiver.Vectors)
        {
            try
            {
                var klass = declaration.Target;
                var attr = declaration.Attribute;

                (var nameSpace, var className) = GetFullyQualifiedName(context, klass);
                var count = GetCount(context, attr);
                var source = Generate(nameSpace, className, count);
                builder.Append(source);
            }
            catch
            {

            }
        }

        context.AddSource("AvxVectors.g.cs", builder.ToString());
    } // public void Execute (GeneratorExecutionContext)

    private static ulong Mask64(int n) => (1UL << n) - 1;

    private const int TABLE_SIZE = 11;
    private const int s = 1 << TABLE_SIZE;

    private static ulong ComputeTable(int index)
    {
        var du = new BitsConverter64 { Double = Math.Pow(2, index * (1.0 / s)) };
        return du.UInt64 & Mask64(52);
    } // private static ulong ComputeTable (int)

    [StructLayout(LayoutKind.Explicit)]
    private struct BitsConverter64
    {
        [FieldOffset(0)]
        public double Double;

        [FieldOffset(0)]
        public ulong UInt64;

        [FieldOffset(0)]
        public long Int64;
    } // private struct BitsConverter64

    private static readonly string AvxVectorSource = @$"// <auto-generated/>

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using TAFitting.Data;

namespace TAFitting.Data
{{
    file static class MathUtils
    {{
        private static readonly Vector256<double> ExpMin, ExpMax;
        private static readonly Vector256<double> Alpha, AlphaInv;
        private static readonly Vector256<double> C1, C2, C3;
        private static readonly Vector256<double> Round;
        private static readonly Vector256<ulong> Mask11;
        private static readonly Vector256<ulong> V2095104;

        private static readonly ulong[] table = [
            {string.Join(",\n\t\t\t", GenerateElements(s, 8, i => ComputeTable(i).ToString() + "UL", ", "))}
        ];

        static MathUtils()
        {{
            ExpMin = Create({Math.Log(Math.Pow(2, -1022))});
            ExpMax = Create({Math.Log(double.MaxValue)});
            Alpha = Create({2048 / Math.Log(2)});
            AlphaInv = Create({Math.Log(2) / 2048});
            C3 = Create(3.0000000027955394);
            C2 = Create(0.16666666685227835);
            C1 = Create(1.0);
            Round = Create({3UL << 51}.0);
            Mask11 = Create(2047UL);
            V2095104 = Create(2095104);
        }} // static MathUtils()

        unsafe private static Vector256<double> Create(double value)
        {{
            var arr = stackalloc double[4] {{ value, value, value, value }};
            return Avx.LoadVector256(arr);
        }} // unsafe private static Vector256<double> Create (double)

        unsafe private static Vector256<ulong> Create(ulong value)
        {{
            var arr = stackalloc ulong[4] {{ value, value, value, value }};
            return Avx.LoadVector256(arr);
        }} // private static Vector256<ulong> Create (ulong)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe internal static Vector256<double> Exp(Vector256<double> v)
        {{
            if (Avx2.IsSupported)
            {{
                v = Avx.Min(v, ExpMax);
                v = Avx.Max(v, ExpMin);

                var d = Avx.Add(Avx.Multiply(v, Alpha), Round);
                var i = d.AsUInt64<double>();

                var u = Avx2.Add(i, V2095104);
                u = Avx2.ShiftRightLogical(u, 11);
                u = Avx2.ShiftLeftLogical(u, 52);

                var t = Avx.Subtract(Avx.Multiply(Avx.Subtract(d, Round), AlphaInv), v);
                var y = Avx.Multiply(Avx.Subtract(C3, t), Avx.Multiply(t, t));
                y = Avx.Multiply(y, C2);
                y = Avx.Add(Avx.Subtract(y, t), C1);

                var adr = Avx2.And(i, Mask11).AsInt64();
                fixed (ulong* p = table)
                {{
                    var iax = Avx2.GatherVector256(p, adr, 8);
                    i = Avx2.Or(u, iax);
                    return Avx.Multiply(i.AsDouble(), y);
                }}
            }}
            else
            {{
                var arr = stackalloc double[4];
                arr[0] = FastExp(v.GetElement(0));
                arr[1] = FastExp(v.GetElement(1));
                arr[2] = FastExp(v.GetElement(2));
                arr[3] = FastExp(v.GetElement(3));
                return Avx.LoadVector256(arr);
            }}
        }} // unsafe internal static Vector256<double> Exp (Vector256<double>)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double FastExp(double x)
        {{
            if (x <= {Math.Log(Math.Pow(2, -1022))}) return 0;
            if (x >= {Math.Log(double.MaxValue)}) return double.PositiveInfinity;

            var d = x * ({2048 / Math.Log(2)}) + ({3UL << 51});
            var i = BitConverter.DoubleToUInt64Bits(d);
            var iax = table[i & 2047];
            var t = (d - ({3UL << 51})) * {Math.Log(2) / 2048} - x;
            var u = ((i + 2095104) >> 11) << 52;
            var y = (3.0000000027955394 - t) * (t * t) * 0.16666666685227835 - t + 1;
            i = u | iax;
            d = BitConverter.UInt64BitsToDouble(i);

            var e = d * y;
            var l = Math.Exp(x);
            System.Diagnostics.Debug.WriteLineIf(Math.Abs(e - l) > 1e-6 * l, $""{{x}}: {{e}} != {{l}}"");
            return d * y;
        }} // private static double FastExp (double)
    }} // file static class MathUtils
}} // namespace TAFitting.Data
";

    private static string Generate(string nameSpace, string className, int count)
    {
        var n = count / 4;

        var builder = new StringBuilder();

        builder.AppendLine();
        builder.AppendLine($"namespace {nameSpace}");
        builder.AppendLine("{");

        builder.AppendLine($"\t/// <summary>");
        builder.AppendLine($"\t/// An AVX vector of {count} elements.");
        builder.AppendLine($"\t/// </summary>");
        builder.AppendLine($"\tinternal partial class {className} : IIntrinsicVector<{className}>");
        builder.AppendLine("\t{");

        builder.AppendLine("\t\tprivate static readonly bool isSupported = Avx.IsSupported && Vector256<double>.IsSupported;");

        builder.AppendLine();
        builder.AppendLine("\t\t#region static properties");

        builder.AppendLine();
        builder.AppendLine("\t\t///");
        builder.AppendLine("\t\t/// Gets a value indicating whether the AVX instruction set is supported.");
        builder.AppendLine("\t\t///");
        builder.AppendLine("\t\t/// <value><see langword=\"true\"/> if the AVX instruction set is supported; otherwise, <see langword=\"false\"/>.</value>");
        builder.AppendLine("\t\tinternal static bool IsSupported => isSupported;");

        builder.AppendLine();
        builder.AppendLine("\t\t///");
        builder.AppendLine("\t\t/// Gets the capacity of the vector.");
        builder.AppendLine("\t\t///");
        builder.AppendLine("\t\tinternal static int Capacity => " + count + ";");
        builder.AppendLine();
        builder.AppendLine("\t\t#endregion static properties");

        builder.AppendLine();
        builder.AppendLine("\t\t#region instance fields");

        builder.AppendLine();
        builder.AppendLine("\t\tprivate readonly int count;");
        foreach (var element in GenerateElements(n, 16, x => $"v{x}", ", "))
            builder.AppendLine($"\t\tprivate Vector256<double> {element};");
        builder.AppendLine();
        builder.AppendLine("\t\t#endregion instance fields");

        builder.AppendLine();
        builder.AppendLine("\t\t#region instance properties");

        builder.AppendLine();
        builder.AppendLine("\t\t/// <summary>");
        builder.AppendLine("\t\t/// Gets the number of elements.");
        builder.AppendLine("\t\t/// </summary>");
        builder.AppendLine("\t\tinternal int Length => " + count + ";");

        builder.AppendLine();
        builder.AppendLine("\t\tpublic double Sum");
        builder.AppendLine("\t\t{");
        builder.AppendLine("\t\t\tget");
        builder.AppendLine("\t\t\t{");
        builder.AppendLine("\t\t\t\tvar acc = Vector256<double>.Zero;");
        foreach (var element in GenerateElements(n, 4, x => $"acc = Avx.Add(acc, this.v{x})", "; "))
            builder.AppendLine($"\t\t\t\t{element};");
        builder.AppendLine();
        builder.AppendLine("\t\t\t\treturn acc.GetElement(0) + acc.GetElement(1) + acc.GetElement(2) + acc.GetElement(3);");
        builder.AppendLine("\t\t\t}");
        builder.AppendLine("\t\t}");
        builder.AppendLine();
        builder.AppendLine("\t\t#endregion instance properties");

        builder.AppendLine();
        builder.AppendLine("\t\t#region constructors");

        builder.AppendLine();
        builder.AppendLine("\t\t/// <summary>");
        builder.AppendLine("\t\t/// Initializes a new instance of the <see cref=\"AvxVector\"/> class with the specified values.");
        builder.AppendLine("\t\t/// </summary>");
        builder.AppendLine("\t\t/// <param name=\"values\">The values.</param>");
        builder.AppendLine($"\t\tunsafe internal {className}(double[] values)");
        builder.AppendLine("\t\t{");
        builder.AppendLine($"\t\t\tthis.count = Math.Min(values.Length >> 2, {n});");
        builder.AppendLine("\t\t\tfixed (double* p = values)");
        builder.AppendLine("\t\t\t{");
        foreach (var element in GenerateElements(n, 4, x => $"this.v{x} = Avx.LoadVector256(p + {4 * x})", "; "))
            builder.AppendLine($"\t\t\t\t{element};");
        builder.AppendLine("\t\t\t}");
        builder.AppendLine("\t\t} // ctor (double[])");

        builder.AppendLine();
        builder.AppendLine("\t\t/// <summary>");
        builder.AppendLine("\t\t/// Initializes a new instance of the <see cref=\"AvxVector\"/> class with the specified value.");
        builder.AppendLine("\t\t/// </summary>");
        builder.AppendLine("\t\t/// <param name=\"length\">The length.</param>");
        builder.AppendLine("\t\t/// <param name=\"value\">The value.</param>");
        builder.AppendLine($"\t\tunsafe internal {className}(int length, double value)");
        builder.AppendLine("\t\t{");
        builder.AppendLine($"\t\t\tthis.count = Math.Min(length >> 2, {n});");
        builder.AppendLine("\t\t\tvar arr = stackalloc double[4] { value, value, value, value };");
        foreach (var element in GenerateElements(n, 4, x => $"this.v{x} = Avx.LoadVector256(arr)", "; "))
            builder.AppendLine($"\t\t\t{element};");
        builder.AppendLine("\t\t} // ctor (int, double)");

        builder.AppendLine();
        builder.AppendLine("\t\t/// <summary>");
        builder.AppendLine("\t\t/// Initializes a new instance of the <see cref=\"AvxVector\"/> class with the specified length.");
        builder.AppendLine("\t\t/// </summary>");
        builder.AppendLine("\t\t/// <param name=\"length\">The length.</param>");
        builder.AppendLine($"\t\tinternal {className}(int length)");
        builder.AppendLine("\t\t{");
        builder.AppendLine($"\t\t\tthis.count = Math.Min(length >> 2, {n});");
        foreach (var element in GenerateElements(n, 4, x => $"this.v{x} = Vector256<double>.Zero", "; "))
            builder.AppendLine($"\t\t\t{element};");
        builder.AppendLine("\t\t} // ctor (int)");

        builder.AppendLine();
        builder.AppendLine("\t\t/// <summary>");
        builder.AppendLine("\t\t/// Initializes a new instance of the <see cref=\"AvxVector\"/> class with the specified vectors.");
        builder.AppendLine("\t\t/// </summary>");
        builder.AppendLine("\t\t/// <param name=\"count\">The count.</param>");
        builder.AppendLine("\t\t/// <param name=\"vectors\">The vectors.</param>");
        builder.AppendLine($"\t\tprivate {className}(int count, ReadOnlySpan<Vector256<double>> vectors)");
        builder.AppendLine("\t\t{");
        builder.AppendLine("\t\t\tthis.count = count;");
        foreach (var element in GenerateElements(n, 4, x => $"this.v{x} = vectors[{x}]", "; "))
            builder.AppendLine($"\t\t\t{element};");
        builder.AppendLine("\t\t} // ctor (int, ReadOnlySpan<Vector256<double>>)");
        builder.AppendLine();
        builder.AppendLine("\t\t#endregion constructors");

        builder.AppendLine();
        builder.AppendLine($"\t\t#region IIntrinsicVector<{className}>");

        builder.AppendLine();
        builder.AppendLine("\t\tunsafe public void Load(double[] values)");
        builder.AppendLine("\t\t{");
        builder.AppendLine("\t\t\tfixed (double* p = values)");
        builder.AppendLine("\t\t\t{");
        foreach (var element in GenerateElements(n, 4, x => $"this.v{x} = Avx.LoadVector256(p + {4 * x})", "; "))
            builder.AppendLine($"\t\t\t\t{element};");
        builder.AppendLine("\t\t\t}");
        builder.AppendLine("\t\t} // unsafe public void Load (double[])");

        builder.AppendLine();
        builder.AppendLine("\t\tunsafe public void Load(double value)");
        builder.AppendLine("\t\t{");
        builder.AppendLine("\t\t\tvar arr = stackalloc double[4] { value, value, value, value };");
        foreach (var element in GenerateElements(n, 4, x => $"this.v{x} = Avx.LoadVector256(arr)", "; "))
            builder.AppendLine($"\t\t\t{element};");
        builder.AppendLine("\t\t} // unsafe public void Load (double)");

        builder.AppendLine();
        builder.AppendLine($"\t\tpublic static {className} Create(double[] values)");
        builder.AppendLine("\t\t\t=> new(values);");

        builder.AppendLine();
        builder.AppendLine($"\t\tpublic static {className} Create(int length, double value)");
        builder.AppendLine("\t\t\t=> new(length, value);");

        builder.AppendLine();
        builder.AppendLine($"\t\tpublic static {className} Create(int length)");
        builder.AppendLine("\t\t\t=> new(length);");

        builder.AppendLine();
        builder.AppendLine("\t\tpublic static int GetCapacity()");
        builder.AppendLine("\t\t\t=> Capacity;");

        builder.AppendLine();
        builder.AppendLine("\t\tpublic static bool CheckSupported()");
        builder.AppendLine("\t\t\t=> IsSupported;");

        AddCalculationVectorVector(builder, className, "Add", "Add", n);
        AddCalculationVectorScaler(builder, className, "Add", "Add", n);

        AddCalculationVectorVector(builder, className, "Subtract", "Subtract", n);
        AddCalculationVectorScaler(builder, className, "Subtract", "Subtract", n);
        AddCalculationScalerVector(builder, className, "Subtract", "Subtract", n);

        AddCalculationVectorVector(builder, className, "Multiply", "Multiply", n);
        AddCalculationVectorScaler(builder, className, "Multiply", "Multiply", n);

        AddCalculationVectorVector(builder, className, "Divide", "Divide", n);
        AddCalculationVectorScaler(builder, className, "Divide", "Divide", n);
        AddCalculationScalerVector(builder, className, "Divide", "Divide", n);

        builder.AppendLine();
        builder.AppendLine($"\t\tpublic static double InnerProduct({className} left, {className} right)");
        builder.AppendLine("\t\t{");
        builder.AppendLine("\t\t\tif (left.count != right.count)");
        builder.AppendLine("\t\t\t\tthrow new ArgumentException(\"The count of the vectors must be the same.\");");
        builder.AppendLine();
        builder.AppendLine("\t\t\tvar acc = Vector256<double>.Zero;");
        foreach (var element in GenerateElements(n, 4, x => $"acc = Avx.Add(acc, Avx.Multiply(left.v{x}, right.v{x}))", "; "))
            builder.AppendLine($"\t\t\t{element};");
        builder.AppendLine();
        builder.AppendLine("\t\t\treturn acc.GetElement(0) + acc.GetElement(1) + acc.GetElement(2) + acc.GetElement(3);");
        builder.AppendLine($"\t\t}} // public static double InnerProduct ({className}, {className})");

        builder.AppendLine();
        builder.AppendLine($"\t\tpublic static void Exp({className} vector, {className} result)");
        builder.AppendLine("\t\t{");
        builder.AppendLine("\t\t\tif (!isSupported)");
        builder.AppendLine("\t\t\t\tthrow new NotSupportedException(\"The AVX instruction set is not supported.\");");
        builder.AppendLine();
        foreach (var element in GenerateElements(n, 4, x => $"result.v{x} = MathUtils.Exp(vector.v{x})", "; "))
            builder.AppendLine($"\t\t\t{element};");
        builder.AppendLine($"\t\t}} // public static void Exp ({className}, {className})");

        builder.AppendLine();
        builder.AppendLine($"\t\t#endregion IIntrinsicVector<{className}>");

        builder.AppendLine();
        builder.AppendLine("\t\t#region operators");

        AddOperator(builder, className, "+", "Add", n);
        AddOperator(builder, className, "-", "Subtract", n);
        AddOperator(builder, className, "*", "Multiply", n);
        AddOperator(builder, className, "/", "Divide", n);

        builder.AppendLine();
        builder.AppendLine("\t\t#endregion operators");

        builder.AppendLine($"\t}} // internal partial class {className} : IIntrinsicVector<{className}>");
        builder.AppendLine("} // namespace " + nameSpace);

        return builder.ToString();
    } // private static string Generate (string, string, int)

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

    private static void AddCalculationVectorVector(StringBuilder builder, string className, string method, string avxMethod, int n)
    {
        builder.AppendLine();
        builder.AppendLine($"\t\tpublic static void {method}({className} left, {className} right, {className} result)");
        builder.AppendLine("\t\t{");
        builder.AppendLine("\t\t\tif (left.count != right.count || left.count != result.count)");
        builder.AppendLine("\t\t\t\tthrow new ArgumentException(\"The count of the vectors must be the same.\");");
        builder.AppendLine();
        foreach (var element in GenerateElements(n, 4, x => $"result.v{x} = Avx.{avxMethod}(left.v{x}, right.v{x})", "; "))
            builder.AppendLine($"\t\t\t{element};");
        builder.AppendLine($"\t\t}} // public static void {method}({className}, {className}, {className})");
    } // private static void AddCalculationVectorVector (StringBuilder, string, string, string, int)

    private static void AddCalculationVectorScaler(StringBuilder builder, string className, string method, string avxMethod, int n)
    {
        builder.AppendLine();
        builder.AppendLine($"\t\tunsafe public static void {method}({className} left, double right, {className} result)");
        builder.AppendLine("\t\t{");
        builder.AppendLine("\t\t\tvar arr = stackalloc double[4] { right, right, right, right };");
        builder.AppendLine("\t\t\tvar v_right = Avx.LoadVector256(arr);");
        builder.AppendLine();
        foreach (var element in GenerateElements(n, 4, x => $"result.v{x} = Avx.{avxMethod}(left.v{x}, v_right)", "; "))
            builder.AppendLine($"\t\t\t{element};");
        builder.AppendLine($"\t\t}} // unsafe public static void {method}({className}, double, {className})");
    } // private static void AddCalculationVectorScaler (StringBuilder, string, string, string, int)

    private static void AddCalculationScalerVector(StringBuilder builder, string className, string method, string avxMethod, int n)
    {
        builder.AppendLine();
        builder.AppendLine($"\t\tunsafe public static void {method}(double left, {className} right, {className} result)");
        builder.AppendLine("\t\t{");
        builder.AppendLine("\t\t\tvar arr = stackalloc double[4] { left, left, left, left };");
        builder.AppendLine("\t\t\tvar v_left = Avx.LoadVector256(arr);");
        builder.AppendLine();
        foreach (var element in GenerateElements(n, 4, x => $"result.v{x} = Avx.{avxMethod}(v_left, right.v{x})", "; "))
            builder.AppendLine($"\t\t\t{element};");
        builder.AppendLine($"\t\t}} // unsafe public static void {method}(double, {className}, {className})");
    } // private static void AddCaluculationScalerVector (StringBuilder, string, string, string, int)

    private static void AddOperator(StringBuilder builder, string className, string op, string method, int n)
    {
        builder.AppendLine();
        builder.AppendLine($"\t\tpublic static {className} operator {op}({className} a, {className} b)");
        builder.AppendLine("\t\t{");
        builder.AppendLine("\t\t\tif (a.count != b.count)");
        builder.AppendLine("\t\t\t\tthrow new ArgumentException(\"The count of the vectors must be the same.\");");
        builder.AppendLine();
        builder.AppendLine($"\t\t\tvar ret = new {className}(a.count << 2);");
        foreach (var element in GenerateElements(n, 4, x => $"ret.v{x} = Avx.{method}(a.v{x}, b.v{x})", "; "))
            builder.AppendLine($"\t\t\t{element};");
        builder.AppendLine();
        builder.AppendLine("\t\t\treturn ret;");
        builder.AppendLine($"\t\t}} // public static {className} operator {op}({className}, {className})");
    } // private static void AddOperator (string, string, int)

    private static int GetCount(GeneratorExecutionContext context, AttributeSyntax attribute)
    {
        var args = attribute.ArgumentList?.Arguments.Cast<AttributeArgumentSyntax>();

        var orderArg = args?.FirstOrDefault()?.Expression
            ?? throw new Exception("Failed to get the order parameter of the model.");
        if (context.Compilation.GetSemanticModel(attribute.SyntaxTree).GetConstantValue(orderArg).Value is not int c)
            throw new Exception("Failed to get the order parameter of the model.");
        return c;
    } // private static int GetCount (GeneratorExecutionContext, AttributeSyntax)

    /// <summary>
    /// Gets the fully qualified name of the class.
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="klass">The class declaration syntax.</param>
    /// <returns>The fully qualified name of the class.</returns>
    /// <exception cref="Exception">Failed to get the symbol of the class.</exception>
    private static (string, string) GetFullyQualifiedName(GeneratorExecutionContext context, ClassDeclarationSyntax klass)
    {
        var typeSymbol =
            context.Compilation
                   .GetSemanticModel(klass.SyntaxTree)
                   .GetDeclaredSymbol(klass)
            ?? throw new Exception("Failed to get the symbol of the class.");
        var nameSpace = typeSymbol.ContainingNamespace.ToDisplayString();
        var className = typeSymbol.Name;
        return (nameSpace, className);
    } // protected virtual (string, string) GetFullyQualifiedName (GeneratorExecutionContext, ClassDeclarationSyntax)
} // internal sealed class AvxVectorGenerator : ISourceGenerator
