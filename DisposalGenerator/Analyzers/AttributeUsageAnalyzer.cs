
// (c) 2025 Kazuki Kohzuki

using DisposalGenerator.Generators;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using TAFitting.SourceGeneratorUtils;

namespace DisposalGenerator.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class AttributeUsageAnalyzer: DiagnosticAnalyzer
{
    internal const string PartialErrId = "DG0001";
    internal const string StaticErrId  = "DG0002";

#pragma warning disable IDE0079
#pragma warning disable RS2008

    private static readonly DiagnosticDescriptor PartialErr = new(
        id                : PartialErrId,
        title             : "Missing partial modifier",
        messageFormat     : "The class with '{0}' must be partial",
        category          : "Usage",
        defaultSeverity   : DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor StaticErr = new(
        id                : StaticErrId,
        title             : "Static modifier",
        messageFormat     : "The class with '{0}' must not be static",
        category          : "Usage",
        defaultSeverity   : DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    private static readonly string[] attributes;

    internal static IReadOnlyCollection<string> Attributes => attributes;

    static AttributeUsageAnalyzer()
    {
        attributes = [
            AttributesGenerator.AutoDisposalAttributeName,
        ];
    } // cctor ()

    override public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [PartialErr, StaticErr];

    override public void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
    } // public void Initialize (AnalysisContext)

    private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var syntax = (ClassDeclarationSyntax)context.Node;
        if (syntax.AttributeLists.Count == 0) return;

        var attrs = syntax.AttributeLists.SelectMany(al => al.Attributes);
        var vectorAttrs = attrs.Where(a => attributes.Contains(a.GetGetFullyQualifiedName(context))).ToArray();
        if (vectorAttrs.Length == 0) return;

        var attr = vectorAttrs[0];
        var attrName = attr.GetGetFullyQualifiedName(context).Split('.').Last();

        var modifiers = syntax.Modifiers.Select(m => m.Text);
        var isPartial = modifiers.Contains("partial");
        if (!isPartial)
            context.ReportDiagnostic(Diagnostic.Create(PartialErr, syntax.Identifier.GetLocation(), attrName));

        var isStatic = modifiers.Contains("static");
        if (isStatic)
            context.ReportDiagnostic(Diagnostic.Create(StaticErr, syntax.Identifier.GetLocation(), attrName));
    } // private static void AnalyzeClass (SyntaxNodeAnalysisContext)
} // internal sealed class AttributeUsageAnalyzer: DiagnosticAnalyzer
