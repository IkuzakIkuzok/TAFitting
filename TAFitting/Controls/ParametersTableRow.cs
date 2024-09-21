﻿
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Controls;

internal sealed class ParametersTableRow : DataGridViewRow
{
    private double GetCellValue(int index, double defaultValue) =>
        this.Cells[index].Value is double value ? value : defaultValue;

    internal double Wavelength
    {
        get => GetCellValue(0, 0.0);
        set => this.Cells[0].Value = value;
    }

    internal int ParametersCount
        => this.Cells.Count - 1;

    internal double this[int index]
    {
        get
        {
            var cell = this.Cells[index + 1];
            if (cell.Value is double value) return value;
            if (cell is DataGridViewNumericBoxCell numericBoxCell)
                return numericBoxCell.DefaultValue;
            return 0.0;
        }
        set => this.Cells[index + 1].Value = value;
    }

    internal IList<double> Parameters
    {
        get
        {
            var count = this.ParametersCount;
            var parameters = new List<double>(count);
            for (var i = 0; i < count; i++)
                parameters.Add(this[i]);
            return parameters;
        }
        set
        {
            for (var i = 0; i < value.Count; i++)
                this[i] = value[i];
        }
    }
} // internal sealed class ParametersTableRow : DataGridViewRow