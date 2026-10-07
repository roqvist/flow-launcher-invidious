namespace Flow.Launcher.Plugin.Invidious;

/// <summary>
/// Discriminator for the polymorphic entries returned by Invidious' /api/v1/search.
/// </summary>
internal enum InvidiousResultType
{
    Video,
    Playlist,
    Channel,
    Hashtag,
    Unknown,
}

internal abstract record InvidiousSearchItem(InvidiousResultType Type);

internal sealed record InvidiousVideo(
    string Title,
    string VideoId,
    string Author,
    string AuthorId,
    long ViewCount,
    int LengthSeconds,
    long Published,
    bool LiveNow) : InvidiousSearchItem(InvidiousResultType.Video);

internal sealed record InvidiousPlaylist(
    string Title,
    string PlaylistId,
    string Author,
    string AuthorId,
    int VideoCount) : InvidiousSearchItem(InvidiousResultType.Playlist);

internal sealed record InvidiousChannel(
    string Author,
    string AuthorId,
    int SubCount,
    int VideoCount,
    string Description) : InvidiousSearchItem(InvidiousResultType.Channel);
