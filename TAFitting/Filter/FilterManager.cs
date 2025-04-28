
// (c) 2025 Kazuki Kohzuki

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Loader;

namespace TAFitting.Filter;

/// <summary>
/// Manages filters.
/// </summary>
internal static class FilterManager
{
    private static readonly Dictionary<string, string> category_map = new()
    {
        { "Fourier", "Fourier denoising (specific)" },
        { "FourierAuto", "Fourier denoising (automatic)" },
        { "LinearCombination", "Linear combination" },
        { "SavitzkyGolay", "Savitzky\u2013Golay" },
    };

    private static readonly Dictionary<Guid, FilterItem> filters = [];

    /// <summary>
    /// Occurs when the filters have changed.
    /// </summary>
    internal static event EventHandler? FiltersChanged;

    /// <summary>
    /// Gets the filters.
    /// </summary>
    internal static IReadOnlyDictionary<Guid, FilterItem> Filters => filters;

    /// <summary>
    /// Gets the default filter.
    /// </summary>
    /// <value>
    /// An instance of the default filter, or <see langword="null"/> if not available.
    /// </value>
    internal static IFilter? DefaultFilter
    {
        get
        {
            var id = Program.DefaultFilter;
            return filters.TryGetValue(id, out var filter) ? filter.Instance : null;
        }
    }

    static FilterManager()
    {
        Load(Assembly.GetExecutingAssembly());

        var modelsDirectory = Path.Combine(Program.AppLocation, "filters");
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

    private static void AddType(Type type)
    {
        var guid = type.GUID;
        if (filters.ContainsKey(guid)) return;

        if (type.IsInterface || type.IsAbstract) return;
        if (!typeof(IFilter).IsAssignableFrom(type)) return;

        var simdAttrs = type.GetCustomAttributes<EquivalentSIMDAttribute>();
        if (!simdAttrs.Any()) return;

        var simdAttr = simdAttrs.FirstOrDefault(CheckSIMDSupport);

        if (!TryGetFilterInstance(type, out var filter)) return;

        var simdType = simdAttr?.SIMDType;
        _ = TryGetFilterInstance(simdType, out var simdFilter);

        var category = GetCategory(type);
        filters.Add(guid, new FilterItem(filter, simdFilter, category));
        FiltersChanged?.Invoke(null, EventArgs.Empty);
    } // private static void AddType (Type)

    private static string GetCategory(Type type)
    {
        var category = type.Namespace?.Split('.').Last() ?? string.Empty;
        return category_map.TryGetValue(category, out var value) ? value : category;
    } // private static string GetCategory (Type)

    private static bool TryGetFilterInstance(Type? type, [NotNullWhen(true)] out IFilter? filter)
    {
        if (type is null)
        {
            filter = null;
            return false;
        }

        try
        {
            filter = Activator.CreateInstance(type) as IFilter;
            return filter is not null;
        }
        catch
        {
            filter = null;
            return false;
        }
    } // private static bool TryGetFilterInstance (Type?, out IFilter?)

    private static bool CheckSIMDSupport(EquivalentSIMDAttribute simdAttr)
    {
        var type = simdAttr.SIMDType;
        if (type is null) return true;  // No SIMD support, which is always OK.

        if (type.IsInterface || type.IsAbstract) return false;
        if (!typeof(IFilter).IsAssignableFrom(type)) return false;
        return CheckSIMDSupport(simdAttr.SIMDRequirements);
    } // private static bool CheckSIMDSupport (EquivalentSIMDAttribute)

    private static bool CheckSIMDSupport(SIMDRequirements requirements)
    {
        if (!Vector256<double>.IsSupported) return false;

        if (requirements == SIMDRequirements.None) return true;
        if (requirements.HasFlag(SIMDRequirements.Avx) && !Avx.IsSupported) return false;
        if (requirements.HasFlag(SIMDRequirements.AvxX64) && !Avx.X64.IsSupported) return false;
        if (requirements.HasFlag(SIMDRequirements.Avx2) && !Avx2.IsSupported) return false;
        if (requirements.HasFlag(SIMDRequirements.Avx2X64) && !Avx2.X64.IsSupported) return false;
        if (requirements.HasFlag(SIMDRequirements.Sse) && !Sse.IsSupported) return false;
        if (requirements.HasFlag(SIMDRequirements.SseX64) && !Sse.X64.IsSupported) return false;
        if (requirements.HasFlag(SIMDRequirements.Sse2) && !Sse2.IsSupported) return false;
        if (requirements.HasFlag(SIMDRequirements.Sse2X64) && !Sse2.X64.IsSupported) return false;
        if (requirements.HasFlag(SIMDRequirements.Sse3) && !Sse3.IsSupported) return false;
        if (requirements.HasFlag(SIMDRequirements.Sse3X64) && !Sse3.X64.IsSupported) return false;
        if (requirements.HasFlag(SIMDRequirements.Ssse3) && !Ssse3.IsSupported) return false;
        if (requirements.HasFlag(SIMDRequirements.Ssse3X64) && !Ssse3.X64.IsSupported) return false;
        if (requirements.HasFlag(SIMDRequirements.Sse41) && !Sse41.IsSupported) return false;
        if (requirements.HasFlag(SIMDRequirements.Sse41X64) && !Sse41.X64.IsSupported) return false;
        if (requirements.HasFlag(SIMDRequirements.Sse42) && !Sse42.IsSupported) return false;
        if (requirements.HasFlag(SIMDRequirements.Sse42X64) && !Sse42.X64.IsSupported) return false;
        return true;
    } // private static bool CheckSIMDSupport (SIMDRequirements)
} // internal static class FilterManager
