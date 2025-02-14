﻿
// (c) 2025 Kazuki Kohzuki


namespace TAFitting.Filter.Fourier;

internal abstract class FourierFilterAuto : FourierFilter
{
    protected double ratio = 0.1;

    override protected string GetName()
        => $"{this.ratio*100}%";

    override protected string GetDescription()
        => $"A filter that uses Fourier transform with a cutoff frequency of {this.ratio * 100}% of time bandwidth.";

    override public IReadOnlyList<double> Filter(IReadOnlyList<double> time, IReadOnlyList<double> signal)
    {
        this.cutoff = 1 / ((time[^1] - time[0]) * this.ratio);
        return base.Filter(time, signal);
    } // public override IReadOnlyList<double> Filter(IReadOnlyList<double> time, IReadOnlyList<double> signal)
} // internal abstract class FourierFilterAuto : FourierFilter
