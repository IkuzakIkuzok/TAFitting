
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.ModelGenerator;

/// <summary>
/// Base class for model series generators.
/// </summary>
internal abstract class ModelGeneratorBase : ISourceGenerator
{
    /// <summary>
    /// Gets the GUIDs of the models to generate.
    /// </summary>
    abstract protected Dictionary<int, string> Guids { get; }

    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        foreach (var m in this.Guids)
        {
            var n = m.Key;
            var guid = m.Value;
            var source = Generate(n, guid);
            context.AddSource($"{GetClassName(n)}.g.cs", source);
        }
    } // public void Execute (GeneratorExecutionContext)

    /// <summary>
    /// Generates the source code of the model.
    /// </summary>
    /// <param name="n">The order parameter of the model.</param>
    /// <param name="guid">The GUID of the model.</param>
    /// <returns>The source code of the model.</returns>
    abstract protected string Generate(int n, string guid);

    /// <summary>
    /// Gets the class name of the model.
    /// </summary>
    /// <param name="n">The order parameter of the model.</param>
    /// <returns>The class name of the model.</returns>
    abstract protected string GetClassName(int n);

    /// <summary>
    /// Gets the suffix of the number.
    /// </summary>
    /// <param name="n">The number.</param>
    /// <returns>The suffix of the number.</returns>
    protected static string GetSuffix(int n)
        => (n % 10) switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th",
        };
} // internal abstract class ModelGeneratorBase : ISourceGenerator
