
// (c) 2024-2025 Kazuki Kohzuki

namespace TAFitting.Controls.Spectra;

/// <summary>
/// Represents a table of times.
/// </summary>
[DesignerCategory("Code")]
internal sealed partial class TimeTable : DataGridView
{
    /// <summary>
    /// Gets a value indicating whether the table is updating.
    /// </summary>
    /// <value><see langword="true"/> if the table is updating; otherwise, <see langword="false"/>.</value>
    internal bool Updating { get; private set; } = false;

    /// <summary>
    /// Gets the times.
    /// </summary>
    internal IReadOnlyList<double> Times
        => [.. this.Rows
               .Cast<DataGridViewRow>()
               .Where(row => !row.IsNewRow)
               .Select(row => (double)row.Cells["Time"].Value)
               .Order()];

    /// <summary>
    /// Gets or sets the unit of time.
    /// </summary>
    /// <value>The unit of time.</value>
    internal string Unit
    {
        get => this.Columns["Time"].HeaderText[6..^1];
        set => this.Columns["Time"].HeaderText = $"Time ({value})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeTable"/> class.
    /// </summary>
    internal TimeTable()
    {
        this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.EnableHeadersVisualStyles = false;

        var col = new DataGridViewNumericBoxColumn()
        {
            Name = "Time",
            HeaderText = "Time (µs)",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
        };
        this.Columns.Add(col);
    } // ctor ()

    /// <inheritdoc/>
    override protected void OnSortCompare(DataGridViewSortCompareEventArgs e)
    {
        e.Handled = true;
        e.SortResult = e.CellValue1 is double v1 && e.CellValue2 is double v2
            ? v1.CompareTo(v2)
            : 0;
    } // override protected void OnSortCompare (DataGridViewSortCompareEventArgs)

    /// <summary>
    /// Sets the colors of the rows.
    /// </summary>
    internal void SetColors()
    {
        if (this.Updating) return;

        Sort(this.Columns["Time"], ListSortDirection.Ascending);
        var count = this.Rows.Count - 1;
        var gradient = new ColorGradient(Program.GradientStart, Program.GradientEnd, count);
        for (var i = 0; i < count; i++)
        {
            var row = this.Rows[i];
            row.HeaderCell.Style.BackColor = gradient[i];
        }
    } // internal void SetColors ()

    /// <summary>
    /// Sets the times.
    /// </summary>
    /// <param name="maxTime">The maximum time.</param>
    /// <param name="n">The number of times.</param>
    internal void SetTimes(double maxTime, int n = 5)
    {
        this.Updating = true;
        try
        {
            var times = new double[n];
            var d = Math.Pow(10, Math.Floor(Math.Log10(maxTime)));
            var m = maxTime / d;

            if (m < 5)
            {
                d /= 10;
                m *= 10;
            }

            var t = Math.Pow(2, Math.Floor(Math.Log2(m))) * d;
            for (var i = n - 1; i >= 0; i--)
            {
                times[i] = t;
                t /= 2;
            }

            this.Rows.Clear();
            foreach (var time in times)
                this.Rows.Add(time);
        }
        finally
        {
            this.Updating = false;
            SetColors();
        }
    } // internal void SetTimes (double, [int])
} // internal sealed partial class TimeTable : DataGridView
