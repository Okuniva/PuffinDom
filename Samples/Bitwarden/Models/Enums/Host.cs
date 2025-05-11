namespace Bitwarden.Models.Enums;

public enum Host
{
    BitwardenCom,
    BitwardenEu,
    SelfHosted
}

public static class HostExtensions
{
    public static string CovertToString(this Host host)
    {
        return host switch
        {
            Host.BitwardenCom => "bitwarden.com",
            Host.BitwardenEu => "bitwarden.eu",
            Host.SelfHosted => "Self-hosted",
            _ => throw new ArgumentOutOfRangeException(nameof(host), host, null)
        };
    }

    public static Host ConvertToEnum(string value)
    {
        return value switch
        {
            "bitwarden.com" => Host.BitwardenCom,
            "bitwarden.eu" => Host.BitwardenEu,
            "Self-hosted" => Host.SelfHosted,
            _ => throw new ArgumentException($"Unknown hosting type: {value}")
        };
    }
}
