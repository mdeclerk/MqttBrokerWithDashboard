using System;
using Microsoft.AspNetCore.Components;
using MqttBrokerWithDashboard.MqttBroker;
using MQTTnet.Server;

namespace MqttBrokerWithDashboard.Components.Panels
{
    public partial class ClientsPanel : ComponentBase, IDisposable
    {
        [Inject] MqttBrokerService _mqtt { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _mqtt.OnClientConnected += OnClientConnected;
            _mqtt.OnClientDisconnected += OnClientDisconnected;
        }

        public void Dispose()
        {
            _mqtt.OnClientConnected -= OnClientConnected;
            _mqtt.OnClientDisconnected -= OnClientDisconnected;
        }

        void OnClientConnected(MqttServerClientConnectedEventArgs e) =>
            InvokeAsync(StateHasChanged);

        void OnClientDisconnected(MqttServerClientDisconnectedEventArgs e) =>
            InvokeAsync(StateHasChanged);
    }
}