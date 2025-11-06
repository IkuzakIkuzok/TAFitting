
// (c) 2025 Kazuki Kohzuki

using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

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
        {
            var attrClass = attribute.AttributeClass;
            if (attrClass is null) continue;
            var attrname = GetFullName(attrClass);
            if (attrname != SerializableAttributeFullName) continue;

            var args = attribute.ConstructorArguments;
            if (args.Length == 0) continue;
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
    } // private static void AnalyzeEnum　(SyntaxNodeAnalysisContext)

    private static string GetFullName(INamedTypeSymbol? symbol)
        => symbol is null ? string.Empty : symbol.ContainingNamespace.ToDisplayString() + "." + symbol.Name;

    private static bool CheckInheritance(INamedTypeSymbol symbol, string baseFullName)
    {
        var current = symbol;
        while (current.BaseType is not null)
        {
            if (GetFullName(current.BaseType) == baseFullName) return true;
            current = current.BaseType;
        }
        return false;
    } // private static bool CheckInheritance (INamedTypeSymbol, string)
} // internal sealed class AttributeUsageAnalyzer : DiagnosticAnalyzer
