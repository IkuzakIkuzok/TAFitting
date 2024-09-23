
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Model;

/// <summary>
/// Represents a provider of parameters estimates.
/// </summary>
public interface IEstimateProvider
{
    /// <summary>
    /// Gets the name of the model.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the model.
    /// </summary>
    public string Description { get; }

    public IReadOnlyList<Guid> SupportedModels { get; }

    public IReadOnlyList<double> EstimateParameters(IReadOnlyList<double> time, IReadOnlyList<double> signal, Guid modelId);
} // public interface IEstimateProvider
