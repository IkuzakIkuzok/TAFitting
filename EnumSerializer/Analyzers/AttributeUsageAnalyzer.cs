
// (c) 2025 Kazuki Kohzuki

using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using TAFitting.SourceGeneratorUtils;
using static EnumSerializer.SymbolUtils;

namespace EnumSerializer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class AttributeUsageAnalyzer : DiagnosticAnalyzer
{
    private const string SerializableAttributeFullName = "EnumSerializer.EnumSerializableAttribute";
    private const string ValueAttributeFullName = "EnumSerializer.SerializeValueAttribute";

    internal const string AttrInheritanceErrId = "ES0001";

#pragma warning disable IDE0079
#pragma warning disable RS2008

    private static readonly DiagnosticDescriptor AttrInheritanceErr = new(
        id                : AttrInheritanceErrId,
        title             : "Invalid parameter inheritance",
        messageFormat     : "The parameter '{0}' must inherit from 'EnumSerializer.SerializeValueAttribute'",
        category          : "Usage",
        defaultSeverity   : DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    override public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [AttrInheritanceErr];

    override public void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeEnum, SyntaxKind.EnumDeclaration);
    } // override public void Initialize (AnalysisContext)

    private static void AnalyzeEnum(SyntaxNodeAnalysisContext context)
    {
        var semanticModel = context.SemanticModel;

        var enumSyntax = (EnumDeclarationSyntax)context.Node;
        var enumSymbol = semanticModel.GetDeclaredSymbol(enumSyntax);
        if (enumSymbol is null) return;

        var attributes = enumSymbol.GetAttributes();

        foreach (var attribute in attributes)
            AnalyzeAttribute(context, attribute, enumSyntax);
    } // private static void AnalyzeEnum　(SyntaxNodeAnalysisContext)

    private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context, AttributeData attribute, EnumDeclarationSyntax enumSyntax)
    {
        var attrClass = attribute.AttributeClass;
        if (attrClass is null) return;
        var attrname = attrClass.GetFullName();
        if (attrname != SerializableAttributeFullName) return;

        var args = attribute.ConstructorArguments;
        if (args.Length == 0) return;
        var argValues = args[0].Values;

        foreach (var argValue in argValues)
        {
            if (argValue.Kind != TypedConstantKind.Type) continue;
            if (argValue.Value is not INamedTypeSymbol c) continue;
            if (CheckInheritance(c, ValueAttributeFullName)) continue;

            var argLocation = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()
                ?? enumSyntax.GetLocation();
            context.ReportDiagnostic(Diagnostic.Create(AttrInheritanceErr, argLocation, c.Name));
        }
    }
} // internal sealed class AttributeUsageAnalyzer : DiagnosticAnalyzer
