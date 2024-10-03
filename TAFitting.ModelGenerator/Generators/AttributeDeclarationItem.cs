
// (c) 2024 Kazuki Kohzuki

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TAFitting.ModelGenerator.Generators;

/// <summary>
/// Represents an attribute declaration item.
/// </summary>
/// <typeparam name="T">The type of the target syntax node.</typeparam>
internal sealed class AttributeDeclarationItem<T> where T : CSharpSyntaxNode
{
    /// <summary>
    /// Gets the target syntax node.
    /// </summary>
    internal T Target { get; }

    /// <summary>
    /// Gets the attribute syntax node.
    /// </summary>
    internal AttributeSyntax Attribute { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AttributeDeclarationItem{T}"/> class.
    /// </summary>
    /// <param name="target">The target syntax node.</param>
    /// <param name="attribute">The attribute syntax node.</param>
    internal AttributeDeclarationItem(T target, AttributeSyntax attribute)
    {
        Target = target;
        Attribute = attribute;
    } // ctor (T, AttributeSyntax)
} // internal sealed class AttributeDeclarationItem<T> where T : CSharpSyntaxNode
