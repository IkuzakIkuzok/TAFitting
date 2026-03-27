
// (c) 2026 Kazuki Kohzuki

namespace EnumSerializer.Generators;

internal sealed class EnumSerializationInfo
{
    internal INamedTypeSymbol EnumType { get; }

    internal bool CaseSensitive { get; }

    internal EnumSerializationInfo(INamedTypeSymbol enumType, bool caseSensitive)
    {
        this.EnumType = enumType;
        this.CaseSensitive = caseSensitive;
    } // ctor (INamedTypeSymbol, bool)

    internal sealed class EqualityComparer : IEqualityComparer<EnumSerializationInfo>
    {
        public bool Equals(EnumSerializationInfo? x, EnumSerializationInfo? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return SymbolEqualityComparer.Default.Equals(x.EnumType, y.EnumType);
        } // public bool Equals (EnumSerializationInfo?, EnumSerializationInfo?)

        public int GetHashCode(EnumSerializationInfo obj)
            => SymbolEqualityComparer.Default.GetHashCode(obj.EnumType);
    } // internal sealed class EqualityComparer : IEqualityComparer<EnumSerializationInfo>
} // internal sealed class EnumSerializationInfo
