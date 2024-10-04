
// (c) 2024 Kazuki Kohzuki

using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Immutable;
using System.Composition;
using TAFitting.ModelGenerator.Analyzers;

namespace TAFitting.ModelGenerator.CodeFixes;

/// <summary>
/// Provides a code fix for adding the partial modifier to a class declaration.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddPartialCodeFixProvider))]
[Shared]
internal sealed class AddPartialCodeFixProvider : CodeFixProvider
{
    override public ImmutableArray<string> FixableDiagnosticIds
        => [AttributeUsageAnalyzer.PartialErrId];

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

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Add partial modifier",
                createChangedDocument: c => AddPartialModifier(context.Document, node, c),
                equivalenceKey: nameof(AddGuidCodeFixProvider)
            ),
            diagnostic
        );
    } // override public Task RegisterCodeFixesAsync (CodeFixContext)

    /// <summary>
    /// Adds the partial modifier to the class declaration.
    /// </summary>
    /// <param name="document">The document to be modified.</param>
    /// <param name="classDeclarationSyntax">The class declaration syntax to be modified.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The modified document.</returns>
    private static async Task<Document> AddPartialModifier(Document document, ClassDeclarationSyntax classDeclarationSyntax, CancellationToken cancellationToken)
    {
        var modifiers = classDeclarationSyntax.Modifiers;
        if (modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))) return document;

        var oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (oldRoot is null) return document;

        var newDeclaration = classDeclarationSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
        var newRoot = oldRoot.ReplaceNode(classDeclarationSyntax, newDeclaration.WithLeadingTrivia(classDeclarationSyntax.GetLeadingTrivia()));
        return document.WithSyntaxRoot(newRoot);
    } // private static async Task<Document> AddPartialModifier (Document, ClassDeclarationSyntax, CancellationToken)
} // internal sealed class AddPartialCodeFixProvider : CodeFixProvider
