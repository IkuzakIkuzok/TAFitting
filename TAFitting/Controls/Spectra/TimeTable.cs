
// (c) 2024 Kazuki Kohzuki

namespace TAFitting.Controls.Spectra;

[DesignerCategory("Code")]
internal sealed class TimeTable : DataGridView
{
    internal IEnumerable<double> Times
        => this.Rows
               .Cast<DataGridViewRow>()
               .Where(row => !row.IsNewRow)
               .Select(row => (double)row.Cells["Time"].Value)
               .Order();

    internal TimeTable()
    {
        this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.EnableHeadersVisualStyles = false;

        var col = new DataGridViewNumericBoxColumn()
        {
            Name = "Time",
            HeaderText = "Time",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
        };
        this.Columns.Add(col);
    } // ctor ()

    override protected void OnSortCompare(DataGridViewSortCompareEventArgs e)
    {
        e.Handled = true;
        e.SortResult = e.CellValue1 is double v1 && e.CellValue2 is double v2
            ? v1.CompareTo(v2)
            : 0;
    } // override protected void OnSortCompare (DataGridViewSortCompareEventArgs)

    internal void SetColors()
    {
        Sort(this.Columns["Time"], ListSortDirection.Ascending);
        var count = this.Rows.Count - 1;
        var gradient = new ColorGradient(Program.GradientStart, Program.GradientEnd, count);
        for (var i = 0; i < count; i++)
        {
            var row = this.Rows[i];
            row.HeaderCell.Style.BackColor = gradient[i];
        }
    } // internal void SetColors ()
} // internal sealed class TimeTable : DataGridView
