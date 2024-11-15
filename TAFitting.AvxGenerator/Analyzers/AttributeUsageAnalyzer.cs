
// (c) 2024 Kazuki Kohzuki

using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using TAFitting.AvxGenerator.Generators;
using TAFitting.SourceGeneratorUtils;

namespace TAFitting.AvxGenerator.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class AttributeUsageAnalyzer : DiagnosticAnalyzer
{
    internal const string PartialErrId = "AV0001";
    internal const string StaticErrId = "AV0002";
    internal const string CountErrId = "AV0101";

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

    private static readonly DiagnosticDescriptor CountErr = new(
        id                : CountErrId,
        title             : "Invalid count",
        messageFormat     : "The count must be a multiple of 4",
        category          : "Usage",
        defaultSeverity   : DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    private static readonly string[] attributes;

    internal static IReadOnlyCollection<string> Attributes => attributes;

    static AttributeUsageAnalyzer()
    {
        attributes = [
            AvxVectorAttributesGenerator.AttributeName,
        ];
    } // cctor ()

    override public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [PartialErr, StaticErr, CountErr];

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

        var count = attr.ArgumentList?.Arguments[0].Expression;
        if (count is null) return;
        if (count is not LiteralExpressionSyntax literal || !int.TryParse(literal.Token.ValueText, out var value) || value % 4 != 0)
            context.ReportDiagnostic(Diagnostic.Create(CountErr, count.GetLocation()));
    } // private static void AnalyzeClass (SyntaxNodeAnalysisContext)
} // internal sealed class AttributeUsageAnalyzer : DiagnosticAnalyzer
