﻿
// (c) 2024 Kazuki Kohzuki

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace TAFitting.ModelGenerator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class AttributeUsageAnalyzer : DiagnosticAnalyzer
{
    private const string GuidErrId = "TA0001";
    private const string PartialErrId = "TA0002";

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

    private static readonly string[] attributes;

    static AttributeUsageAnalyzer()
    {
        attributes = [
            AttributesGenerator.ExponentialModelName,
            AttributesGenerator.PolynomialModelName,
        ];
    } // cctor ()

    override public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [GuidErr, PartialErr];

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
        var attr = attrs.FirstOrDefault(a => attributes.Contains(a.GetGetFullyQualifiedName(context)));
        if (attr is null) return;

        var attrName = attr.GetGetFullyQualifiedName(context).Split('.').Last();

        var hasGuid = attrs.Any(a => a.GetGetFullyQualifiedName(context) == "GuidAttribute");
        if (!hasGuid)
            context.ReportDiagnostic(Diagnostic.Create(GuidErr, attr.GetLocation(), attrName));

        var modifiers = syntax.Modifiers.Select(m => m.Text);
        var isPartial = modifiers.Contains("partial");
        if (!isPartial)
            context.ReportDiagnostic(Diagnostic.Create(PartialErr, syntax.Identifier.GetLocation(), attrName));
    } // private static void AnalyzeClass (SyntaxNodeAnalysisContext)
} // internal sealed class AttributeUsageAnalyzer : DiagnosticAnalyzer
