﻿
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.ModelGenerator;

[Generator]
internal sealed class PolynomialGenerator : ModelGeneratorBase
{
    private static readonly Dictionary<int, string> guids = new()
    {
        { 1, "9C488607-39F4-48EE-ADDF-E15CB94FF86F" },
        { 2, "99C057E2-D53E-4110-9610-FEF403D75527" },
        { 3, "88F0E853-73C6-450B-B5BE-F37B26196D37" },
        { 4, "03980EB3-7963-4EC3-91D8-A436DAECE02D" },
        { 5, "C7773F3D-44D1-48B0-BF58-746B58433CC0" },
        { 6, "82F9F05F-91FE-46B4-9CC2-3F792DF5DB83" },
        { 7, "6E27A0B6-51BF-4D65-918F-CC4B0FDADF60" },
        { 8, "F68E43C2-6417-480C-BEB4-898B7F335757" },
        { 9, "913359D9-FA8A-43FF-AEFD-4C2E48DD5A28" },
    };

    protected override Dictionary<int, string> Guids
        => guids;

    override protected string GetClassName(int n)
        => $"Polynomial{n}";

    override protected string Generate(int n, string guid)
    {
        var builder = new StringBuilder();

        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine();
        builder.AppendLine("using System.Runtime.InteropServices;");
        builder.AppendLine();
        builder.AppendLine("namespace TAFitting.Model.Polynomial;");
        builder.AppendLine();

        builder.AppendLine($"/// <summary>");
        builder.AppendLine($"/// Represents a {n}{GetSuffix(n)}-order polynomial model.");
        builder.AppendLine($"/// </summary>");
        builder.AppendLine($"[Guid(\"{guid}\")]");
        builder.AppendLine($"internal sealed class Polynomial{n} : IFittingModel, IAnalyticallyDifferentiable");
        builder.AppendLine("{");

        #region fields

        builder.AppendLine("\tprivate static readonly Parameter[] parameters = [");
        for (var i = 0; i <= n; i++)
            builder.AppendLine($"\t\tnew() {{ Name = \"A{i}\", InitialValue = {Math.Pow(-1, i)}e+{n - i}, IsMagnitude = true }},");
        builder.AppendLine("\t];");

        #endregion fields

        #region properties

        builder.AppendLine();
        builder.AppendLine("\t/// <inheritdoc/>");
        builder.AppendLine("\tpublic string Name => \"Poly" + n + "\";");

        builder.AppendLine();
        builder.AppendLine("\t/// <inheritdoc/>");
        builder.AppendLine("\tpublic string Description => \"" + n + GetSuffix(n) + "-order polynomial model\";");

        builder.AppendLine();
        builder.AppendLine("\t/// <inheritdoc/>");
        builder.AppendLine(
            "\tpublic string ExcelFormula => \"[A0] + [A1] * $X"
            + string.Concat(Enumerable.Range(2, n - 1).Select(i => $" + [A{i}] * $X^{i}")) + "\";"
        );

        builder.AppendLine();
        builder.AppendLine("\t/// <inheritdoc/>");
        builder.AppendLine("\tpublic IReadOnlyList<Parameter> Parameters => parameters;");

        builder.AppendLine();
        builder.AppendLine("\t/// <inheritdoc/>");
        builder.AppendLine("\tpublic bool XLogScale => false;");

        builder.AppendLine();
        builder.AppendLine("\t/// <inheritdoc/>");
        builder.AppendLine("\tpublic bool YLogScale => false;");

        #endregion properties

        #region methods

        #region GetFunction

        builder.AppendLine();
        builder.AppendLine("\t/// <inheritdoc/>");
        builder.AppendLine("\tpublic Func<double, double> GetFunction(IReadOnlyList<double> parameters)");
        builder.AppendLine("\t{");
        for (var i = 0; i <= n; i++)
            builder.AppendLine($"\t\tvar a{i} = parameters[{i}];");
        builder.AppendLine();

        if (n == 1)
        {
            builder.AppendLine("\t\treturn x => a0 + a1 * x;");
        }
        else if (n == 2)
        {
            builder.AppendLine("\t\treturn x => a0 + a1 * x + a2 * x * x;");
        }
        else
        {
            builder.AppendLine("\t\treturn (x) =>");
            builder.AppendLine("\t\t{");
            for (var i = 1; i <= n; i++)
            {
                if (i == 1)
                    builder.AppendLine($"\t\t\tvar x1 = x;");
                else
                    builder.AppendLine($"\t\t\tvar x{i} = x{i - 1} * x;");
            }
            static string GetTerm(int i) => $"a{i} * x{i}";
            builder.AppendLine($"\t\t\treturn a0 + {string.Join(" + ", Enumerable.Range(1, n).Select(GetTerm))};");
            builder.AppendLine("\t\t};");
        } // if (n == 1)
        builder.AppendLine("\t} // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)");

        #endregion GetFunction

        #region ComputeDifferentials

        builder.AppendLine();
        builder.AppendLine("\t/// <inheritdoc/>");
        builder.AppendLine("\tpublic double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x)");
        builder.AppendLine("\t{");
        builder.AppendLine("\t\tvar d_a0 = 1.0;");
        for (var i = 1; i <= n; i++)
        {
            if (i == 1)
                builder.AppendLine($"\t\tvar d_a{i} = x;");
            else
                builder.AppendLine($"\t\tvar d_a{i} = d_a{i - 1} * x;");
        }
        builder.Append($"\t\treturn [");
        builder.Append(string.Join(", ", Enumerable.Range(0, n + 1).Select(i => $"d_a{i}")));
        builder.AppendLine("];");
        builder.AppendLine("\t} // public double[] ComputeDifferentials(IReadOnlyList<double> parameters, double x)");

        #endregion ComputeDifferentials

        #endregion methods

        builder.AppendLine($"}} // internal sealed class Polynomial{n} : IFittingModel, IAnalyticallyDifferentiable");

        return builder.ToString();
    } // override protected string Generate (int, string)
} // internal sealed class PolynomialGenerator : ModelGeneratorBase
