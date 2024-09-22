
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Clipboard;

internal sealed class ClipboardRow
{
    internal double Wavelength { get; }

    internal IReadOnlyList<double> Parameters { get; }

    internal ClipboardRow(double wavelength, IReadOnlyList<double> parameters)
    {
        this.Wavelength = wavelength;
        this.Parameters = parameters;
    } // ctor (double, IReadOnlyList<double>)
} // internal sealed class ClipboardRow
