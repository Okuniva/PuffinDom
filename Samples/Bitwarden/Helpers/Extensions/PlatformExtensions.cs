using NUnit.Framework;
using PuffinDom.Infrastructure;
using PuffinDom.Infrastructure.Helpers;

namespace Bitwarden.Helpers.Extensions;

public static class PlatformExtensions
{
    public static void ThrowIfPlatformIsNotEnabled(this RunOn platform, string? reason = null)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (platform)
        {
            case RunOn.AndroidOnly when !CoreEnvironmentVariables.RunDroid:
            case RunOn.AndroidOnly_iOSInDevelopment when !CoreEnvironmentVariables.RunDroid:
                throw new IgnoreException(
                    reason == null
                        ? BitwardenConstants.iOSTurnedOff
                        : $"{BitwardenConstants.iOSTurnedOff} | {reason}");
            case RunOn.iOSOnly when !CoreEnvironmentVariables.RunIOS:
            case RunOn.iOSOnly_AndroidInDevelopment when !CoreEnvironmentVariables.RunIOS:
                throw new IgnoreException(
                    reason == null
                        ? BitwardenConstants.AndroidTurnedOff
                        : $"{BitwardenConstants.AndroidTurnedOff} | {reason}");
            case RunOn.AllPlatforms:
                break;
            case RunOn.Ignore:
                throw new IgnoreException(
                    reason == null
                        ? BitwardenConstants.TestInDevelopment
                        : $"{BitwardenConstants.TestInDevelopment} | {reason}");
        }
    }
}