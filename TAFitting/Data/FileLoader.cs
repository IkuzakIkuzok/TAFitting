
// (c) 2025 Kazuki KOHZUKI

using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace TAFitting.Data;

/// <summary>
/// Manages the file loading.
/// </summary>
internal sealed class FileLoader : IEnumerable<KeyValuePair<double, string>>
{
    private readonly string format_ab = Program.AMinusBSignalFormat;
    private readonly string format_b = Program.BSignalFormat;

    private readonly Dictionary<double, string> folders = new();
    private readonly ConcurrentDictionary<double, byte[]> cache_ab = new();
    private readonly ConcurrentDictionary<double, byte[]> cache_b = new();

    /// <summary>
    /// Registers the specified folder and starts loading the data.
    /// </summary>
    /// <param name="folder">The folder to register.</param>
    /// <param name="wavelength">The wavelength corresponding to the <paramref name="folder"/>.</param>
    internal void Register(string folder, double wavelength)
    {
        var basename = Path.GetFileName(folder);
        var name_ab = FileNameHandler.GetFileName(basename, this.format_ab);
        var name_b = FileNameHandler.GetFileName(basename, this.format_b);

        var file_ab = Path.Combine(folder, name_ab);
        var file_b = Path.Combine(folder, name_b);

        if (!File.Exists(file_ab) || !File.Exists(file_b)) return;

        this.folders.Add(wavelength, folder);
        Task.Run(() =>
        {
            Load(file_ab, this.cache_ab, wavelength);
            Load(file_b, this.cache_b, wavelength);
        });
    } // internal void Register (string)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Load(string path, ConcurrentDictionary<double, byte[]> cache, double wavelength)
    {
        var data = File.ReadAllBytes(path);
        cache[wavelength] = data;
    } // private static void Load (string, ConcurrentDictionary<double, byte[]>, double)

    /// <summary>
    /// Gets the A-B signal file data.
    /// </summary>
    /// <param name="wavelength">The wavelength to get the data.</param>
    /// <returns>The A-B file data if loading is completed; otherwise, <see langword="null"/>.</returns>
    internal byte[]? GetAMinusBFileData(double wavelength)
        => GetFileData(wavelength, this.cache_ab);


    /// <summary>
    /// Gets the B file signal data.
    /// </summary>
    /// <param name="wavelength">The wavelength to get the data.</param>
    /// <returns>The B file data if loading is completed; otherwise, <see langword="null"/>.</returns>
    internal byte[]? GetBFileData(double wavelength)
        => GetFileData(wavelength, this.cache_b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte[]? GetFileData(double wavelength, ConcurrentDictionary<double, byte[]> cache)
        => cache.TryGetValue(wavelength, out var data) ? data : null;

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<double, string>> GetEnumerator()
        => this.folders.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => this.folders.GetEnumerator();
} // internal sealed class FileLoader
