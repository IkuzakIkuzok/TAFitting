
// (c) 2024 Kazuki KOHZUKI

using TAFitting.Model;

namespace TAFitting.Excel;

internal interface ISpreadSheetWriter
{
    IFittingModel Model { get; init; }

    IReadOnlyList<string> Parameters { get; set; }

    IReadOnlyList<double> Times { get; set; }

    void AddRow(double wavelength, IEnumerable<double> parameters);

    void Write(string path);
} // internal interface ISpreadSheetWriter
