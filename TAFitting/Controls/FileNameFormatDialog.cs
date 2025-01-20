
// (c) 2024 Kazuki KOHZUKI

using DisposalGenerator;
using TAFitting.Data;

namespace TAFitting.Controls;

/// <summary>
/// Represents a dialog for setting the filename format.
/// </summary>
[DesignerCategory("Code")]
[AutoDisposal]
internal sealed partial class FileNameFormatDialog : Form
{
    private readonly Label lb_ab, lb_b, lb_basename;
    private readonly TextBox tb_ab, tb_b, tb_basename;
    private readonly Label lb_test_ab, lb_test_b;
    private readonly Label lb_sample_ab, lb_sample_b;
    private readonly Button ok;

    /// <summary>
    /// Gets or sets the 'a−b' format.
    /// </summary>
    internal string AMinusBFormat
    {
        get => this.tb_ab.Text;
        set => this.tb_ab.Text = value;
    }

    /// <summary>
    /// Gets or sets the 'b' format.
    /// </summary>
    internal string BFormat
    {
        get => this.tb_b.Text;
        set => this.tb_b.Text = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileNameFormatDialog"/> class.
    /// </summary>
    internal FileNameFormatDialog()
    {
        this.Text = "Filename format";
        this.Size = this.MinimumSize = this.MaximumSize = new Size(400, 300);
        this.MaximizeBox = false;

        this.lb_ab = new()
        {
            Text = "a\u2212b",
            Location = new Point(10, 10),
            Width = 40,
            Parent = this,
        };

        this.tb_ab = new()
        {
            Text = Program.AMinusBSignalFormat,
            Location = new Point(60, 10),
            Width = 300,
            PlaceholderText = "<BASENAME>-a-b-tdm.csv",
            Parent = this,
        };
        this.tb_ab.TextChanged += UpdateText;

        this.lb_b = new()
        {
            Text = "b",
            Location = new Point(10, 40),
            Width = 40,
            Parent = this,
        };

        this.tb_b = new()
        {
            Text = Program.BSignalFormat,
            Location = new Point(60, 40),
            Width = 300,
            PlaceholderText = "<BASENAME>-b.csv",
            Parent = this,
        };
        this.tb_b.TextChanged += UpdateText;

        this.lb_basename = new()
        {
            Text = "Test basename",
            Location = new Point(10, 100),
            Width = 80,
            Parent = this,
        };

        this.tb_basename = new()
        {
            Text = "600nm",
            Location = new Point(100, 100),
            Width = 260,
            Parent = this,
        };
        this.tb_basename.TextChanged += UpdateText;

        this.lb_test_ab = new()
        {
            Text = "a\u2212b test",
            Location = new Point(10, 130),
            Width = 80,
            Parent = this,
        };

        this.lb_sample_ab = new()
        {
            Location = new Point(100, 130),
            Width = 260,
            Parent = this,
        };

        this.lb_test_b = new()
        {
            Text = "b test",
            Location = new Point(10, 160),
            Width = 80,
            Parent = this,
        };

        this.lb_sample_b = new()
        {
            Location = new Point(100, 160),
            Width = 260,
            Parent = this,
        };

        this.ok = new()
        {
            Text = "OK",
            Location = new Point(10, 200),
            Size = new Size(80, 30),
            DialogResult = DialogResult.OK,
            Parent = this,
        };
        this.AcceptButton = this.ok;

        this.CancelButton = new Button()
        {
            Text = "Cancel",
            Location = new Point(100, 200),
            Size = new Size(80, 30),
            DialogResult = DialogResult.Cancel,
            Parent = this,
        };

        UpdateText();
    } // ctor ()

    /// <summary>
    /// Updats the test filenames.
    /// </summary>
    private void UpdateText(object? sender, EventArgs e)
        => UpdateText();

    /// <summary>
    /// Updats the test filenames.
    /// </summary>
    private void UpdateText()
    {
        var format_ab = this.tb_ab.Text;
        var format_b = this.tb_b.Text;
        if (string.IsNullOrEmpty(format_ab) || string.IsNullOrEmpty(format_b))
        {
            this.ok.Enabled = false;
            return;
        }
        else
        {
            this.ok.Enabled = true;
        }

        var basename = this.tb_basename.Text;
        if (string.IsNullOrEmpty(basename)) return;

        this.lb_sample_ab.Text = FileNameHandler.GetFileName(basename, format_ab);
        this.lb_sample_b.Text = FileNameHandler.GetFileName(basename, format_b);
    } // UpdateText ()
} // internal sealed partial class FileNameFormatDialog : Form

