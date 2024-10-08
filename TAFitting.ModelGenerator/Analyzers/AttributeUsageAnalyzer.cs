
// (c) 2024 Kazuki Kohzuki

using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using TAFitting.ModelGenerator.Generators;

namespace TAFitting.ModelGenerator.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class AttributeUsageAnalyzer : DiagnosticAnalyzer
{
    internal const string GuidErrId = "TA0001";
    internal const string MultipleErrId = "TA0002";
    internal const string PartialErrId = "TA0003";
    internal const string StaticErrId = "TA0004";
    internal const string NoNameErrId = "TA0101";
    internal const string MultipleNameErrId = "TA0102";

#pragma warning disable RS2008

    private static readonly DiagnosticDescriptor GuidErr = new(
        id                : GuidErrId,
        title             : "Missing GUID attribute",
        messageFormat     : "The class with '{0}' must have a GUID attribute",
        category          : "Usage",
        defaultSeverity   : DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor PartialErr = new(
        id                : PartialErrId,
        title             : "Missing partial modifier",
        messageFormat     : "The class with '{0}' must be partial",
        category          : "Usage",
        defaultSeverity   : DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor MultipleErr = new(
        id                : MultipleErrId,
        title             : "Multiple model attribute",
        messageFormat     : "A class can have only one model attribute",
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

    private static readonly DiagnosticDescriptor NoNameErr = new(
        id                : NoNameErrId,
        title             : "Missing Name property",
        messageFormat     : "The model must have a Name property",
        category          : "Usage",
        defaultSeverity   : DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor MultipleNameErr = new(
        id                : MultipleNameErrId,
        title             : "Multiple Name properties",
        messageFormat     : "The model must have only one Name property",
        category          : "Usage",
        defaultSeverity   : DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    private static readonly string[] attributes;

    static AttributeUsageAnalyzer()
    {
        attributes = [
            AttributesGenerator.ExponentialModelName,
            AttributesGenerator.PolynomialModelName,
        ];
    } // cctor ()

    override public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [GuidErr, PartialErr, MultipleErr, StaticErr, NoNameErr, MultipleNameErr];

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
        var modelAttrs = attrs.Where(a => attributes.Contains(a.GetGetFullyQualifiedName(context))).ToArray();
        if (modelAttrs.Length == 0) return;
        if (modelAttrs.Length > 1)
        {
            foreach (var a in modelAttrs)
                context.ReportDiagnostic(Diagnostic.Create(MultipleErr, a.GetLocation()));
            return;
        }

        var attr = modelAttrs.First();

        var attrName = attr.GetGetFullyQualifiedName(context).Split('.').Last();

        var hasGuid = attrs.Any(a => a.GetGetFullyQualifiedName(context) == "GuidAttribute");
        if (!hasGuid)
            context.ReportDiagnostic(Diagnostic.Create(GuidErr, attr.GetLocation(), attrName));

        var modifiers = syntax.Modifiers.Select(m => m.Text);
        var isPartial = modifiers.Contains("partial");
        if (!isPartial)
            context.ReportDiagnostic(Diagnostic.Create(PartialErr, syntax.Identifier.GetLocation(), attrName));

        var isStatic = modifiers.Contains("static");
        if (isStatic)
            context.ReportDiagnostic(Diagnostic.Create(StaticErr, syntax.Identifier.GetLocation(), attrName));

        var nameArg = attr.ArgumentList?.Arguments.FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == "Name");
        var nameProp = syntax.Members.OfType<PropertyDeclarationSyntax>().FirstOrDefault(p => p.Identifier.Text == "Name");
        var hasNameArg = nameArg is not null;
        var hasNameProp = nameProp is not null;
        if (!hasNameArg && !hasNameProp)
            context.ReportDiagnostic(Diagnostic.Create(NoNameErr, attr.GetLocation(), attrName));
        if (hasNameArg && hasNameProp)
            context.ReportDiagnostic(Diagnostic.Create(MultipleNameErr, nameProp!.GetLocation(), attrName));
    } // private static void AnalyzeClass (SyntaxNodeAnalysisContext)
} // internal sealed class AttributeUsageAnalyzer : DiagnosticAnalyzer
