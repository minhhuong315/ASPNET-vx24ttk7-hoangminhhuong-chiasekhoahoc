using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace OnlineLearningPlatform.Helpers;

/// <summary>
/// Presentation-only thumbnail lookup. All roles use the same Course.ThumbnailUrl
/// and the same existing Guest image directory keyed by Course.Slug.
/// </summary>
public static class CourseThumbnailResolver
{
    private const string PlaceholderPath =
        "~/images/course-thumbnails/by-slug/course-placeholder.svg";

    public const string OnErrorHandler =
        "if(this.dataset.edThumbStep==='1'){this.onerror=null;this.src=this.dataset.edThumbPlaceholder;}" +
        "else{this.dataset.edThumbStep='1';this.src=this.dataset.edThumbFallback||this.dataset.edThumbPlaceholder;}";

    public readonly record struct ThumbnailSources(
        string Source,
        string Fallback,
        string Placeholder);

    public static ThumbnailSources Resolve(
        IUrlHelper url,
        IWebHostEnvironment environment,
        string? storedUrl,
        string? slug)
    {
        var placeholder = url.Content(PlaceholderPath);
        var guestPath = GetExistingGuestThumbnail(url, environment, slug);
        var stored = GetStoredThumbnail(url, environment, storedUrl);

        var source = stored ?? guestPath ?? placeholder;
        var fallback = source == guestPath
            ? placeholder
            : guestPath ?? placeholder;

        return new ThumbnailSources(source, fallback, placeholder);
    }

    private static string? GetExistingGuestThumbnail(
        IUrlHelper url,
        IWebHostEnvironment environment,
        string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug) ||
            !System.Text.RegularExpressions.Regex.IsMatch(
                slug, "^[a-zA-Z0-9-]+$"))
            return null;

        var filePath = $"images/course-thumbnails/by-slug/{slug}.png";
        return environment.WebRootFileProvider.GetFileInfo(filePath).Exists
            ? url.Content("~/" + filePath)
            : null;
    }

    private static string? GetStoredThumbnail(
        IUrlHelper url,
        IWebHostEnvironment environment,
        string? storedUrl)
    {
        if (string.IsNullOrWhiteSpace(storedUrl))
            return null;

        var raw = storedUrl.Trim().Replace('\\', '/');
        if (raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            return raw;

        if (raw.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            return raw;

        // App-local paths only. Do not turn an invalid URL into a broken image.
        var path = raw.Split('?', '#')[0];
        if (path.StartsWith("~/", StringComparison.Ordinal)) path = path[2..];
        else if (path.StartsWith("/", StringComparison.Ordinal)) path = path[1..];
        if (path.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
            path = path["wwwroot/".Length..];

        if (path.Length == 0 || path.Contains("..", StringComparison.Ordinal) ||
            path.Contains(':') || path.StartsWith("//", StringComparison.Ordinal))
            return null;

        return environment.WebRootFileProvider.GetFileInfo(path).Exists
            ? url.Content("~/" + path)
            : null;
    }
}
