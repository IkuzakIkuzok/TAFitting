
// (c) 2025 Kazuki Kohzuki

using TAFitting.SourceGeneratorUtils;

namespace EnumSerializer;

/// <summary>
/// Provides utility methods for working with Roslyn symbols.
/// </summary>
internal static class SymbolUtils
{
    /// <summary>
    /// Checks if a named type symbol inherits from a specified base type by its full name.
    /// </summary>
    /// <param name="symbol">The named type symbol to check.</param>
    /// <param name="baseFullName">The full name of the base type to check against.</param>
    /// <returns><see langword="true"/> if the symbol inherits from the specified base type; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method returns <see langword="false"/> if the specified <paramref name="symbol"/> is equal to the base type itself.
    /// </remarks>
    internal static bool CheckInheritance(INamedTypeSymbol symbol, string baseFullName)
    {
        var current = symbol;
        while (current.BaseType is not null)
        {
            if (current.BaseType.GetFullName() == baseFullName) return true;
            current = current.BaseType;
        }
        return false;
    } // internal static bool CheckInheritance (INamedTypeSymbol, string)
} // internal static class RoslynUtils
