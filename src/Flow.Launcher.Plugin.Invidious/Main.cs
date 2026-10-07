using System.Windows.Controls;

namespace Flow.Launcher.Plugin.Invidious;

public class Main : IAsyncPlugin, ISettingProvider, IContextMenu
{
    private const string IconPath = "Icons\\Invidious-logo.svg";

    private PluginInitContext _context = null!;
    private Settings _settings = null!;
    private InvidiousClient _client = null!;

    public Task InitAsync(PluginInitContext context)
    {
        _context = context;
        _settings = context.API.LoadSettingJsonStorage<Settings>();
        _client = new InvidiousClient(context.API);
        return Task.CompletedTask;
    }

    public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
    {
        var search = query.Search.Trim();

        if (string.IsNullOrEmpty(_settings.InstanceUrl))
        {
            return
            [
                new Result
                {
                    Title = "No Invidious instance configured",
                    SubTitle = "Open plugin settings and set the Invidious instance URL",
                    IcoPath = IconPath,
                    Action = _ =>
                    {
                        _context.API.OpenSettingDialog();
                        return false;
                    },
                },
            ];
        }

        if (string.IsNullOrEmpty(search))
        {
            return
            [
                new Result
                {
                    Title = "Search Invidious",
                    SubTitle = $"Type to search videos, channels and playlists on {_settings.InstanceUrl}",
                    IcoPath = IconPath,
                },
            ];
        }

        IReadOnlyList<InvidiousSearchItem> items;
        try
        {
            items = await _client.SearchAsync(_settings.InstanceUrl, search, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _context.API.LogException(nameof(Main), "Invidious search failed", ex);
            return
            [
                new Result
                {
                    Title = "Invidious search failed",
                    SubTitle = ex.Message,
                    IcoPath = IconPath,
                },
            ];
        }

        token.ThrowIfCancellationRequested();

        var results = new List<Result>(items.Count);
        foreach (var item in items)
        {
            var result = item switch
            {
                InvidiousVideo video => BuildVideoResult(video),
                InvidiousPlaylist playlist => BuildPlaylistResult(playlist),
                InvidiousChannel channel => BuildChannelResult(channel),
                _ => null,
            };

            if (result is not null)
            {
                results.Add(result);
            }
        }

        return results;
    }

    public Control CreateSettingPanel() => new SettingsControl(_settings, _context.API);

    public List<Result> LoadContextMenus(Result selectedResult)
    {
        if (selectedResult.ContextData is not VideoContext video)
        {
            return [];
        }

        return
        [
            new Result
            {
                Title = "Copy Invidious link",
                SubTitle = video.InvidiousUrl,
                IcoPath = IconPath,
                Action = _ =>
                {
                    _context.API.CopyToClipboard(video.InvidiousUrl);
                    return true;
                },
            },
        ];
    }

    private Result BuildVideoResult(InvidiousVideo video)
    {
        var invidiousUrl = $"{_settings.InstanceUrl}/watch?v={video.VideoId}";
        var date = FormatDate(video.Published);
        var subtitle = video.LiveNow
            ? $"{video.Author} • LIVE" + (date is null ? "" : $" • {date}")
            : $"{video.Author} • {FormatDuration(video.LengthSeconds)} • {FormatCount(video.ViewCount)} views" + (date is null ? "" : $" • {date}");

        return new Result
        {
            Title = video.Title,
            SubTitle = subtitle,
            IcoPath = IconPath,
            ContextData = new VideoContext(invidiousUrl),
            Action = _ =>
            {
                _context.API.OpenUrl(invidiousUrl);
                return true;
            },
        };
    }

    private Result BuildPlaylistResult(InvidiousPlaylist playlist)
    {
        var invidiousUrl = $"{_settings.InstanceUrl}/playlist?list={playlist.PlaylistId}";

        return new Result
        {
            Title = playlist.Title,
            SubTitle = $"Playlist • {playlist.Author} • {playlist.VideoCount} videos",
            IcoPath = IconPath,
            Action = _ =>
            {
                _context.API.OpenUrl(invidiousUrl);
                return true;
            },
        };
    }

    private Result BuildChannelResult(InvidiousChannel channel)
    {
        var invidiousUrl = $"{_settings.InstanceUrl}/channel/{channel.AuthorId}";

        return new Result
        {
            Title = channel.Author,
            SubTitle = $"Channel • {FormatCount(channel.SubCount)} subscribers • {channel.VideoCount} videos",
            IcoPath = IconPath,
            Action = _ =>
            {
                _context.API.OpenUrl(invidiousUrl);
                return true;
            },
        };
    }

    private static string FormatDuration(int totalSeconds)
    {
        var time = TimeSpan.FromSeconds(Math.Max(totalSeconds, 0));
        return time.TotalHours >= 1
            ? time.ToString(@"h\:mm\:ss")
            : time.ToString(@"m\:ss");
    }

    private static string? FormatDate(long publishedUnixSeconds) =>
        publishedUnixSeconds > 0
            ? DateTimeOffset.FromUnixTimeSeconds(publishedUnixSeconds).UtcDateTime.ToString("yyyy-MM-dd")
            : null;

    private static string FormatCount(long count) => count.ToString("N0");

    private sealed record VideoContext(string InvidiousUrl);
}
