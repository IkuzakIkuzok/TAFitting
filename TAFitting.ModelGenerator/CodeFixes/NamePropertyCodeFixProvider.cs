
// (c) 2024 Kazuki Kohzuki

using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Immutable;
using System.Composition;
using TAFitting.ModelGenerator.Analyzers;

namespace TAFitting.ModelGenerator.CodeFixes;

/// <summary>
/// Provides a code fix for adding the Name property to a class declaration.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixProvider))]
[Shared]
internal sealed class NamePropertyCodeFixProvider : CodeFixProvider
{
    override public ImmutableArray<string> FixableDiagnosticIds
        => [AttributeUsageAnalyzer.NoNameErrId, AttributeUsageAnalyzer.MultipleNameErrId];

    override public FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

    override public async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var nodes = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<ClassDeclarationSyntax>();
        if (nodes is null) return;
        if (!nodes.Any()) return;
        var node = nodes.First();

        var hasNameProperty = node.Members.OfType<PropertyDeclarationSyntax>().Any(p => p.Identifier.Text == "Name");

        if (hasNameProperty)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove 'Name' property from the class",
                    createChangedDocument: c => RemoveNameProperty(context.Document, node, c),
                    equivalenceKey: nameof(RemoveNameProperty)
                ),
                diagnostic
            );

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove 'Name' argument from the attribute",
                    createChangedDocument: c => RemoveNameArgument(context.Document, node, c),
                    equivalenceKey: nameof(RemoveNameArgument)
                ),
                diagnostic
            );
        }
        else
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add 'Name' argument to the attribute",
                    createChangedDocument: c => AddNameArgument(context.Document, node, c),
                    equivalenceKey: nameof(AddNameArgument)
                ),
                diagnostic
            );
        }
    } // override public Task RegisterCodeFixesAsync (CodeFixContext)

    private static async Task<Document> AddNameArgument(Document document, ClassDeclarationSyntax classDeclarationSyntax, CancellationToken cancellationToken)
    {
        var oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (oldRoot is null) return document;

        var attr = GetAttributeSyntax(classDeclarationSyntax);
        if (attr is null) return document;
        if (attr.Parent is not AttributeListSyntax attrList) return document;

        var name = classDeclarationSyntax.Identifier.Text;
        var nameEqual = SyntaxFactory.NameEquals("Name");
        var nameArgument
            = SyntaxFactory.AttributeArgument(nameEqual, null, SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(name)));
        
        var newAttr = attr.AddArgumentListArguments(nameArgument);
        var newAttrList = attrList.ReplaceNode(attr, newAttr);
        var newRoot = oldRoot.ReplaceNode(attrList, newAttrList);
        return document.WithSyntaxRoot(newRoot);
    } // private static async Task<Document> AddGuidAttribute (Document, ClassDeclarationSyntax, CancellationToken)

    private static async Task<Document> RemoveNameProperty(Document document, ClassDeclarationSyntax classDeclarationSyntax, CancellationToken cancellationToken)
    {
        var oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (oldRoot is null) return document;

        var nameProperty = classDeclarationSyntax.Members.OfType<PropertyDeclarationSyntax>().FirstOrDefault(p => p.Identifier.Text == "Name");
        if (nameProperty is null) return document;

        var newRoot = oldRoot.RemoveNode(nameProperty, SyntaxRemoveOptions.KeepNoTrivia);
        if (newRoot is null) return document;
        return document.WithSyntaxRoot(newRoot);
    } // private static async Task<Document> RemoveNameProperty (Document, ClassDeclarationSyntax, CancellationToken)

    private static async Task<Document> RemoveNameArgument(Document document, ClassDeclarationSyntax classDeclarationSyntax, CancellationToken cancellationToken)
    {
        var oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (oldRoot is null) return document;

        var attr = GetAttributeSyntax(classDeclarationSyntax);
        if (attr is null) return document;
        if (attr.Parent is not AttributeListSyntax attrList) return document;

        var nameNode = attr.ArgumentList?.Arguments.FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == "Name");
        if (nameNode is null) return document;
        var newArgs = attr.ArgumentList?.RemoveNode(nameNode, SyntaxRemoveOptions.KeepNoTrivia);
        var newAttr = attr.WithArgumentList(newArgs);
        var newAttrList = attrList.ReplaceNode(attr, newAttr);
        var newRoot = oldRoot.ReplaceNode(attrList, newAttrList);
        return document.WithSyntaxRoot(newRoot);
    } // private static async Task<Document> RemoveGuidAttribute (Document, ClassDeclarationSyntax, CancellationToken)

    private static AttributeSyntax? GetAttributeSyntax(ClassDeclarationSyntax classDeclarationSyntax)
        => classDeclarationSyntax.AttributeLists
            .SelectMany(al => al.Attributes)
            .FirstOrDefault(a => AttributeUsageAnalyzer.Attributes.Contains(a.Name.ToFullString().NormalizeAttributeName()));

} // internal sealed class NamePropertyCodeFixProvider : CodeFixProvider
