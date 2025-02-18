
# TAFitting

Fitting tool for Transient Absorption (TA) data

## Requirements

- [.NET Desktop runtime 8.0](https://dotnet.microsoft.com/ja-jp/download/dotnet/8.0)
- Windows 10 or later

## Usage

1. Run `TAFitting.exe`
1. Open TA data from folder (for µs-TAS) or from file (for fs-TAS)
1. Select appropriate fitting model
1. Set parameters for each decay
1. Show spectra preview
1. Set time to show spectra
1. Save spectra data

### Data format

TAFitting supports both µs-TAS and fs-TAS data.

#### µs-TAS

For µs-TAS data, the decay of each wavelength is stored in a folder.
The folder should contain the following files:

- 'A' minus 'B' signal data for plotting
- 'B' signal data for determining the time zero

File names of these files are decided based on the containing folder name.
File name format can be changed from `Data` -&gt; `Filename format`.

In the file name format, following rules are available:

- `<BASENAME>`: The name of the containing folder.
- `<BASENAME|old/new>`: Replace `old` with `new` in the base name.
- `<BASENAME|old1/new1|old2/new2>`: Replace `old1` with `new1` and `old2` with `new2` in the base name. Replace patterns can be added as many as you want.
- `<BASENAME|r:pattern/value>`: Replace `pattern` with `value` in the base name using regular expression.

For example, if the folder name is `600nm`, the format `<BASENAME|nm/>-b.csv` will be converted to `600-b.csv`.

Time zero is determind by the minimum value of the 'B' signal data.
Outliers are removed by the Smirnov-Grubbs test, and remaining data are averaged to determine the time zero.
You can change the time zero value after loading the data.

#### fs-TAS

For fs-TAS data, the decay of each wavelength is stored in a single file.
TAFitting only accepts the csv file exported from SurfaceXplorer.
You can use [SXConverter](https://github.com/IkuzakIkuzok/SXConverter) to conver the Ultrafast System (UFS) file to the csv file.

### Filtering

Each dacay data can be filtered for smoothing and denoising.
Followinf filters are available as built-in:

- Savitzky&ndash;Golay filter (Cubic, 15 or 25 points)
- Specific Fourier filter (1 kHz to 5 GHz)
- Automatic Fourier filter (variable cutoff frequency based on the data)

Filters can be added by creating a new class that implements `TAFitting.Filter.IFilter` interface.
See [Custom filters](#custom-filters) section for more details.

#### Savitzky&ndash;Golay filter

The Savitzky&ndash;Golay filter is a smoothing filter based on the least squares method.
The filter assumes that the times are equally spaced.
No check is performed for the time spacing, so the filter may not work properly if the times are not equally spaced.
See [<i>Anal. Chem.</i> <b>1964</b>, 36, 1627&ndash;1639](https://doi.org/10.1021/ac60214a047) for mathematical details.

#### Fourier filter

The Fourier filter is a low-pass filter based on the Fourier transform.
The data is extended to the next power of 2 with appropriate padding before and after the data (e.g., 2500 points to 4096 points).
The extended data is transformed to the frequency domain, and the high-frequency components are removed.
The filtered data is obtained by the inverse Fourier transform.
The filter assumes that the times are equally spaced.
No check is performed for the time spacing, so the filter may not work properly if the times are not equally spaced.

The automatic Fourier filter determines the cutoff frequency based on the time range of the data.
For example, if the time range is 0 to 1 ms, "10%" means 100 µs, which is equivalent to 10 kHz.

### Fitting models

TAFitting supports the following fitting models:

- 1st to 9th order polynomial model
- 1 to 4 component exponential model
- Empirical power-law model
- Linear combination of power-law and exponential model

Models can be added by creating a new class that implements `TAFitting.Model.IFittingModel` interface.
See [Custom models](#custom-models) section for more details.

### Parameters estimation

TAFitting provides a parameter estimation tool for any fitting model.
It uses the Levenberg-Marquardt algorithm to minimize the sum of squared residuals.
You can access the tool from `Fit` in the menu.

Degree-of-freedom adjusted coefficients of determination R<sup>2</sup> are shown in the parameter table.
Generally, R<sup>2</sup> &gt; 0.5 is considered as a good fit, but this value should be used only as a reference.
The R<sup>2</sup> cells are colored based on the value.

### Spectra preview

Spectra preview window can be opend from `Data` -&gt; `Preview spectra`.
You can edit times on the right side of the window.
The preview is updated automatically when you change the parameters on the main window.

### Saving data

You can save the spectra data to a CSV or Excel file.

## Custom features

### Custom models

You can add custom fitting models by creating a new class that implements `TAFitting.Model.IFittingModel` interface.

The interface has the following properties and method:

- `string Name`: The name of the model.
- `string Description`: The description of the model.
- `string ExcelFormula`: The formula of the model for Excel.
- `IReadOnlyList<Parameter> Parameters`: The list of parameters for the model.
- `bool XLogScale`: Whether the x-axis is in log scale.
- `bool YLogScale`: Whether the y-axis is in log scale.
- `Func<double, double> GetFunction(IReadOnlyList<double> parameters)`: The method to get the function of the model with the given parameters.

Each model should have a GUID attribute, which is used to identify the model.

It is strongly recommended that the model also implements `IAnalyticallyDifferentiable`,
because built-in numerical differentiation is computationally expensive.

### Custom filters

You can add custom filters by creating a new class that implements `TAFitting.Filter.IFilter` interface.

The interface has the following properties and method:

- `string Name`: The name of the filter.
- `string Description`: The description of the filter.
- `double[] Apply(double[] data)`: The method to apply the filter to the data.

Each filter should have a GUID attribute, which is used to identify the filter.

Each filter other than SIMD filters must have a EquivalentSIMD to specify the SIMD filter.
If no SIMD filter is available, set the `SIMDType` to `null`.
Required SIMD implementations can be specified by `SIMDRequirements` property.

## Advanced configuration

Some features cannot be configured from the GUI, and you need to edit the configuration file directly.
The configuration file is named `TAFitting.config` and located in the same directory as the executable file.
Inappropriate changes may cause the application to crash or undesired behavior, so be careful when editing the file.

### Filters

#### `appSettings/filter/default-filter`

Specify the default filter to be applied to the data.
The value should be the GUID of the filter.
Do not specifiy the GUID of the SIMD filter.

The default filter can be applied by Ctrl+F (selected row only) or Ctrl+Shift+F (all rows).
It is also applied when the data is loaded iff Auto-fit is set to true.

### Solver for Levenberg-Marquardt algorithm

#### `appSettings/solver/parallel-threshold`

Specify the threshold for parallelization.
If the number of data is greater than or equal to this value, solving is parallelized.
An integer value is acceptable. Meaning of the value is as follows:

- Positive value: The threshold for parallelization.
- 0: Always parallelize.
- Negative value: Never parallelize.

#### `appSettings/solver/maximum-iterations`

Specify the maximum number of iterations for the solver.
A positive integer value is acceptable.
Too small value may cause the solver to converge to a local minimum,
whereas too large value may cause the solver to take too long to converge.

#### `appSettings/solver/use-simd`

Specify whether to use SIMD to compute the values of the model.
A boolean value with all lowercase (i.e., `true` or `false`) is acceptable.
For many cases `true` is recommended as SIMD is much faster than scalar computation.

Note that SIMD computation is not always used even if this value is set to `true`.

#### `appSettings/solver/max-truncate-ratio`

Specify the maximum ratio of the truncated data.
SIMD computation is available only when the number of data points matches the lenghth of the SIMD vector (1024 or 2048).
If the number of data points is greater than the length of the SIMD vector,
the data is truncated, since the later time domain has less effect on the fitting.
The trancation is performed by removing the data points from the end of the data,
as long as the ratio of the truncated data is less than the specified value.
(If the number of data points is less than the length of the SIMD vector, the data is extended by zero padding.)

For example, if the number of data points is 2100, the last 52 points are removed to make the number of data points 2048.
This trancation is valid only when the `max-truncate-ratio` is greater than 0.0254 (= 52 / 2048).
Too small value prevents the SIMD computation, whereas too large value may cause the fitting to be inaccurate as many data points are removed.
Note that the fitting is performed on the data only after the time zero,
and the length of the data for fitting may not be the same as the original data.

## License

TAFitting is licensed under the MIT License.
