
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
    /// Gets the formula of the model for Excel.
    /// </summary>
    /// <remarks>
    /// - Parameter names must be enclosed in square brackets. e.g., "[A0]"
    /// - Time is notated as "$X"
    /// - An equal sign is not required.
    /// </remarks>
    /// <example>
    /// "[A0] + [A1] * EXP(-$X / [T1])"
    /// for single-component exponential model.
    /// </example>
    public string ExcelFormula { get; }

    /// <summary>
    /// Gets the parameter list of the model.
    /// </summary>
    public IReadOnlyList<Parameter> Parameters { get; }

    /// <summary>
    /// Gets a value indicating whether the X-axis should be shown in logarithmic scale.
    /// </summary>
    public bool XLogScale { get; }

    /// <summary>
    /// Gets a value indicating whether the Y-axis should be shown in logarithmic scale.
    /// </summary>
    public bool YLogScale { get; }

    /// <summary>
    /// Gets a function based on the current model with the specified parameters.
    /// </summary>
    /// <param name="parameters">The parameters of the model.</param>
    /// <returns>A function based on the current model with the specified <paramref name="parameters"/>.</returns>
    public Func<double, double> GetFunction(IReadOnlyList<double> parameters);
} // public interface IFittingModel
