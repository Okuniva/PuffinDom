namespace Bitwarden.Helpers;

public class LocalTestContext
{
    private static LocalTestContext? _instance;

    private LocalTestContext()
    {
    }

    public static LocalTestContext Instance => _instance ??= new LocalTestContext();

    public static void Reset()
    {
        _instance = null;
    }
}