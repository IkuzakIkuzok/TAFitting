
// (c) 2025 Kazuki Kohzuki

using DisposalGenerator;
using TAFitting.Print;

namespace TAFitting.Controls.Spectra;

/// <summary>
/// Represents a window for previewing the summary of spectra for printing.
/// </summary>
[DesignerCategory("Code")]
[AutoDisposal]
internal sealed partial class SummaryPreviewWindow : Form
{
    [NotToBeDisposed]  // The document should be disposed by the caller.
    private readonly SpectraSummaryDocument document;

    private readonly SplitContainer main_container;
    private readonly PrintPreviewControl preview;

    private readonly Label lb_name, lb_font;
    private readonly DelayedTextBox sample_name;
    private readonly CheckBox datetime;
    private readonly Label font;
    private readonly Button changeFont;

    /// <summary>
    /// Initializes a new instance of the <see cref="SummaryPreviewWindow"/> class.
    /// </summary>
    /// <param name="document">The document to preview.</param>
    internal SummaryPreviewWindow(SpectraSummaryDocument document)
    {
        this.document = document;

        this.Text = "Summary Preview";
        this.Size = this.MinimumSize = new(800, 600);

        this.main_container = new()
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterWidth = 2,
            Parent = this,
        };

        this.preview = new()
        {
            Document = this.document,
            Dock = DockStyle.Fill,
            Parent = this.main_container.Panel1,
        };

        this.preview.InvalidatePreview();

        this.lb_name = new()
        {
            Text = "Sample Name",
            Width = 100,
            Location = new(10, 30),
            Parent = this.main_container.Panel2,
        };

        this.sample_name = new()
        {
            Text = this.document.AdditionalContents[AdditionalContentPosition.UpperLeft].FirstOrDefault()?.Text ?? string.Empty,
            Width = 200,
            Location = new(120, 30),
            Parent = this.main_container.Panel2,
        };
        this.sample_name.DelayedTextChanged += SetSampleName;

        this.datetime = new()
        {
            Text = "Show date and time",
            Checked = !string.IsNullOrWhiteSpace(this.document.AdditionalContents[AdditionalContentPosition.UpperRight].FirstOrDefault()?.Text ?? string.Empty),
            Width = 200,
            Location = new(10, 70),
            Parent = this.main_container.Panel2,
        };
        this.datetime.CheckedChanged += SetDateTime;

        this.lb_font = new()
        {
            Text = "Font",
            Width = 100,
            Location = new(10, 110),
            Parent = this.main_container.Panel2,
        };

        this.font = new()
        {
            Text = $"{this.document.FontName}, {this.document.FonrSize} pt",
            Width = 100,
            Location = new(120, 110),
            Parent = this.main_container.Panel2,
        };

        this.changeFont = new()
        {
            Text = "Change...",
            Width = 80,
            Location = new(240, 110),
            Parent = this.main_container.Panel2,
        };
        this.changeFont.Click += ChangeFont;

        this.CancelButton = new Button()
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new(10, 150),
            Parent = this.main_container.Panel2,
        };

        var run = new Button()
        {
            Text = "Print",
            DialogResult = DialogResult.OK,
            Location = new(100, 150),
            Parent = this.main_container.Panel2,
        };
        run.Click += Run;
        this.AcceptButton = run;

        this.main_container.SplitterDistance = 400;
    } // ctor (SpectraSummaryDocument)

    private void SetSampleName(object? sender, EventArgs e)
    {
        var content = this.document.AdditionalContents[AdditionalContentPosition.UpperLeft].FirstOrDefault();
        if (content is null) return;
        content.Text = this.sample_name.Text;
        this.preview.InvalidatePreview();
    } // private void SetSampleName (object?, EventArgs)

    private void SetDateTime(object? sender, EventArgs e)
    {
        var content = this.document.AdditionalContents[AdditionalContentPosition.UpperRight].FirstOrDefault();
        if (content is null) return;
        content.Text = this.datetime.Checked ? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") : string.Empty;
        this.preview.InvalidatePreview();
    } // private void SetDateTime (object?, EventArgs)

    private void ChangeFont(object? sender, EventArgs e)
    {
        using var dialog = new FontDialog()
        {
            Font = new(this.document.FontName, this.document.FonrSize),
            ShowEffects = false,
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        this.document.FontName = dialog.Font.Name;
        this.document.FonrSize = dialog.Font.Size;
        this.font.Text = $"{this.document.FontName}, {this.document.FonrSize} pt";
        this.preview.InvalidatePreview();
    } // private void ChangeFont (object?, EventArgs)

    private void Run(object? sender, EventArgs e)
        => Run();

    private void Run()
    {
        using var dialog = new PrintDialog()
        {
            Document = this.document,
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        try
        {
            dialog.Document.Print();
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    } // private void Run ()
} // internal sealed partial class SummaryPreviewWindow : Form
