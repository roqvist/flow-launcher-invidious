namespace Flow.Launcher.Plugin.Invidious;

/// <summary>
/// Persisted plugin configuration. Loaded/saved via <see cref="IPublicAPI.LoadSettingJsonStorage{T}"/>.
/// </summary>
public class Settings
{
    /// <summary>
    /// Base URL of the Invidious instance to query, e.g. "https://yewtu.be".
    /// No trailing slash. Must be configured by the user before the plugin can search.
    /// </summary>
    public string InstanceUrl { get; set; } = string.Empty;
}
