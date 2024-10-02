
// (c) 2024 Kazuki Kohzuki

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TAFitting.ModelGenerator;

/// <summary>
/// Provides extension methods for <see cref="SyntaxNode"/>.
/// </summary>
internal static class SyntaxNodeUtils
{
    /// <summary>
    /// Gets the fully qualified name of the attribute.
    /// </summary>
    /// <param name="attribute">The attribute.</param>
    /// <param name="context">The context.</param>
    /// <returns>The fully qualified name.</returns>
    internal static string GetGetFullyQualifiedName(this AttributeSyntax attribute, SyntaxNodeAnalysisContext context)
    {
        var symbol = context.SemanticModel.GetSymbolInfo(attribute).Symbol;
        return symbol?.GetFullyQualifiedName() ?? string.Empty;
    } // internal static string GetGetFullyQualifiedName (AttributeSyntax, SyntaxNodeAnalysisContext)

    /// <summary>
    /// Obtains the fully qualified name of the class.
    /// </summary>
    /// <param name="symbol">The symbol.</param>
    /// <returns>The fully qualified name.</returns>
    /// <remarks><see cref="ISymbol.ToDisplayParts(SymbolDisplayFormat?)"/> with argument <see cref="SymbolDisplayFormat.FullyQualifiedFormat"/>
    /// does NOT work as expected. This problem is already reported on <a href="https://github.com/dotnet/roslyn/issues/50259">GitHub</a>.</remarks>
    internal static string GetFullyQualifiedName(this ISymbol symbol)
    {
        var definition = symbol.OriginalDefinition.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
        if (definition == null) return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var node = definition;
        var names = new List<string>();
        while ((node = node.Parent) != null)
        {
            if (node is NamespaceDeclarationSyntax ns)
                names.Add(ns.Name.ToString());
            if (node is ClassDeclarationSyntax @class)
                names.Add(@class.ChildTokens().Where(token => token.IsKind(SyntaxKind.IdentifierToken)).First().Text);
        }
        names.Reverse();
        return string.Join(".", names);
    } // internal static string GetFullyQualifiedName (this ISymbol)
} // internal static class SyntaxNodeUtils
