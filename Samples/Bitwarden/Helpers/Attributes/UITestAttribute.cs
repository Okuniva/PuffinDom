using NUnit.Framework;
using PuffinDom.Infrastructure.Helpers;

namespace Bitwarden.Helpers.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class UITestAttribute : TheoryAttribute
{
    public UITestAttribute(
        string testRailName,
        RunOn platform = RunOn.AllPlatforms,
        string? platformIgnoringReasonMessage = null)
    {
        TestRail = testRailName;
        TestPlatform = platform;
        PlatformIgnoringReasonMessage = platformIgnoringReasonMessage;
    }

    public string? PlatformIgnoringReasonMessage { get; }

    public string TestRail { get; }
    public RunOn TestPlatform { get; }
}