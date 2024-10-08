﻿
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.ModelGenerator.Generators;

[Generator(LanguageNames.CSharp)]
internal sealed class PolynomialGenerator : ModelGeneratorBase
{
    override protected string AttributeName => AttributesGenerator.PolynomialModelName;

    override protected string Generate(string nameSpace, string className, int n, string? name)
    {
        var builder = new StringBuilder();

        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine();
        builder.AppendLine("using System.Runtime.InteropServices;");
        if (!string.IsNullOrEmpty(nameSpace))
        {
            builder.AppendLine();
            builder.AppendLine($"namespace {nameSpace};");
        }

        builder.AppendLine();
        builder.AppendLine($"/// <summary>");
        builder.AppendLine($"/// Represents a {n}{GetSuffix(n)}-order polynomial model.");
        builder.AppendLine($"/// </summary>");
        builder.AppendLine($"internal partial class {className} : IFittingModel, IAnalyticallyDifferentiable");
        builder.AppendLine("{");

        #region fields

        builder.AppendLine("\tprivate static readonly Parameter[] parameters = [");
        for (var i = 0; i <= n; i++)
            builder.AppendLine($"\t\tnew() {{ Name = \"A{i}\", InitialValue = {Math.Pow(-1, i)}e+{n - i}, IsMagnitude = true }},");
        builder.AppendLine("\t];");

        #endregion fields

        #region properties

        if (!string.IsNullOrEmpty(name))
        {
            builder.AppendLine();
            builder.AppendLine("\t/// <inheritdoc/>");
            builder.AppendLine($"\tpublic string Name => \"{name}\";");
        }

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

        #region GetDerivatives

        builder.AppendLine();
        builder.AppendLine("\t/// <inheritdoc/>");
        builder.AppendLine("\tpublic Func<double, double[]> GetDerivatives(IReadOnlyList<double> parameters)");
        builder.AppendLine("\t\t=> Derivatives;");

        builder.AppendLine();
        builder.AppendLine("\tprivate double[] Derivatives(double x)");
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
        builder.AppendLine("\t} // private double[] Derivatives(double x)");

        #endregion GetDerivatives

        #endregion methods

        builder.AppendLine($"}} // internal partial class {className} : IFittingModel, IAnalyticallyDifferentiable");

        return builder.ToString();
    } // override protected string Generate (string, string, int, string?)
} // internal sealed class PolynomialGenerator : ModelGeneratorBase
