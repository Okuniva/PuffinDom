using System.Text.RegularExpressions;
using PuffinDom.UI.Exceptions;

namespace PuffinDom.UI.Extensions;

public static class StringExtensions
{
    public static bool CompareToStringWithPlaceholders(this string value, string stringToCompare)
    {
        var stringToCompareRegexed = stringToCompare.ReplacePlaceholdersWithRegexAnyAndSafeRegexKeywords();
        return Regex.Match(
                value,
                stringToCompareRegexed)
            .Success;
    }

    public static string ReplacePlaceholdersWithRegexAnyAndSafeRegexKeywords(this string value)
    {
        const string temporaryRegexReplacement =
            "any_text_placeholder_to_replace_other_regex_keywords_correctly";

        return Regex
            .Replace(value, "{\\d}", temporaryRegexReplacement)
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace(".", "\\.")
            .Replace("+", "\\+")
            .Replace("*", "\\*")
            .Replace("?", "\\?")
            .Replace("^", "\\^")
            .Replace("$", "\\$")
            .Replace("|", "\\|")
            .Replace(temporaryRegexReplacement, ".*")
            .Replace("{", "\\{")
            .Replace("}", "\\}");
    }

    private static string ConvertPlaceholdersToRegexGroups(this string value)
    {
        return $"^{value.Replace(".*", "(.*)")}$";
    }

    public static List<string> GetValuesFromPlaceholdersOfStringWithPattern(this string value, string pattern)
    {
        var list = new List<string>();

        pattern = pattern
            .ReplacePlaceholdersWithRegexAnyAndSafeRegexKeywords()
            .ConvertPlaceholdersToRegexGroups();

        var match = Regex.Match(value, pattern);

        if (!match.Success)
            return list;

        for (var i = 1; i < match.Groups.Count; i++)
            list.Add(match.Groups[i].Value);

        return list;
    }
}