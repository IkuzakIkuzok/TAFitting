
// (c) 2024 Kazuki KOHZUKI

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

namespace TAFitting.Model;

/// <summary>
/// Manages the fitting models.
/// </summary>
internal static class ModelManager
{
    private static readonly Dictionary<Guid, ModelItem> models = [];
    private static readonly Dictionary<Guid, List<IEstimateProvider>> estimateProviders = [];

    /// <summary>
    /// Occurs when the models are changed.
    /// </summary>
    internal static event EventHandler? ModelsChanged;

    /// <summary>
    /// Gets the models.
    /// </summary>
    internal static IReadOnlyDictionary<Guid, ModelItem> Models => models;

    /// <summary>
    /// Gets the estimate providers.
    /// </summary>
    internal static IReadOnlyDictionary<Guid, List<IEstimateProvider>> EstimateProviders => estimateProviders;

    static ModelManager()
    {
        Load(Assembly.GetExecutingAssembly());

        foreach (var item in Program.Config.ModelConfig.LinearCombinations)
            item.Register();

        var modelsDirectory = Path.Combine(Program.AppLocation, "models");
        if (!Directory.Exists(modelsDirectory)) return;
        foreach (var file in Directory.EnumerateFiles(modelsDirectory, "*.dll"))
        {
            try
            {
                Load(file);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    } // cctor ()

    /// <summary>
    /// Loads models from the specified assembly.
    /// </summary>
    /// <param name="assemblyPath">The path to the assembly.</param>
    internal static void Load(string assemblyPath)
    {
        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
        Load(assembly);
    } // internal static void Load (string

    /// <summary>
    /// Loads models from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    internal static void Load(Assembly assembly)
    {
        var types = assembly.GetTypes();
        foreach (var type in types)
            AddType(type);
    } // internal static void Load (Assembly)

    /// <summary>
    /// Add the specified type to the models if it is a fitting model or an estimate provider.
    /// </summary>
    /// <param name="type">The type.</param>
    private static void AddType(Type type)
    {
        var guid = type.GUID;
        if (models.ContainsKey(guid)) return;

        if (type.IsInterface || type.IsAbstract) return;

        if (typeof(IFittingModel).IsAssignableFrom(type))
        {
            if (TryGetModelInstance(type, out var model))
            {
                AddModel(guid, model);
            }
        }

        if (typeof(IEstimateProvider).IsAssignableFrom(type))
        {
            if (TryGetEstimateProviderInstance(type, out var provider))
            {
                AddEstimateProvider(provider);
            }
        }
    } // private static void AddType (Type)

    /// <summary>
    /// Tries to create an instance of the specified type of the model.
    /// </summary>
    /// <param name="type">The type of the model to create an instance.</param>
    /// <param name="model">When this method returns, contains the model instance, if the model is created successfully; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the model is created successfully; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetModelInstance(Type type, [NotNullWhen(true)] out IFittingModel? model)
    {
        try
        {
            model = Activator.CreateInstance(type) as IFittingModel;
            return model != null;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            model = null;
            return false;
        }
    } // private static bool TryGetModelInstance (Type, out IFittingModel?)

    /// <summary>
    /// Triest to create an instance of the specified type of the estimate provider.
    /// </summary>
    /// <param name="type">The type of the estimate provider to create an instance.</param>
    /// <param name="provider">When this method returns, contains the estimate provider instance, if the provider is created successfully; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the estimate provider is created successfully; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetEstimateProviderInstance(Type type, [NotNullWhen(true)] out IEstimateProvider? provider)
    {
        try
        {
            provider = Activator.CreateInstance(type) as IEstimateProvider;
            return provider != null;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            provider = null;
            return false;
        }
    } // private static bool TryGetEstimateProviderInstance (Type, out IEstimateProvider?)

    private static void AddModel(Guid guid, IFittingModel model)
    {
        var category = model.GetType().Namespace?.Split('.').Last() ?? string.Empty;
        AddModel(guid, model, category);
    } // private static void AddModel (Guid, IFittingModel)

    /// <summary>
    /// Adds the specified model to the models.
    /// </summary>
    /// <param name="guid">The GUID of the model.</param>
    /// <param name="model">The model.</param>
    /// <param name="category">The category of the model.</param>
    internal static void AddModel(Guid guid, IFittingModel model, string category)
    {
        var item = new ModelItem(model, category);
        models.Add(guid, item);
        ModelsChanged?.Invoke(null, EventArgs.Empty);
    } // internal static void AddModel (Guid, IFittingModel, string)

    /// <summary>
    /// Adds the specified estimate provider.
    /// </summary>
    /// <param name="provider">The estimate provider.</param>
    private static void AddEstimateProvider(IEstimateProvider provider)
    {
        foreach (var supported in provider.SupportedModels)
        {
            if (!estimateProviders.ContainsKey(supported))
                estimateProviders.Add(supported, []);
            estimateProviders[supported].Add(provider);
        }
        ModelsChanged?.Invoke(null, EventArgs.Empty);
    } // private static void AddEstimateProvider (IEstimateProvider)

    /// <summary>
    /// Removes the model with the specified GUID.
    /// </summary>
    /// <param name="guid">The GUID of the model.</param>
    internal static void RemoveModel(Guid guid)
    {
        models.Remove(guid);
        ModelsChanged?.Invoke(null, EventArgs.Empty);
    } // internal static void RemoveModel (Guid)
} // internal static class ModelManager
