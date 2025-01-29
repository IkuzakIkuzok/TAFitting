
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Filter;

internal sealed record FilterItem(IFilter Filter, IFilter? SIMDFilter)
{
    internal IFilter Instance => this.SIMDFilter ?? this.Filter;
}  // internal sealed record FilterItem (IFilter, IFilter?)
