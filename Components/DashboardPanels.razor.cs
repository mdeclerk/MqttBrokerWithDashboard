using Microsoft.AspNetCore.Components;
using MqttBrokerWithDashboard.MqttBroker;
using MQTTnet;
using MQTTnet.Server;

namespace MqttBrokerWithDashboard.Components
{
    public partial class DashboardPanels : ComponentBase
    {
        [Inject] MqttBrokerService _mqtt { get; set; }

        int _numberOfUnseenMessages = 0;

        bool _isMessagesPanelExpanded;
        bool IsMessagesPanelExpanded
        {
            get => _isMessagesPanelExpanded;

            set
            {
                if (value)
                    _numberOfUnseenMessages = 0;
                _isMessagesPanelExpanded = value;
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _mqtt.OnClientConnected += OnClientConnected;
            _mqtt.OnClientDisconnected += OnClientDisconnected;
            _mqtt.OnMessageReceived += OnMessageReceived;
        }

        public void Dispose()
        {
            _mqtt.OnClientConnected -= OnClientConnected;
            _mqtt.OnClientDisconnected -= OnClientDisconnected;
            _mqtt.OnMessageReceived -= OnMessageReceived;
        }

        void OnClientConnected(MqttServerClientConnectedEventArgs e) =>
            InvokeAsync(StateHasChanged);

        void OnClientDisconnected(MqttServerClientDisconnectedEventArgs e) =>
            InvokeAsync(StateHasChanged);

        void OnMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            if (!_isMessagesPanelExpanded)
                _numberOfUnseenMessages++;
            InvokeAsync(StateHasChanged);
        }
    }
}