using System.Text.Json;

namespace Flow.Launcher.Plugin.Invidious;

/// <summary>
/// Thin wrapper around Invidious' public search API.
/// Uses Flow's <see cref="IPublicAPI.HttpGetStringAsync"/> so proxy/user-agent settings configured
/// in Flow Launcher are respected, and .NET's default dual-stack connection logic picks whichever
/// IP family (v4/v6) resolves and connects.
/// </summary>
internal sealed class InvidiousClient(IPublicAPI api)
{
    /// <summary>
    /// Searches the given Invidious instance. Returns an empty list on malformed entries;
    /// network/HTTP errors are thrown to the caller.
    /// </summary>
    public async Task<IReadOnlyList<InvidiousSearchItem>> SearchAsync(
        string instanceUrl,
        string query,
        CancellationToken token)
    {
        var baseUrl = instanceUrl.TrimEnd('/');
        var url = $"{baseUrl}/api/v1/search?q={Uri.EscapeDataString(query)}&type=all";

        var json = await api.HttpGetStringAsync(url, token).ConfigureAwait(false);

        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var results = new List<InvidiousSearchItem>();
        foreach (var element in document.RootElement.EnumerateArray())
        {
            var item = ParseItem(element);
            if (item is not null)
            {
                results.Add(item);
            }
        }

        return results;
    }

    private static InvidiousSearchItem? ParseItem(JsonElement element)
    {
        var type = element.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : null;

        return type switch
        {
            "video" => new InvidiousVideo(
                Title: GetString(element, "title"),
                VideoId: GetString(element, "videoId"),
                Author: GetString(element, "author"),
                AuthorId: GetString(element, "authorId"),
                ViewCount: GetInt64(element, "viewCount"),
                LengthSeconds: GetInt32(element, "lengthSeconds"),
                Published: GetInt64(element, "published"),
                LiveNow: GetBool(element, "liveNow")),

            "playlist" => new InvidiousPlaylist(
                Title: GetString(element, "title"),
                PlaylistId: GetString(element, "playlistId"),
                Author: GetString(element, "author"),
                AuthorId: GetString(element, "authorId"),
                VideoCount: GetInt32(element, "videoCount")),

            "channel" => new InvidiousChannel(
                Author: GetString(element, "author"),
                AuthorId: GetString(element, "authorId"),
                SubCount: GetInt32(element, "subCount"),
                VideoCount: GetInt32(element, "videoCount"),
                Description: GetString(element, "description")),

            _ => null,
        };
    }

    private static string GetString(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static int GetInt32(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetInt32()
            : 0;

    private static long GetInt64(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetInt64()
            : 0;

    private static bool GetBool(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) &&
        (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False) &&
        value.GetBoolean();
}
