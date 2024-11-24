﻿// <auto-generated/>

using System.Runtime.InteropServices;

using TAFitting.Data.Solver.SIMD;

namespace TAFitting.Model.Polynomial
{

	/// <summary>
	/// Represents a 1st-order polynomial model.
	/// </summary>
	internal partial class Polynomial1 : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>
	{
		private static readonly Parameter[] parameters = [
			new() { Name = "A0", InitialValue = 1e+1, IsMagnitude = true },
			new() { Name = "A1", InitialValue = -1e+0, IsMagnitude = true },
		];

		/// <inheritdoc/>
		public string Name => "Poly1";

		/// <inheritdoc/>
		public string Description => "1st-order polynomial model";

		/// <inheritdoc/>
		public string ExcelFormula => "[A0] + [A1] * $X";

		/// <inheritdoc/>
		public IReadOnlyList<Parameter> Parameters => parameters;

		/// <inheritdoc/>
		public bool XLogScale => false;

		/// <inheritdoc/>
		public bool YLogScale => false;

		/// <inheritdoc/>
		public Func<double, double> GetFunction(IReadOnlyList<double> parameters)
		{
			var a0 = parameters[0];
			var a1 = parameters[1];

			return x => a0 + a1 * x;
		} // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)

		/// <inheritdoc/>
		public Func<AvxVector2048, AvxVector2048> GetVectorizedFunc(IReadOnlyList<double> parameters)
			=> x => 
			{
				var length = x.Length << 2;
				var a = new AvxVector2048(length, parameters[1]);
				AvxVector2048.Multiply(a, x, a);
				AvxVector2048.Add(a, parameters[0], a);
				return a;
			};

		/// <inheritdoc/>
		public Action<double, double[]> GetDerivatives(IReadOnlyList<double> parameters)
			=> Derivatives;

		private void Derivatives(double x, double[] res)
		{
			var d_a0 = 1.0;
			var d_a1 = x;

			res[0] = d_a0;
			res[1] = d_a1;
		} // private void Derivatives (double, double[])

		/// <inheritdoc/>
		public Action<AvxVector2048, AvxVector2048[]> GetVectorizedDerivatives(IReadOnlyList<double> parameters)
			=> (x, res) =>
			{
				res[0].Load(1.0);
				res[1] = x;
			};
	} // internal partial class Polynomial1 : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>
} // namespaceTAFitting.Model.Polynomial


namespace TAFitting.Model.Polynomial
{

	/// <summary>
	/// Represents a 2nd-order polynomial model.
	/// </summary>
	internal partial class Polynomial2 : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>
	{
		private static readonly Parameter[] parameters = [
			new() { Name = "A0", InitialValue = 1e+2, IsMagnitude = true },
			new() { Name = "A1", InitialValue = -1e+1, IsMagnitude = true },
			new() { Name = "A2", InitialValue = 1e+0, IsMagnitude = true },
		];

		/// <inheritdoc/>
		public string Name => "Poly2";

		/// <inheritdoc/>
		public string Description => "2nd-order polynomial model";

		/// <inheritdoc/>
		public string ExcelFormula => "[A0] + [A1] * $X + [A2] * $X^2";

		/// <inheritdoc/>
		public IReadOnlyList<Parameter> Parameters => parameters;

		/// <inheritdoc/>
		public bool XLogScale => false;

		/// <inheritdoc/>
		public bool YLogScale => false;

		/// <inheritdoc/>
		public Func<double, double> GetFunction(IReadOnlyList<double> parameters)
		{
			var a0 = parameters[0];
			var a1 = parameters[1];
			var a2 = parameters[2];

			return x => a0 + a1 * x + a2 * x * x;
		} // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)

		/// <inheritdoc/>
		public Func<AvxVector2048, AvxVector2048> GetVectorizedFunc(IReadOnlyList<double> parameters)
			=> x => 
			{
				var length = x.Length << 2;
				var temp = new AvxVector2048(length);
				var temp_x = new AvxVector2048(length, 1.0);
				var a0 = new AvxVector2048(length, parameters[0]);

				var a1 = parameters[1];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a1, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a2 = parameters[2];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a2, temp);
				AvxVector2048.Add(temp, a0, a0);

				return a0;
			};

		/// <inheritdoc/>
		public Action<double, double[]> GetDerivatives(IReadOnlyList<double> parameters)
			=> Derivatives;

		private void Derivatives(double x, double[] res)
		{
			var d_a0 = 1.0;
			var d_a1 = x;
			var d_a2 = d_a1 * x;

			res[0] = d_a0;
			res[1] = d_a1;
			res[2] = d_a2;
		} // private void Derivatives (double, double[])

		/// <inheritdoc/>
		public Action<AvxVector2048, AvxVector2048[]> GetVectorizedDerivatives(IReadOnlyList<double> parameters)
			=> (x, res) =>
			{
				res[0].Load(1.0);
				res[1] = x;
				AvxVector2048.Multiply(res[1], x, res[2]);
			};
	} // internal partial class Polynomial2 : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>
} // namespaceTAFitting.Model.Polynomial


namespace TAFitting.Model.Polynomial
{

	/// <summary>
	/// Represents a 3rd-order polynomial model.
	/// </summary>
	internal partial class Polynomial3 : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>
	{
		private static readonly Parameter[] parameters = [
			new() { Name = "A0", InitialValue = 1e+3, IsMagnitude = true },
			new() { Name = "A1", InitialValue = -1e+2, IsMagnitude = true },
			new() { Name = "A2", InitialValue = 1e+1, IsMagnitude = true },
			new() { Name = "A3", InitialValue = -1e+0, IsMagnitude = true },
		];

		/// <inheritdoc/>
		public string Name => "Poly3";

		/// <inheritdoc/>
		public string Description => "3rd-order polynomial model";

		/// <inheritdoc/>
		public string ExcelFormula => "[A0] + [A1] * $X + [A2] * $X^2 + [A3] * $X^3";

		/// <inheritdoc/>
		public IReadOnlyList<Parameter> Parameters => parameters;

		/// <inheritdoc/>
		public bool XLogScale => false;

		/// <inheritdoc/>
		public bool YLogScale => false;

		/// <inheritdoc/>
		public Func<double, double> GetFunction(IReadOnlyList<double> parameters)
		{
			var a0 = parameters[0];
			var a1 = parameters[1];
			var a2 = parameters[2];
			var a3 = parameters[3];

			return (x) =>
			{
				var x1 = x;
				var x2 = x1 * x;
				var x3 = x2 * x;
				return a0 + a1 * x1 + a2 * x2 + a3 * x3;
			};
		} // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)

		/// <inheritdoc/>
		public Func<AvxVector2048, AvxVector2048> GetVectorizedFunc(IReadOnlyList<double> parameters)
			=> x => 
			{
				var length = x.Length << 2;
				var temp = new AvxVector2048(length);
				var temp_x = new AvxVector2048(length, 1.0);
				var a0 = new AvxVector2048(length, parameters[0]);

				var a1 = parameters[1];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a1, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a2 = parameters[2];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a2, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a3 = parameters[3];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a3, temp);
				AvxVector2048.Add(temp, a0, a0);

				return a0;
			};

		/// <inheritdoc/>
		public Action<double, double[]> GetDerivatives(IReadOnlyList<double> parameters)
			=> Derivatives;

		private void Derivatives(double x, double[] res)
		{
			var d_a0 = 1.0;
			var d_a1 = x;
			var d_a2 = d_a1 * x;
			var d_a3 = d_a2 * x;

			res[0] = d_a0;
			res[1] = d_a1;
			res[2] = d_a2;
			res[3] = d_a3;
		} // private void Derivatives (double, double[])

		/// <inheritdoc/>
		public Action<AvxVector2048, AvxVector2048[]> GetVectorizedDerivatives(IReadOnlyList<double> parameters)
			=> (x, res) =>
			{
				res[0].Load(1.0);
				res[1] = x;
				AvxVector2048.Multiply(res[1], x, res[2]);
				AvxVector2048.Multiply(res[2], x, res[3]);
			};
	} // internal partial class Polynomial3 : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>
} // namespaceTAFitting.Model.Polynomial


namespace TAFitting.Model.Polynomial
{

	/// <summary>
	/// Represents a 4th-order polynomial model.
	/// </summary>
	internal partial class Polynomial4 : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>
	{
		private static readonly Parameter[] parameters = [
			new() { Name = "A0", InitialValue = 1e+4, IsMagnitude = true },
			new() { Name = "A1", InitialValue = -1e+3, IsMagnitude = true },
			new() { Name = "A2", InitialValue = 1e+2, IsMagnitude = true },
			new() { Name = "A3", InitialValue = -1e+1, IsMagnitude = true },
			new() { Name = "A4", InitialValue = 1e+0, IsMagnitude = true },
		];

		/// <inheritdoc/>
		public string Name => "Poly4";

		/// <inheritdoc/>
		public string Description => "4th-order polynomial model";

		/// <inheritdoc/>
		public string ExcelFormula => "[A0] + [A1] * $X + [A2] * $X^2 + [A3] * $X^3 + [A4] * $X^4";

		/// <inheritdoc/>
		public IReadOnlyList<Parameter> Parameters => parameters;

		/// <inheritdoc/>
		public bool XLogScale => false;

		/// <inheritdoc/>
		public bool YLogScale => false;

		/// <inheritdoc/>
		public Func<double, double> GetFunction(IReadOnlyList<double> parameters)
		{
			var a0 = parameters[0];
			var a1 = parameters[1];
			var a2 = parameters[2];
			var a3 = parameters[3];
			var a4 = parameters[4];

			return (x) =>
			{
				var x1 = x;
				var x2 = x1 * x;
				var x3 = x2 * x;
				var x4 = x3 * x;
				return a0 + a1 * x1 + a2 * x2 + a3 * x3 + a4 * x4;
			};
		} // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)

		/// <inheritdoc/>
		public Func<AvxVector2048, AvxVector2048> GetVectorizedFunc(IReadOnlyList<double> parameters)
			=> x => 
			{
				var length = x.Length << 2;
				var temp = new AvxVector2048(length);
				var temp_x = new AvxVector2048(length, 1.0);
				var a0 = new AvxVector2048(length, parameters[0]);

				var a1 = parameters[1];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a1, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a2 = parameters[2];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a2, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a3 = parameters[3];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a3, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a4 = parameters[4];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a4, temp);
				AvxVector2048.Add(temp, a0, a0);

				return a0;
			};

		/// <inheritdoc/>
		public Action<double, double[]> GetDerivatives(IReadOnlyList<double> parameters)
			=> Derivatives;

		private void Derivatives(double x, double[] res)
		{
			var d_a0 = 1.0;
			var d_a1 = x;
			var d_a2 = d_a1 * x;
			var d_a3 = d_a2 * x;
			var d_a4 = d_a3 * x;

			res[0] = d_a0;
			res[1] = d_a1;
			res[2] = d_a2;
			res[3] = d_a3;
			res[4] = d_a4;
		} // private void Derivatives (double, double[])

		/// <inheritdoc/>
		public Action<AvxVector2048, AvxVector2048[]> GetVectorizedDerivatives(IReadOnlyList<double> parameters)
			=> (x, res) =>
			{
				res[0].Load(1.0);
				res[1] = x;
				AvxVector2048.Multiply(res[1], x, res[2]);
				AvxVector2048.Multiply(res[2], x, res[3]);
				AvxVector2048.Multiply(res[3], x, res[4]);
			};
	} // internal partial class Polynomial4 : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>
} // namespaceTAFitting.Model.Polynomial


namespace TAFitting.Model.Polynomial
{

	/// <summary>
	/// Represents a 5th-order polynomial model.
	/// </summary>
	internal partial class Polynomial5 : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>
	{
		private static readonly Parameter[] parameters = [
			new() { Name = "A0", InitialValue = 1e+5, IsMagnitude = true },
			new() { Name = "A1", InitialValue = -1e+4, IsMagnitude = true },
			new() { Name = "A2", InitialValue = 1e+3, IsMagnitude = true },
			new() { Name = "A3", InitialValue = -1e+2, IsMagnitude = true },
			new() { Name = "A4", InitialValue = 1e+1, IsMagnitude = true },
			new() { Name = "A5", InitialValue = -1e+0, IsMagnitude = true },
		];

		/// <inheritdoc/>
		public string Name => "Poly5";

		/// <inheritdoc/>
		public string Description => "5th-order polynomial model";

		/// <inheritdoc/>
		public string ExcelFormula => "[A0] + [A1] * $X + [A2] * $X^2 + [A3] * $X^3 + [A4] * $X^4 + [A5] * $X^5";

		/// <inheritdoc/>
		public IReadOnlyList<Parameter> Parameters => parameters;

		/// <inheritdoc/>
		public bool XLogScale => false;

		/// <inheritdoc/>
		public bool YLogScale => false;

		/// <inheritdoc/>
		public Func<double, double> GetFunction(IReadOnlyList<double> parameters)
		{
			var a0 = parameters[0];
			var a1 = parameters[1];
			var a2 = parameters[2];
			var a3 = parameters[3];
			var a4 = parameters[4];
			var a5 = parameters[5];

			return (x) =>
			{
				var x1 = x;
				var x2 = x1 * x;
				var x3 = x2 * x;
				var x4 = x3 * x;
				var x5 = x4 * x;
				return a0 + a1 * x1 + a2 * x2 + a3 * x3 + a4 * x4 + a5 * x5;
			};
		} // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)

		/// <inheritdoc/>
		public Func<AvxVector2048, AvxVector2048> GetVectorizedFunc(IReadOnlyList<double> parameters)
			=> x => 
			{
				var length = x.Length << 2;
				var temp = new AvxVector2048(length);
				var temp_x = new AvxVector2048(length, 1.0);
				var a0 = new AvxVector2048(length, parameters[0]);

				var a1 = parameters[1];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a1, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a2 = parameters[2];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a2, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a3 = parameters[3];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a3, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a4 = parameters[4];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a4, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a5 = parameters[5];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a5, temp);
				AvxVector2048.Add(temp, a0, a0);

				return a0;
			};

		/// <inheritdoc/>
		public Action<double, double[]> GetDerivatives(IReadOnlyList<double> parameters)
			=> Derivatives;

		private void Derivatives(double x, double[] res)
		{
			var d_a0 = 1.0;
			var d_a1 = x;
			var d_a2 = d_a1 * x;
			var d_a3 = d_a2 * x;
			var d_a4 = d_a3 * x;
			var d_a5 = d_a4 * x;

			res[0] = d_a0;
			res[1] = d_a1;
			res[2] = d_a2;
			res[3] = d_a3;
			res[4] = d_a4;
			res[5] = d_a5;
		} // private void Derivatives (double, double[])

		/// <inheritdoc/>
		public Action<AvxVector2048, AvxVector2048[]> GetVectorizedDerivatives(IReadOnlyList<double> parameters)
			=> (x, res) =>
			{
				res[0].Load(1.0);
				res[1] = x;
				AvxVector2048.Multiply(res[1], x, res[2]);
				AvxVector2048.Multiply(res[2], x, res[3]);
				AvxVector2048.Multiply(res[3], x, res[4]);
				AvxVector2048.Multiply(res[4], x, res[5]);
			};
	} // internal partial class Polynomial5 : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>
} // namespaceTAFitting.Model.Polynomial


namespace TAFitting.Model.Polynomial
{

	/// <summary>
	/// Represents a 6th-order polynomial model.
	/// </summary>
	internal partial class Polynomial6 : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>
	{
		private static readonly Parameter[] parameters = [
			new() { Name = "A0", InitialValue = 1e+6, IsMagnitude = true },
			new() { Name = "A1", InitialValue = -1e+5, IsMagnitude = true },
			new() { Name = "A2", InitialValue = 1e+4, IsMagnitude = true },
			new() { Name = "A3", InitialValue = -1e+3, IsMagnitude = true },
			new() { Name = "A4", InitialValue = 1e+2, IsMagnitude = true },
			new() { Name = "A5", InitialValue = -1e+1, IsMagnitude = true },
			new() { Name = "A6", InitialValue = 1e+0, IsMagnitude = true },
		];

		/// <inheritdoc/>
		public string Name => "Poly6";

		/// <inheritdoc/>
		public string Description => "6th-order polynomial model";

		/// <inheritdoc/>
		public string ExcelFormula => "[A0] + [A1] * $X + [A2] * $X^2 + [A3] * $X^3 + [A4] * $X^4 + [A5] * $X^5 + [A6] * $X^6";

		/// <inheritdoc/>
		public IReadOnlyList<Parameter> Parameters => parameters;

		/// <inheritdoc/>
		public bool XLogScale => false;

		/// <inheritdoc/>
		public bool YLogScale => false;

		/// <inheritdoc/>
		public Func<double, double> GetFunction(IReadOnlyList<double> parameters)
		{
			var a0 = parameters[0];
			var a1 = parameters[1];
			var a2 = parameters[2];
			var a3 = parameters[3];
			var a4 = parameters[4];
			var a5 = parameters[5];
			var a6 = parameters[6];

			return (x) =>
			{
				var x1 = x;
				var x2 = x1 * x;
				var x3 = x2 * x;
				var x4 = x3 * x;
				var x5 = x4 * x;
				var x6 = x5 * x;
				return a0 + a1 * x1 + a2 * x2 + a3 * x3 + a4 * x4 + a5 * x5 + a6 * x6;
			};
		} // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)

		/// <inheritdoc/>
		public Func<AvxVector2048, AvxVector2048> GetVectorizedFunc(IReadOnlyList<double> parameters)
			=> x => 
			{
				var length = x.Length << 2;
				var temp = new AvxVector2048(length);
				var temp_x = new AvxVector2048(length, 1.0);
				var a0 = new AvxVector2048(length, parameters[0]);

				var a1 = parameters[1];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a1, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a2 = parameters[2];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a2, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a3 = parameters[3];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a3, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a4 = parameters[4];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a4, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a5 = parameters[5];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a5, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a6 = parameters[6];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a6, temp);
				AvxVector2048.Add(temp, a0, a0);

				return a0;
			};

		/// <inheritdoc/>
		public Action<double, double[]> GetDerivatives(IReadOnlyList<double> parameters)
			=> Derivatives;

		private void Derivatives(double x, double[] res)
		{
			var d_a0 = 1.0;
			var d_a1 = x;
			var d_a2 = d_a1 * x;
			var d_a3 = d_a2 * x;
			var d_a4 = d_a3 * x;
			var d_a5 = d_a4 * x;
			var d_a6 = d_a5 * x;

			res[0] = d_a0;
			res[1] = d_a1;
			res[2] = d_a2;
			res[3] = d_a3;
			res[4] = d_a4;
			res[5] = d_a5;
			res[6] = d_a6;
		} // private void Derivatives (double, double[])

		/// <inheritdoc/>
		public Action<AvxVector2048, AvxVector2048[]> GetVectorizedDerivatives(IReadOnlyList<double> parameters)
			=> (x, res) =>
			{
				res[0].Load(1.0);
				res[1] = x;
				AvxVector2048.Multiply(res[1], x, res[2]);
				AvxVector2048.Multiply(res[2], x, res[3]);
				AvxVector2048.Multiply(res[3], x, res[4]);
				AvxVector2048.Multiply(res[4], x, res[5]);
				AvxVector2048.Multiply(res[5], x, res[6]);
			};
	} // internal partial class Polynomial6 : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>
} // namespaceTAFitting.Model.Polynomial


namespace TAFitting.Model.Polynomial
{

	/// <summary>
	/// Represents a 7th-order polynomial model.
	/// </summary>
	internal partial class Polynomial7 : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>
	{
		private static readonly Parameter[] parameters = [
			new() { Name = "A0", InitialValue = 1e+7, IsMagnitude = true },
			new() { Name = "A1", InitialValue = -1e+6, IsMagnitude = true },
			new() { Name = "A2", InitialValue = 1e+5, IsMagnitude = true },
			new() { Name = "A3", InitialValue = -1e+4, IsMagnitude = true },
			new() { Name = "A4", InitialValue = 1e+3, IsMagnitude = true },
			new() { Name = "A5", InitialValue = -1e+2, IsMagnitude = true },
			new() { Name = "A6", InitialValue = 1e+1, IsMagnitude = true },
			new() { Name = "A7", InitialValue = -1e+0, IsMagnitude = true },
		];

		/// <inheritdoc/>
		public string Name => "Poly7";

		/// <inheritdoc/>
		public string Description => "7th-order polynomial model";

		/// <inheritdoc/>
		public string ExcelFormula => "[A0] + [A1] * $X + [A2] * $X^2 + [A3] * $X^3 + [A4] * $X^4 + [A5] * $X^5 + [A6] * $X^6 + [A7] * $X^7";

		/// <inheritdoc/>
		public IReadOnlyList<Parameter> Parameters => parameters;

		/// <inheritdoc/>
		public bool XLogScale => false;

		/// <inheritdoc/>
		public bool YLogScale => false;

		/// <inheritdoc/>
		public Func<double, double> GetFunction(IReadOnlyList<double> parameters)
		{
			var a0 = parameters[0];
			var a1 = parameters[1];
			var a2 = parameters[2];
			var a3 = parameters[3];
			var a4 = parameters[4];
			var a5 = parameters[5];
			var a6 = parameters[6];
			var a7 = parameters[7];

			return (x) =>
			{
				var x1 = x;
				var x2 = x1 * x;
				var x3 = x2 * x;
				var x4 = x3 * x;
				var x5 = x4 * x;
				var x6 = x5 * x;
				var x7 = x6 * x;
				return a0 + a1 * x1 + a2 * x2 + a3 * x3 + a4 * x4 + a5 * x5 + a6 * x6 + a7 * x7;
			};
		} // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)

		/// <inheritdoc/>
		public Func<AvxVector2048, AvxVector2048> GetVectorizedFunc(IReadOnlyList<double> parameters)
			=> x => 
			{
				var length = x.Length << 2;
				var temp = new AvxVector2048(length);
				var temp_x = new AvxVector2048(length, 1.0);
				var a0 = new AvxVector2048(length, parameters[0]);

				var a1 = parameters[1];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a1, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a2 = parameters[2];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a2, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a3 = parameters[3];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a3, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a4 = parameters[4];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a4, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a5 = parameters[5];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a5, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a6 = parameters[6];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a6, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a7 = parameters[7];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a7, temp);
				AvxVector2048.Add(temp, a0, a0);

				return a0;
			};

		/// <inheritdoc/>
		public Action<double, double[]> GetDerivatives(IReadOnlyList<double> parameters)
			=> Derivatives;

		private void Derivatives(double x, double[] res)
		{
			var d_a0 = 1.0;
			var d_a1 = x;
			var d_a2 = d_a1 * x;
			var d_a3 = d_a2 * x;
			var d_a4 = d_a3 * x;
			var d_a5 = d_a4 * x;
			var d_a6 = d_a5 * x;
			var d_a7 = d_a6 * x;

			res[0] = d_a0;
			res[1] = d_a1;
			res[2] = d_a2;
			res[3] = d_a3;
			res[4] = d_a4;
			res[5] = d_a5;
			res[6] = d_a6;
			res[7] = d_a7;
		} // private void Derivatives (double, double[])

		/// <inheritdoc/>
		public Action<AvxVector2048, AvxVector2048[]> GetVectorizedDerivatives(IReadOnlyList<double> parameters)
			=> (x, res) =>
			{
				res[0].Load(1.0);
				res[1] = x;
				AvxVector2048.Multiply(res[1], x, res[2]);
				AvxVector2048.Multiply(res[2], x, res[3]);
				AvxVector2048.Multiply(res[3], x, res[4]);
				AvxVector2048.Multiply(res[4], x, res[5]);
				AvxVector2048.Multiply(res[5], x, res[6]);
				AvxVector2048.Multiply(res[6], x, res[7]);
			};
	} // internal partial class Polynomial7 : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>
} // namespaceTAFitting.Model.Polynomial


namespace TAFitting.Model.Polynomial
{

	/// <summary>
	/// Represents a 8th-order polynomial model.
	/// </summary>
	internal partial class Polynomial8 : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>
	{
		private static readonly Parameter[] parameters = [
			new() { Name = "A0", InitialValue = 1e+8, IsMagnitude = true },
			new() { Name = "A1", InitialValue = -1e+7, IsMagnitude = true },
			new() { Name = "A2", InitialValue = 1e+6, IsMagnitude = true },
			new() { Name = "A3", InitialValue = -1e+5, IsMagnitude = true },
			new() { Name = "A4", InitialValue = 1e+4, IsMagnitude = true },
			new() { Name = "A5", InitialValue = -1e+3, IsMagnitude = true },
			new() { Name = "A6", InitialValue = 1e+2, IsMagnitude = true },
			new() { Name = "A7", InitialValue = -1e+1, IsMagnitude = true },
			new() { Name = "A8", InitialValue = 1e+0, IsMagnitude = true },
		];

		/// <inheritdoc/>
		public string Name => "Poly8";

		/// <inheritdoc/>
		public string Description => "8th-order polynomial model";

		/// <inheritdoc/>
		public string ExcelFormula => "[A0] + [A1] * $X + [A2] * $X^2 + [A3] * $X^3 + [A4] * $X^4 + [A5] * $X^5 + [A6] * $X^6 + [A7] * $X^7 + [A8] * $X^8";

		/// <inheritdoc/>
		public IReadOnlyList<Parameter> Parameters => parameters;

		/// <inheritdoc/>
		public bool XLogScale => false;

		/// <inheritdoc/>
		public bool YLogScale => false;

		/// <inheritdoc/>
		public Func<double, double> GetFunction(IReadOnlyList<double> parameters)
		{
			var a0 = parameters[0];
			var a1 = parameters[1];
			var a2 = parameters[2];
			var a3 = parameters[3];
			var a4 = parameters[4];
			var a5 = parameters[5];
			var a6 = parameters[6];
			var a7 = parameters[7];
			var a8 = parameters[8];

			return (x) =>
			{
				var x1 = x;
				var x2 = x1 * x;
				var x3 = x2 * x;
				var x4 = x3 * x;
				var x5 = x4 * x;
				var x6 = x5 * x;
				var x7 = x6 * x;
				var x8 = x7 * x;
				return a0 + a1 * x1 + a2 * x2 + a3 * x3 + a4 * x4 + a5 * x5 + a6 * x6 + a7 * x7 + a8 * x8;
			};
		} // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)

		/// <inheritdoc/>
		public Func<AvxVector2048, AvxVector2048> GetVectorizedFunc(IReadOnlyList<double> parameters)
			=> x => 
			{
				var length = x.Length << 2;
				var temp = new AvxVector2048(length);
				var temp_x = new AvxVector2048(length, 1.0);
				var a0 = new AvxVector2048(length, parameters[0]);

				var a1 = parameters[1];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a1, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a2 = parameters[2];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a2, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a3 = parameters[3];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a3, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a4 = parameters[4];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a4, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a5 = parameters[5];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a5, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a6 = parameters[6];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a6, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a7 = parameters[7];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a7, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a8 = parameters[8];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a8, temp);
				AvxVector2048.Add(temp, a0, a0);

				return a0;
			};

		/// <inheritdoc/>
		public Action<double, double[]> GetDerivatives(IReadOnlyList<double> parameters)
			=> Derivatives;

		private void Derivatives(double x, double[] res)
		{
			var d_a0 = 1.0;
			var d_a1 = x;
			var d_a2 = d_a1 * x;
			var d_a3 = d_a2 * x;
			var d_a4 = d_a3 * x;
			var d_a5 = d_a4 * x;
			var d_a6 = d_a5 * x;
			var d_a7 = d_a6 * x;
			var d_a8 = d_a7 * x;

			res[0] = d_a0;
			res[1] = d_a1;
			res[2] = d_a2;
			res[3] = d_a3;
			res[4] = d_a4;
			res[5] = d_a5;
			res[6] = d_a6;
			res[7] = d_a7;
			res[8] = d_a8;
		} // private void Derivatives (double, double[])

		/// <inheritdoc/>
		public Action<AvxVector2048, AvxVector2048[]> GetVectorizedDerivatives(IReadOnlyList<double> parameters)
			=> (x, res) =>
			{
				res[0].Load(1.0);
				res[1] = x;
				AvxVector2048.Multiply(res[1], x, res[2]);
				AvxVector2048.Multiply(res[2], x, res[3]);
				AvxVector2048.Multiply(res[3], x, res[4]);
				AvxVector2048.Multiply(res[4], x, res[5]);
				AvxVector2048.Multiply(res[5], x, res[6]);
				AvxVector2048.Multiply(res[6], x, res[7]);
				AvxVector2048.Multiply(res[7], x, res[8]);
			};
	} // internal partial class Polynomial8 : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>
} // namespaceTAFitting.Model.Polynomial


namespace TAFitting.Model.Polynomial
{

	/// <summary>
	/// Represents a 9th-order polynomial model.
	/// </summary>
	internal partial class Polynomial9 : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>
	{
		private static readonly Parameter[] parameters = [
			new() { Name = "A0", InitialValue = 1e+9, IsMagnitude = true },
			new() { Name = "A1", InitialValue = -1e+8, IsMagnitude = true },
			new() { Name = "A2", InitialValue = 1e+7, IsMagnitude = true },
			new() { Name = "A3", InitialValue = -1e+6, IsMagnitude = true },
			new() { Name = "A4", InitialValue = 1e+5, IsMagnitude = true },
			new() { Name = "A5", InitialValue = -1e+4, IsMagnitude = true },
			new() { Name = "A6", InitialValue = 1e+3, IsMagnitude = true },
			new() { Name = "A7", InitialValue = -1e+2, IsMagnitude = true },
			new() { Name = "A8", InitialValue = 1e+1, IsMagnitude = true },
			new() { Name = "A9", InitialValue = -1e+0, IsMagnitude = true },
		];

		/// <inheritdoc/>
		public string Name => "Poly9";

		/// <inheritdoc/>
		public string Description => "9th-order polynomial model";

		/// <inheritdoc/>
		public string ExcelFormula => "[A0] + [A1] * $X + [A2] * $X^2 + [A3] * $X^3 + [A4] * $X^4 + [A5] * $X^5 + [A6] * $X^6 + [A7] * $X^7 + [A8] * $X^8 + [A9] * $X^9";

		/// <inheritdoc/>
		public IReadOnlyList<Parameter> Parameters => parameters;

		/// <inheritdoc/>
		public bool XLogScale => false;

		/// <inheritdoc/>
		public bool YLogScale => false;

		/// <inheritdoc/>
		public Func<double, double> GetFunction(IReadOnlyList<double> parameters)
		{
			var a0 = parameters[0];
			var a1 = parameters[1];
			var a2 = parameters[2];
			var a3 = parameters[3];
			var a4 = parameters[4];
			var a5 = parameters[5];
			var a6 = parameters[6];
			var a7 = parameters[7];
			var a8 = parameters[8];
			var a9 = parameters[9];

			return (x) =>
			{
				var x1 = x;
				var x2 = x1 * x;
				var x3 = x2 * x;
				var x4 = x3 * x;
				var x5 = x4 * x;
				var x6 = x5 * x;
				var x7 = x6 * x;
				var x8 = x7 * x;
				var x9 = x8 * x;
				return a0 + a1 * x1 + a2 * x2 + a3 * x3 + a4 * x4 + a5 * x5 + a6 * x6 + a7 * x7 + a8 * x8 + a9 * x9;
			};
		} // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)

		/// <inheritdoc/>
		public Func<AvxVector2048, AvxVector2048> GetVectorizedFunc(IReadOnlyList<double> parameters)
			=> x => 
			{
				var length = x.Length << 2;
				var temp = new AvxVector2048(length);
				var temp_x = new AvxVector2048(length, 1.0);
				var a0 = new AvxVector2048(length, parameters[0]);

				var a1 = parameters[1];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a1, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a2 = parameters[2];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a2, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a3 = parameters[3];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a3, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a4 = parameters[4];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a4, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a5 = parameters[5];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a5, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a6 = parameters[6];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a6, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a7 = parameters[7];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a7, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a8 = parameters[8];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a8, temp);
				AvxVector2048.Add(temp, a0, a0);

				var a9 = parameters[9];
				AvxVector2048.Multiply(x, temp_x, temp_x);
				AvxVector2048.Multiply(temp_x, a9, temp);
				AvxVector2048.Add(temp, a0, a0);

				return a0;
			};

		/// <inheritdoc/>
		public Action<double, double[]> GetDerivatives(IReadOnlyList<double> parameters)
			=> Derivatives;

		private void Derivatives(double x, double[] res)
		{
			var d_a0 = 1.0;
			var d_a1 = x;
			var d_a2 = d_a1 * x;
			var d_a3 = d_a2 * x;
			var d_a4 = d_a3 * x;
			var d_a5 = d_a4 * x;
			var d_a6 = d_a5 * x;
			var d_a7 = d_a6 * x;
			var d_a8 = d_a7 * x;
			var d_a9 = d_a8 * x;

			res[0] = d_a0;
			res[1] = d_a1;
			res[2] = d_a2;
			res[3] = d_a3;
			res[4] = d_a4;
			res[5] = d_a5;
			res[6] = d_a6;
			res[7] = d_a7;
			res[8] = d_a8;
			res[9] = d_a9;
		} // private void Derivatives (double, double[])

		/// <inheritdoc/>
		public Action<AvxVector2048, AvxVector2048[]> GetVectorizedDerivatives(IReadOnlyList<double> parameters)
			=> (x, res) =>
			{
				res[0].Load(1.0);
				res[1] = x;
				AvxVector2048.Multiply(res[1], x, res[2]);
				AvxVector2048.Multiply(res[2], x, res[3]);
				AvxVector2048.Multiply(res[3], x, res[4]);
				AvxVector2048.Multiply(res[4], x, res[5]);
				AvxVector2048.Multiply(res[5], x, res[6]);
				AvxVector2048.Multiply(res[6], x, res[7]);
				AvxVector2048.Multiply(res[7], x, res[8]);
				AvxVector2048.Multiply(res[8], x, res[9]);
			};
	} // internal partial class Polynomial9 : IFittingModel, IAnalyticallyDifferentiable, IVectorizedModel<AvxVector2048>
} // namespaceTAFitting.Model.Polynomial

