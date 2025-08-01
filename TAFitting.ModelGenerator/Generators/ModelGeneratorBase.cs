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
        foreach (var type in sources)
        {
            try
            {
                var typeSymbol = (INamedTypeSymbol)type.TargetSymbol;
                var nameSpace = typeSymbol.ContainingNamespace.ToDisplayString();
                var className = typeSymbol.Name;

                var attr = type.Attributes.First(attr => GetFullName(attr.AttributeClass) == this.AttributeName);
                var args = attr.ConstructorArguments;
                if (args.Length == 0) continue;
                var order = args[0].Value is int o ? o : throw new Exception("Failed to get the order parameter of the model.");
                var namedArgs = attr.NamedArguments;
                var name = namedArgs.FirstOrDefault(arg => arg.Key == "Name").Value.Value as string;

                Generate(builder, nameSpace, className, order, name);
            }
            catch
            {

            }
        }

        context.AddSource(this.FileName, builder.ToString().NormalizeNewLines());
    } // private void Execute (SourceProductionContext, GeneratorAttributeSyntaxContext)

    private static string GetFullName(INamedTypeSymbol? symbol)
        => symbol is null ? string.Empty : symbol.ContainingNamespace.ToDisplayString() + "." + symbol.Name;

    /// <summary>
    /// Generates the source code of the model.
    /// </summary>
    /// <param name="builder"><see cref="StringBuilder"/> to append the generated code.</param>
    /// <param name="nameSpace">The namespace of the model.</param>
    /// <param name="className">The class name of the model.</param>
    /// <param name="n">The order parameter of the model.</param>
    /// <param name="name">The name of the model.</param>
    abstract protected void Generate(StringBuilder builder, string nameSpace, string className, int n, string? name);

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
