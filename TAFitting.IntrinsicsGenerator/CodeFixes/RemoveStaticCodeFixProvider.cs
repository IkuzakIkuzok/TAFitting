
// (c) 2024 Kazuki Kohzuki

using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Immutable;
using System.Composition;
using TAFitting.IntrinsicsGenerator.Analyzers;

namespace TAFitting.IntrinsicsGenerator.CodeFixes;

/// <summary>
/// Provides a code fix for removing the static modifier from a class declaration.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveStaticCodeFixProvider))]
[Shared]
internal sealed class RemoveStaticCodeFixProvider : CodeFixProvider
{
    override public ImmutableArray<string> FixableDiagnosticIds
        => [AttributeUsageAnalyzer.StaticErrId];

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
                title: "Remove static modifier",
                createChangedDocument: c => RemoveStaticModifier(context.Document, node, c),
                equivalenceKey: nameof(RemoveStaticCodeFixProvider)
            ),
            diagnostic
        );
    } // override public Task RegisterCodeFixesAsync (CodeFixContext)


    /// <summary>
    /// Removes the static modifier from the class declaration.
    /// </summary>
    /// <param name="document">The document to be modified.</param>
    /// <param name="classDeclarationSyntax">The class declaration syntax to be modified.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The modified document.</returns>
    private static async Task<Document> RemoveStaticModifier(Document document, ClassDeclarationSyntax classDeclarationSyntax, CancellationToken cancellationToken)
    {
        var modifiers = classDeclarationSyntax.Modifiers;
        if (!modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) return document;

        var oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (oldRoot is null) return document;

        var newDeclaration = classDeclarationSyntax.ReplaceToken(modifiers.First(m => m.IsKind(SyntaxKind.StaticKeyword)), SyntaxFactory.Token(SyntaxKind.None));
        var newRoot = oldRoot.ReplaceNode(classDeclarationSyntax, newDeclaration.WithLeadingTrivia(classDeclarationSyntax.GetLeadingTrivia()));
        return document.WithSyntaxRoot(newRoot);
    } // private static async Task<Document> RemoveStaticModifier (Document, ClassDeclarationSyntax, CancellationToken)
} // internal sealed class RemoveStaticCodeFixProvider : CodeFixProvider
