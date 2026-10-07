# Flow Launcher plugin: Invidious

A Flow Launcher plugin for searching videos, channels and playlists
through an [Invidious](https://invidious.io) instance.

## Usage

- Action keyword: `iv`
- Open the plugin settings (Flow Settings → Plugins → Invidious) and set
  the Invidious instance URL. The plugin does nothing until this is set.
- `iv <search term>` searches videos, channels and playlists.
- Enter opens the result on the configured Invidious instance.
- Right-click/context menu on a video result: copy the Invidious link.

## Build

```
dotnet publish src\Flow.Launcher.Plugin.Invidious -c Release -r win-x64 --no-self-contained

powershell -NoProfile -Command ^
  "Compress-Archive -Path 'src\Flow.Launcher.Plugin.Invidious\bin\Release\win-x64\publish\*' -DestinationPath 'dist\Flow.Launcher.Plugin.Invidious.zip' -Force"
```
</content>
