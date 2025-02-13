﻿
// (c) 2024 Kazuki KOHZUKI

using System.Collections.Immutable;
using TAFitting.SourceGeneratorUtils;

namespace TAFitting.ModelGenerator.Generators;

/// <summary>
/// Base class for model series generators.
/// </summary>
internal abstract class ModelGeneratorBase : IIncrementalGenerator
{
    /// <summary>
    /// Gets the name of the attribute to generate the model.
    /// </summary>
    abstract protected string AttributeName { get; }

    /// <summary>
    /// Gets a hint name of the generated file.
    /// </summary>
    abstract protected string FileName { get; }

    /// <summary>
    /// Gets the additional code to be added to the generated file.
    /// </summary>
    /// <remarks>
    /// The additional codes are recommended to be file-scoped.
    /// </remarks>
    abstract protected string AdditionalCode { get; }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var sources = context.SyntaxProvider.ForAttributeWithMetadataName(
            this.AttributeName,
            static (node, token) => true,
            static (context, token) => context
        ).Collect();
        context.RegisterSourceOutput(sources, Execute);
    } // public void Initialize (IncrementalGeneratorInitializationContext)

    private void Execute(SourceProductionContext context, ImmutableArray<GeneratorAttributeSyntaxContext> sources)
    {
        if (sources.Length == 0) return;

        var compilation = sources.First().SemanticModel.Compilation;

        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine();
        builder.AppendLine("using System.Runtime.InteropServices;");
        builder.AppendLine();
        builder.AppendLine(this.AdditionalCode);
        foreach (var type in sources)
        {
            try
            {
                var typeSymbol = (INamedTypeSymbol)type.TargetSymbol;
                var nameSpace = typeSymbol.ContainingNamespace.ToDisplayString();
                var className = typeSymbol.Name;

                var attr = type.Attributes.First(attr => attr.AttributeClass?.Name == this.AttributeName);
                var args = attr.ConstructorArguments;
                if (args.Length == 0) continue;
                var order = args[0].Value is int o ? o : throw new Exception("Failed to get the order parameter of the model.");
                var namedArgs = attr.NamedArguments;
                var name = namedArgs.FirstOrDefault(arg => arg.Key == "Name").Value.Value as string;

                var code = Generate(nameSpace, className, order, name);
                builder.AppendLine(code);
            }
            catch
            {

            }
        }

        context.AddSource(this.FileName, builder.ToString().NormalizeNewLines());
    } // private void Execute (SourceProductionContext, GeneratorAttributeSyntaxContext)

    /// <summary>
    /// Generates the source code of the model.
    /// </summary>
    /// <param name="nameSpace">The namespace of the model.</param>
    /// <param name="className">The class name of the model.</param>
    /// <param name="n">The order parameter of the model.</param>
    /// <param name="name">The name of the model.</param>
    /// <returns>The source code of the model.</returns>
    abstract protected string Generate(string nameSpace, string className, int n, string? name);

    /// <summary>
    /// Gets the suffix of the number.
    /// </summary>
    /// <param name="n">The number.</param>
    /// <returns>The suffix of the number.</returns>
    protected static string GetSuffix(int n)
        => (n % 10) switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th",
        };
} // internal abstract class ModelGeneratorBase : IIncrementalGenerator
