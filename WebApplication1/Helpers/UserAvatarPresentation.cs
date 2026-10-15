using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineLearningPlatform.Helpers;

/// <summary>Presentation-only avatar helpers; no Identity or account data changes.</summary>
public static class UserAvatarPresentation
{
    /// <summary>First Unicode text element of the last non-empty name word.</summary>
    public static string? GetLastWordInitial(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return null;
        var parts = fullName.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return null;
        var lastWord = parts[^1];
        var initial = StringInfo.GetNextTextElement(lastWord, 0);
        return char.IsLetter(initial, 0) ? initial.ToUpperInvariant() : null;
    }

    /// <summary>Resolve an existing avatar path for an img tag without changing stored values.</summary>
    public static string? ResolveImageUrl(IUrlHelper url, string? storedPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath)) return null;
        var raw = storedPath.Trim().Replace('\\', '/');
        if (raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            return raw;
        if (raw.StartsWith("//", StringComparison.Ordinal) ||
            raw.Contains("..", StringComparison.Ordinal) ||
            raw.Contains(':', StringComparison.Ordinal))
            return null;
        if (raw.StartsWith("~/", StringComparison.Ordinal)) return url.Content(raw);
        if (raw.StartsWith("/", StringComparison.Ordinal)) return url.Content("~" + raw);
        if (raw.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
            raw = raw["wwwroot/".Length..];
        return url.Content("~/" + raw);
    }
}
