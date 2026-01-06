
// (c) 2026 Kazuki KOHZUKI

using System.Collections.Concurrent;
using TAFitting.Data;
using TAFitting.Data.Solver;
using TAFitting.Data.Solver.SIMD;
using TAFitting.Model;

namespace TAFitting.Controls;

/// <summary>
/// Provides helper methods for performing parameter estimation using the Levenberg-Marquardt algorithm on parameter tables.
/// </summary>
/// <remarks>This class is intended for internal use to facilitate batch parameter fitting across multiple data rows.
/// It manages parallel execution and applies parameter updates to the table after estimation completes.</remarks>
/// <param name="parametersTable">The parameters table containing the data and configuration for parameter estimation operations.</param>
internal class LevenbergMarquardtEstimationHelper(ParametersTable parametersTable)
{
    private readonly ParametersTable parametersTable = parametersTable;

    /// <summary>
    /// Performs parameter estimation for each row in the specified collection using the provided fitting model.
    /// </summary>
    /// <remarks>If all parameters are fixed, the method displays a message and does not perform any estimation.
    /// For large collections, estimation is performed in parallel to improve performance.
    /// Parameter updates are applied to each row after estimation completes.</remarks>
    /// <param name="rows">The collection of parameter table rows to estimate parameters for. Each row represents a set of data to be fitted.</param>
    /// <param name="model">The fitting model to use for parameter estimation. Defines the mathematical model applied to each row's data.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous estimation operation.</returns>
    internal async Task Estimate(ParametersTableRowsEnumerable rows, IFittingModel model)
    {
        var cols =
            this.parametersTable.Columns.OfType<DataGridViewNumericBoxColumn>().ToArray();

        var fixedCols =
            cols.Select((c, i) => (c.Fixed, Index: i))
                .Where(c => c.Fixed)
                .Select(c => c.Index)
                .ToArray();

        if (cols.All(c => c.Fixed))
        {
            FadingMessageBox.Show(
                "All parameters are fixed.\nNothing to fit.",
                0.8, 1000, 75, 0.1
            );
            return;
        }

        var source = rows.ToArray();

        var d = source.First().Decay;
        var x = d.Times;
        var range = d.GetRangeAfterT0();

        ILevenbergMarquardtSolver initSolver() => InitializeSolver(model, x, range, fixedCols);

        var stopRSquared = this.parametersTable.StopUpdateRSquared;
        try
        {
            this.parametersTable.StopUpdateRSquared = true;
            
            if (source.Length >= Program.ParallelThreshold && Program.ParallelThreshold >= 0)
            {
                var results = new ConcurrentDictionary<ParametersTableRow, IReadOnlyList<double>>();
                await Task.Run(() => Parallel.ForEach(source, initSolver, (row, _, solver) =>
                {
                    var parameters = Estimate(solver, row);
                    results.TryAdd(row, [.. parameters]);  // Allocating a new array is necessary because the parameters array is reused.
                    return solver;
                }, (_) => { }));

                // updating parameters on the UI thread
                // Invoke() in each iteration is too slow
                foreach (var (row, parameters) in results)
                {
                    if (row.DataGridView is null) continue;  // Removed from the table during the fitting
                    row.Parameters = parameters;
                }
            }
            else
            {
                var solver = initSolver();
                foreach (var row in rows)
                    row.Parameters = Estimate(solver, row);
            }
        }
        finally
        {
            this.parametersTable.StopUpdateRSquared = stopRSquared;
        }
    } // internal async Task Estimate (ParametersTableRowsEnumerable, IFittingModel)

    private static ILevenbergMarquardtSolver InitializeSolver(IFittingModel model, IReadOnlyList<double> x, Range range, int[] fixedCols)
    {
        if (Program.Config.SolverConfig.UseSIMD && AvxVector.IsSupported && model is IVectorizedModel vectorized)
            return new LevenbergMarquardtSIMD(vectorized, x, range, fixedCols) { MaxIteration = Program.MaxIterations };
        return new LevenbergMarquardt(model, x, range, fixedCols) { MaxIteration = Program.MaxIterations };
    } // private static ILevenbergMarquardtSolver InitializeSolver (IFittingModel, IReadOnlyList<double>, Range, int[])

    /// <summary>
    /// Fits the specified row using the Levenberg-Marquardt algorithm.
    /// </summary>
    /// <param name="solver">The solver to use.</param>
    /// <param name="row">The row to fit.</param>
    /// <returns>The estimated parameters.</returns>
    private static IReadOnlyList<double> Estimate(ILevenbergMarquardtSolver solver, ParametersTableRow row)
    {
        var signals = row.Decay.Signals;
        solver.Initialize(signals, row.Parameters);
        solver.Fit();
        return solver.Parameters;
    } // private static void Estimate (ILevenbergMarquardtSolver, ParametersTableRow)
} // internal class LevenbergMarquardtEstimationHelper
