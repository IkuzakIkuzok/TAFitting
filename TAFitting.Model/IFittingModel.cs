
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Model;

/// <summary>
/// Represents a fitting model.
/// </summary>
public interface IFittingModel
{
    /// <summary>
    /// Gets the name of the model.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the model.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the parameter list of the model.
    /// </summary>
    public IReadOnlyList<Parameter> Parameters { get; }

    /// <summary>
    /// Gets a function based on the current model with the specified parameters.
    /// </summary>
    /// <param name="parameters">The parameters of the model.</param>
    /// <returns>A function based on the current model with the specified <paramref name="parameters"/>.</returns>
    public Func<double, double> GetFunction(IList<double> parameters);
} // public interface IFittingModel
