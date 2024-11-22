﻿
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.SourceGeneratorUtils;

[Generator(LanguageNames.CSharp)]
internal sealed class StringUtilsGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        context.AddSource("StringUtils.g.cs", StringUtilsSource);
    } // public void Execute (GeneratorExecutionContext)

    private const string StringUtilsSource = @"// <auto-generated/>

namespace TAFitting.SourceGeneratorUtils;

/// <summary>
/// Provides extension methods for <see cref=""string""/>.
/// </summary>
internal static class StringUtils
{
    /// <summary>
    /// Normalizes new lines.
    /// </summary>
    /// <param name=""text"">The text.</param>
    /// <returns>The normalized text.</returns>
    internal static string NormalizeNewLines(this string text)
        => text.Replace(""\r\n"", ""\n"").Replace(""\r"", ""\n"").Replace(""\n"", ""\r\n"");
} // internal static class StringUtils
";
} // internal sealed class StringUtilsGenerator : ISourceGenerator
