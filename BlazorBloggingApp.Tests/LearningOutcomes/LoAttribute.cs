using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace BlazorBloggingApp.Tests.LearningOutcomes;

/// <summary>
/// Tags a test as encoding a course learning outcome. Every claim a clip makes
/// on camera is pinned by at least one [Lo] test, so a green suite means the
/// course's teachable claims are actually true of the demo code.
///
/// Filter by objective or clip:
///   dotnet test --filter "LO=TO3"
///   dotnet test --filter "Clip=M2C1"
/// </summary>
[TraitDiscoverer("BlazorBloggingApp.Tests.LearningOutcomes.LoDiscoverer", "BlazorBloggingApp.Tests")]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class LoAttribute(string objective, string clip) : Attribute, ITraitAttribute
{
    public string Objective { get; } = objective;
    public string Clip { get; } = clip;
}

public sealed class LoDiscoverer : ITraitDiscoverer
{
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        var args = traitAttribute.GetConstructorArguments().ToList();
        yield return new KeyValuePair<string, string>("LO", (string)args[0]!);
        yield return new KeyValuePair<string, string>("Clip", (string)args[1]!);
    }
}
