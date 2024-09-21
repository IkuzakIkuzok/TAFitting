
// (c) 2024 Kazuki KOHZUKI

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

namespace TAFitting.Model;

internal static class ModelManager
{
    private static readonly Dictionary<Guid, IFittingModel> models = [];

    internal static event EventHandler? ModelsChanged;

    internal static IReadOnlyDictionary<Guid, IFittingModel> Models => models;

    static ModelManager()
    {
        Load(Assembly.GetExecutingAssembly());
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

            if (!typeof(IFittingModel).IsAssignableFrom(type)) continue;
            if (!TryGetInstance(type, out var model)) continue;
            AddModel(guid, model);
            added = true;
        }

        if (added)
            ModelsChanged?.Invoke(null, EventArgs.Empty);
    } // internal static void Load (Assembly)

    private static bool TryGetInstance(Type type, [NotNullWhen(true)] out IFittingModel? model)
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
    } // private static bool TryGetInstance (Type, out IFittingModel?)

    private static void AddModel(Guid guid, IFittingModel model)
    {
        models.Add(guid, model);
    } // private static void AddModel (Guid, IFittingModel)
} // internal static class ModelManager
