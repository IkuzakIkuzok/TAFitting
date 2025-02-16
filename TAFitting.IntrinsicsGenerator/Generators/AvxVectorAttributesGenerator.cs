﻿
// (c) 2024 Kazuki Kohzuki

namespace TAFitting.IntrinsicsGenerator.Generators;

[Generator(LanguageNames.CSharp)]
internal sealed class AvxVectorAttributesGenerator : IIncrementalGenerator
{
    internal const string Namespace = "TAFitting.Data";
    internal const string AttributeName = "AvxVectorAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource("AvxVectorAttribute.g.cs", AvxVectorAttributeSource);
        });
    } // public void Initialize (IncrementalGeneratorInitializationContext)

    private const string AvxVectorAttributeSource = @"// <auto-generated/>

using System;
using System.Runtime.Intrinsics;

#nullable enable

namespace TAFitting.Data;

/// <summary>
/// An AVX vector.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal sealed class AvxVectorAttribute : Attribute
{
    /// <summary>
    /// Gets the number of components.
    /// </summary>
    internal int Count { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref=""AvxVectorAttribute""/> class.
    /// </summary>
    internal AvxVectorAttribute(int count)
    {
        if (count < 1)
            throw new ArgumentOutOfRangeException(nameof(count), count, ""Count must be greater than or equal to 1."");
        
        if (count % Vector256<double>.Count != 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, ""Count must be a multiple of 4."");

        this.Count = count;
    } // ctor (int)
} // internal sealed class AvxVectorAttribute : Attribute
";
} // internal sealed class IIncrementalGenerator
