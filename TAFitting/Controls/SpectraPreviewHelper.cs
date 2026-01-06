
// (c) 2026 Kazuki Kohzuki

using TAFitting.Controls.Spectra;
using TAFitting.Data;
using TAFitting.Sync;

namespace TAFitting.Controls;

/// <summary>
/// Provides helper methods for managing and displaying preview windows of spectral data, including updating parameters, units, and selected wavelengths across multiple previews.
/// </summary>
internal sealed class SpectraPreviewHelper
{
    private readonly List<SpectraPreviewWindow> previewWindows = [];

    /// <summary>
    /// Gets the list of preview windows for the spectra.
    /// </summary>
    internal IEnumerable<int> SpectraIds => this.previewWindows.Select(w => w.SerialNumber);

    /// <summary>
    /// Displays a preview window for spectral data using the specified model, wavelength, parameters, and optional decay information.
    /// </summary>
    /// <param name="model">The unique identifier of the model to be visualized in the spectra preview.</param>
    /// <param name="wavelength">The wavelength, in nanometers, to be highlighted or analyzed in the preview.</param>
    /// <param name="parametersList">A collection of parameters that configure the spectra preview window.</param>
    /// <param name="decays">Optional decay information used to set time and signal units in the preview. If null, default units are used.</param>
    /// <param name="timeMax">The maximum time value, in appropriate units, to be displayed in the preview's time table.</param>
    internal void ShowPreviewWindow(Guid model, double wavelength, ParametersList parametersList, Decays? decays, double timeMax)
    {
        var preview = new SpectraPreviewWindow(parametersList)
        {
            ModelId = model,
            SelectedWavelength = wavelength,
        };
        if (decays is not null)
        {
            preview.TimeUnit = decays.TimeUnit;
            preview.SignalUnit = decays.SignalUnit;
        }
        SetTimeTable(preview, timeMax);
        this.previewWindows.Add(preview);
        preview.FormClosed += (_, _) => this.previewWindows.Remove(preview);
        preview.Show();
    } // internal void ShowPreviewWindow ()

    /// <summary>
    /// Updates the parameters and model selection for all active preview windows using the specified model and parameter list.
    /// </summary>
    /// <param name="model">The identifier of the model to apply to each preview window.</param>
    /// <param name="parametersList">The list of parameters to assign to each preview window.</param>
    /// <param name="timeMax">The maximum time value to use when updating the time table for each preview window.</param>
    internal void UpdateParameters(Guid model, ParametersList parametersList, double timeMax)
    {
        if (this.previewWindows.Count == 0) return;

        var parameters = parametersList;
        var token = parameters.CurrentStateToken;
        foreach (var preview in this.previewWindows)
        {
            /*
             * If preview.ModelId != tmodel:
             *   1. Set ModelId to Guid.Empty to clear the current model and stop updating spectra.
             *   2. Set parameters. The spectra will not be updated because the model is not set.
             *   3. Set the selected model to update spectra.
             *   Note: Do NOT set the selected model at step 1, because setting the model raises spectra update automatically, which may cause parameters mismatch.
             *  
             * If preview.ModelId == model:
             *   1. Set parameters. This step raises spectra update automatically.
             *   2. Set the selected model. This step does not have any effect, because the setter method returns early.
             */
            if (preview.ModelId != model)
                preview.ModelId = Guid.Empty;
            preview.SetParameters(parameters, token);
            preview.ModelId = model;

            SetTimeTable(preview, timeMax);
        }
    } // internal void UpdateParameters (Guid, ParametersList, double)

    private static void SetTimeTable(SpectraPreviewWindow preview, double timeMax)
        => preview.SetTimeTable(timeMax);

    /// <summary>
    /// Clears the masking cache for all preview windows, ensuring that any cached masking data is removed.
    /// </summary>
    internal void ClearSpectraMaskingCache()
    {
        foreach (var preview in this.previewWindows)
            preview.ClearMaskingCache();
    } // internal void ClearSpectraMaskingCache ()

    /// <summary>
    /// Updates the selected wavelength in the spectra preview windows.
    /// </summary>
    /// <param name="wavelength">The selected wavelength.</param>
    internal void UpdateSelectedWavelength(double wavelength)
    {
        foreach (var preview in this.previewWindows)
            preview.SelectedWavelength = wavelength;
    } // internal void UpdateSelectedWavelength (double)

    /// <summary>
    /// Sets the time unit for all preview windows to the specified value.
    /// </summary>
    /// <param name="unit">The time unit to apply to each preview window.</param>
    internal void UpdateTimeUnit(TimeUnit unit)
    {
        foreach (var preview in this.previewWindows)
            preview.TimeUnit = unit;
    } // internal void UpdateTimeUnit (TimeUnit)

    /// <summary>
    /// Sets the signal unit for all preview windows to the specified value.
    /// </summary>
    /// <param name="unit">The signal unit to apply to each preview window.</param>
    internal void UpdateSignalUnits(SignalUnit unit)
    {
        foreach (var preview in this.previewWindows)
            preview.SignalUnit = unit;
    } // internal void UpdateSignalUnits (SignalUnit)

    internal SpectraSyncObject? GetSyncSpectra(int spectraId)
    {
        var window = this.previewWindows.FirstOrDefault(w => w.SerialNumber == spectraId);
        if (window is null) return null;
        return window.SpectraSyncObject;
    } // internal SpectraSyncObject? GetSyncSpectra (int)
} // internal sealed class SpectraPreviewHelper
