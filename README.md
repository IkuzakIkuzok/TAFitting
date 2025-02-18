
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

## Custom models

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

## License

TAFitting is licensed under the MIT License.
