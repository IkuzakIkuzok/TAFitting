﻿
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.ModelGenerator.Generators;

/// <summary>
/// Base class for model series generators.
/// </summary>
internal abstract class ModelGeneratorBase : ISourceGenerator
{
    /// <summary>
    /// Gets the name of the attribute to generate the model.
    /// </summary>
    abstract protected string AttributeName { get; }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new AttributeSyntaxReceiver(this.AttributeName));
    } // public void Initialize (GeneratorInitializationContext)

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not AttributeSyntaxReceiver receiver) return;

        foreach (var declaration in receiver.Models)
        {
            try
            {
                var klass = declaration.Target;
                var attr = declaration.Attribute;

                (var nameSpace, var className) = GetFullyQualifiedName(context, klass);
                GetArguments(context, attr, out var order, out var name);
                var source = Generate(nameSpace, className, order, name);
                var fileName = $"{className}.g.cs";
                context.AddSource(fileName, source);
            }
            catch
            {

            }
        }
    } // public void Execute (GeneratorExecutionContext)

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
    /// Gets the arguments of the model from the attribute.
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="attr">The attribute.</param>
    /// <param name="order">When this method returns, contains the order parameter of the model.</param>
    /// <param name="name">When this method returns, contains the name parameter of the model if it exists; otherwise, <see langword="null"/>.</param>
    /// <exception cref="Exception">
    /// Failed to get the order parameter of the model.
    /// -or-
    /// Failed to get the name parameter of the model.
    /// </exception>
    protected virtual void GetArguments(GeneratorExecutionContext context, AttributeSyntax attr, out int order, out string? name)
    {
        var args = attr.ArgumentList?.Arguments.Cast<AttributeArgumentSyntax>();

        var orderArg = args?.FirstOrDefault()?.Expression
            ?? throw new Exception("Failed to get the order parameter of the model.");
        if (context.Compilation.GetSemanticModel(attr.SyntaxTree).GetConstantValue(orderArg).Value is not int o)
            throw new Exception("Failed to get the order parameter of the model.");
        order = o;

        name = null;
        if (args.Count() < 2) return;

        var nameArgs = args.Where(args => args.NameEquals?.Name.Identifier.Text == "Name");
        if (nameArgs.Any())
        {
            var nameArg = nameArgs.First().Expression;
            if (context.Compilation.GetSemanticModel(attr.SyntaxTree).GetConstantValue(nameArg).Value is not string n)
                throw new Exception("Failed to get the name parameter of the model.");
            name = n;
        }
    } // protected virtual void GetArguments (GeneratorExecutionContext, AttributeSyntax, out int, out string?)

    /// <summary>
    /// Gets the fully qualified name of the class.
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="klass">The class declaration syntax.</param>
    /// <returns>The fully qualified name of the class.</returns>
    /// <exception cref="Exception">Failed to get the symbol of the class.</exception>
    protected virtual (string, string) GetFullyQualifiedName(GeneratorExecutionContext context, ClassDeclarationSyntax klass)
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
} // internal abstract class ModelGeneratorBase : ISourceGenerator
