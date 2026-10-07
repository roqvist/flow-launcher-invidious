using System.Windows;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.Invidious;

public partial class SettingsControl : UserControl
{
    private readonly Settings _settings;
    private readonly IPublicAPI _api;

    public SettingsControl(Settings settings, IPublicAPI api)
    {
        _settings = settings;
        _api = api;
        InitializeComponent();
        InstanceUrlTextBox.Text = _settings.InstanceUrl;
    }

    private void InstanceUrlTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        _settings.InstanceUrl = InstanceUrlTextBox.Text.Trim().TrimEnd('/');
        _api.SaveSettingJsonStorage<Settings>();
    }
}
