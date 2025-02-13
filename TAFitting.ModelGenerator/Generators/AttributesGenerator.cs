﻿
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.ModelGenerator.Generators;

[Generator(LanguageNames.CSharp)]
internal sealed class AttributesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource("ExponentialModelAttribute.g.cs", ExponentialModelSource);
            context.AddSource("PolynomialModelAttribute.g.cs", PolynomialModelSource);
        });
    } // public void Initialize (IncrementalGeneratorInitializationContext)

    #region sources

    internal const string ExponentialModelName = "ExponentialModelAttribute";

    internal const string PolynomialModelName = "PolynomialModelAttribute";

    private const string ExponentialModelSource = @"// <auto-generated/>

using System;

#nullable enable

namespace TAFitting.Model;

/// <summary>
/// An exponential model.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal sealed class ExponentialModelAttribute : Attribute
{
    /// <summary>
    /// Gets the number of components.
    /// </summary>
    internal int ComponentsCount { get; }

    /// <summary>
    /// Gets or sets the name of the model.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref=""ExponentialModelAttribute""/> class.
    /// </summary>
    /// <param name=""componentsCount""></param>
    /// <exception cref=""ArgumentOutOfRangeException""></exception>
    internal ExponentialModelAttribute(int componentsCount)
    {
        if (componentsCount < 1)
            throw new ArgumentOutOfRangeException(nameof(componentsCount), componentsCount, ""Components count must be greater than or equal to 1."");
        this.ComponentsCount = componentsCount;
    } // ctor (int)
} // internal sealed class ExponentialModelAttribute : Attribute
";

    private const string PolynomialModelSource = @"// <auto-generated/>

using System;

#nullable enable

namespace TAFitting.Model;

/// <summary>
/// A polynomial model.
/// </summary>
internal sealed class PolynomialModelAttribute : Attribute
{
    /// <summary>
    /// 
    /// </summary>
    internal int Order { get; }

    /// <summary>
    /// Gets or sets the name of the model.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref=""PolynomialModelAttribute""/> class.
    /// </summary>
    /// <param name=""order"">The order of the polynomial.</param>
    /// <exception cref=""ArgumentOutOfRangeException"">Order must be greater than or equal to 1.</exception>
    internal PolynomialModelAttribute(int order)
    {
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), order, ""Order must be greater than or equal to 1."");
        this.Order = order;
    } // ctor (int)
} // internal sealed class PolynomialModelAttribute : Attribute
";
    #endregion sources
} // internal sealed class AttributesGenerator : IIncrementalGenerator
