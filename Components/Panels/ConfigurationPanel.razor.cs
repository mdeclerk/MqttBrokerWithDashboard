using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MqttBrokerWithDashboard.Components.Dialogs;
using MqttBrokerWithDashboard.MqttBroker;
using MudBlazor;

namespace MqttBrokerWithDashboard.Components.Panels
{
    public partial class ConfigurationPanel : ComponentBase
    {
        [Inject] MqttBrokerService _mqtt { get; set; }

        [Inject] ILogger<ConfigurationPanel> _log { get; set; }

        [Inject] IDialogService _dlg { get; set; }

        bool _hidePanel;

        int _tcpPort;

        int _httpPort;

        bool IsDirty =>
            _hidePanel != Program.HostConfig.HideConfigPanel ||
            _tcpPort != Program.HostConfig.TcpPort ||
            _httpPort != Program.HostConfig.HttpPort;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Reset();
        }

        void Reset()
        {
            _hidePanel = Program.HostConfig.HideConfigPanel;
            _tcpPort = Program.HostConfig.TcpPort;
            _httpPort = Program.HostConfig.HttpPort;
        }

        void SaveToFile()
        {
            Program.HostConfig.HideConfigPanel = _hidePanel;
            Program.HostConfig.TcpPort = _tcpPort;
            Program.HostConfig.HttpPort = _httpPort;

            Program.HostConfig.SaveToFile();
        }

        async Task SaveAndRestart()
        {
            var parameters = new DialogParameters();
            parameters.Add("ContentText", "Do you really want to save changes and restart server?");
            parameters.Add("ButtonText", "Save & Restart");
            parameters.Add("Color", Color.Error);

            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Small };

            var result = await _dlg.Show<ConfirmationDialog>("Save & Restart?", parameters, options).Result;
            if (!result.Cancelled)
            {
                _log.LogWarning("Save config and restart server ...");
                SaveToFile();
                Program.RestartHost();
            }
        }

        async Task HidePanelChanged(bool newValue)
        {
            if (newValue)
            {
                var parameters = new DialogParameters();
                parameters.Add("ContentText", $"Do you really want to hide the Configuration Panel at startup? This option can only be undone in '{HostConfig.Filename}' when saved.");
                parameters.Add("ButtonText", "Ok");
                parameters.Add("Color", Color.Error);

                var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Small };

                var result = await _dlg.Show<ConfirmationDialog>("Hide Configuration Panel?", parameters, options).Result;
                if (result.Cancelled) return;
            }

            _hidePanel = newValue;
        }
    }
}