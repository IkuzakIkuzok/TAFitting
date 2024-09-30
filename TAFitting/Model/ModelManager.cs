﻿
// (c) 2024 Kazuki KOHZUKI

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

namespace TAFitting.Model;

internal static class ModelManager
{
    private static readonly Dictionary<Guid, ModelItem> models = [];
    private static readonly Dictionary<Guid, List<IEstimateProvider>> estimateProviders = [];

    internal static event EventHandler? ModelsChanged;

    internal static IReadOnlyDictionary<Guid, ModelItem> Models => models;

    internal static IReadOnlyDictionary<Guid, List<IEstimateProvider>> EstimateProviders => estimateProviders;

    static ModelManager()
    {
        Load(Assembly.GetExecutingAssembly());

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

    internal static void Load(string assemblyPath)
    {
        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
        Load(assembly);
    } // internal static void Load (string

    internal static void Load(Assembly assembly)
    {
        var added = false;

        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            var guid = type.GUID;
            if (models.ContainsKey(guid)) continue;

            if (type.IsInterface || type.IsAbstract) continue;

            if (typeof(IFittingModel).IsAssignableFrom(type))
            {
                if (TryGetModelInstance(type, out var model))
                {
                    AddModel(guid, model);
                    added = true;
                }
            }

            if (typeof(IEstimateProvider).IsAssignableFrom(type))
            {
                if (TryGetEstimateProviderInstance(type, out var provider))
                {
                    AddEstimateProvider(provider);
                    added = true;
                }
            }
        }

        if (added)
            ModelsChanged?.Invoke(null, EventArgs.Empty);
    } // internal static void Load (Assembly)

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
        var item = new ModelItem(model);
        models.Add(guid, item);
    } // private static void AddModel (Guid, IFittingModel)

    private static void AddEstimateProvider(IEstimateProvider provider)
    {
        foreach (var supported in provider.SupportedModels)
        {
            if (!estimateProviders.ContainsKey(supported))
                estimateProviders.Add(supported, []);
            estimateProviders[supported].Add(provider);
        }
    } // private static void AddEstimateProvider (IEstimateProvider)
} // internal static class ModelManager
