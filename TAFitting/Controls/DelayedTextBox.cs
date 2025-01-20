
// (c) 2024 Kazuki KOHZUKI

using DisposalGenerator;
using Timer = System.Windows.Forms.Timer;

namespace TAFitting.Controls;

/// <summary>
/// Represents a text box with delayed text changed event.
/// </summary>
[DesignerCategory("Code")]
[AutoDisposal]
internal partial class DelayedTextBox : TextBox
{
    private Timer? delayedTextChangedTimer;

    /// <summary>
    /// Gets or set the delay time for the <see cref="DelayedTextChanged"/> event to occur.
    /// </summary>
    internal int DelayedTextChangedTimeout { get; set; } = 1_000;

    /// <summary>
    /// Occurs when the specified time passed after <see cref="Text"/> property value changes.
    /// </summary>
    internal event EventHandler? DelayedTextChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelayedTextBox"/> class.
    /// </summary>
    internal DelayedTextBox() : base() { }

    /// <summary>
    /// Raises the <see cref="DelayedTextChanged"/> event.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnDelayedTextChanged(EventArgs e)
        => DelayedTextChanged?.Invoke(this, e);

    override protected void OnTextChanged(EventArgs e)
    {
        InitializeDelayedTextChangedEvent();
        base.OnTextChanged(e);
    } // override protected void OnTextChanged (EventArgs)

    private void InitializeDelayedTextChangedEvent()
    {
        this.delayedTextChangedTimer?.Stop();

        if (this.delayedTextChangedTimer is null || this.delayedTextChangedTimer.Interval != this.DelayedTextChangedTimeout)
        {
            this.delayedTextChangedTimer = new()
            {
                Interval = this.DelayedTextChangedTimeout,
            };
            this.delayedTextChangedTimer.Tick += HandleDelayedTextChangedTimerTick;
        }

        this.delayedTextChangedTimer.Start();
    } // private void InitializeDelayedTextChangedEvent ()

    private void HandleDelayedTextChangedTimerTick(object? sender, EventArgs e)
    {
        if (sender is Timer timer)
            timer.Stop();

        OnDelayedTextChanged(EventArgs.Empty);
    } // private void HandleDelayedTextChangedTimerTick (object?, EventArgs)
} // internal partial class DelayedTextBox : TextBox
