
// (c) 2024 Kazuki Kohzuki

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TAFitting.ModelGenerator;

internal sealed class AttributeSyntaxReceiver : ISyntaxReceiver
{
    private readonly List<AttributeDeclarationItem<ClassDeclarationSyntax>> models;

    internal IReadOnlyCollection<string> AttributeName { get; }

    internal IReadOnlyList<AttributeDeclarationItem<ClassDeclarationSyntax>> Models => this.models;

    internal AttributeSyntaxReceiver(string attributeName)
    {
        this.models = [];

        var arr = new string[2];

        var className = attributeName.Split('.').Last();
        arr[0] = className;
        if (className.EndsWith("Attribute"))
            arr[1] = className[..^9];
        else
            arr[1] = className + "Attribute";

        this.AttributeName = arr;
    } // internal AttributeSyntaxReceiver (string)

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDeclarationSyntax) return;
        if (classDeclarationSyntax.AttributeLists.Count == 0) return;

        var attrs = classDeclarationSyntax.AttributeLists.SelectMany(al => al.Attributes);
        var attr = attrs.FirstOrDefault(a => this.AttributeName.Contains(a.Name.ToFullString().Split('.').Last()));
        if (attr is null) return;
        this.models.Add(new(classDeclarationSyntax, attr));
    } // public void OnVisitSyntaxNode (SyntaxNode)
} // internal sealed class AttributeSyntaxReceiver : ISyntaxReceiver
