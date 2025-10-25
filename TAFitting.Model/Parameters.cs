
// (c) 2025 Kazuki KOHZUKI

using System.Collections;
using System.Runtime.CompilerServices;

namespace TAFitting.Model;

[CollectionBuilder(typeof(Parameters), nameof(Create))]
public sealed class Parameters(Parameter[] parameters) : IReadOnlyList<Parameter>
{
    private readonly Parameter[] parameters = parameters;

    /// <inheritdoc/>
    public Parameter this[int index]
        => this.parameters[index];

    /// <inheritdoc/>
    public int Count
        => this.parameters.Length;

    public static Parameters Create(ReadOnlySpan<Parameter> parameters) =>
        new([.. parameters]);

    /// <inheritdoc/>
    public IEnumerator<Parameter> GetEnumerator()
        => ((IEnumerable<Parameter>)this.parameters).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => this.parameters.GetEnumerator();
} // public sealed class Parameters : IReadOnlyList<Parameter>
