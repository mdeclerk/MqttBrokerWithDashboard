using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using MqttBrokerWithDashboard.MqttBroker;
using MQTTnet;

namespace MqttBrokerWithDashboard.Components.Panels
{
    public partial class MessagesPanel : ComponentBase, IDisposable
    {
        [Inject] MqttBrokerService _mqtt { get; set; }

        string _searchString = "";

        bool _collapseByTopic = true;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _mqtt.OnMessageReceived += OnMessageReceived;
        }

        public void Dispose()
        {
            _mqtt.OnMessageReceived -= OnMessageReceived;
        }

        void OnMessageReceived(MqttApplicationMessageReceivedEventArgs e) =>
            InvokeAsync(StateHasChanged);

        IEnumerable<MqttMessage> GetItems()
        {
            if (_collapseByTopic)
                return _mqtt.MessagesByTopic.Values.Select(x => x[0]);
            return _mqtt.Messages;
        }

        bool FilterFunc(MqttMessage message)
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;
            if (message.Client != null && message.Client.ClientId.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (message.Topic != null && message.Topic.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (message.Payload != null && message.Payload.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        string GetClientId(MqttMessage message) => message.Client?.ClientId ?? "SERVER";
    }
}