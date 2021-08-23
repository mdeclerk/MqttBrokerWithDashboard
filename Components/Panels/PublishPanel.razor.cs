using Microsoft.AspNetCore.Components;
using MqttBrokerWithDashboard.MqttBroker;

namespace MqttBrokerWithDashboard.Components.Panels
{
    public partial class PublishPanel : ComponentBase
    {
        [Inject] MqttBrokerService _mqtt { get; set; }

        string _topic = "MyTopic";

        string _payload = "MyPayload";

        bool _retained;

        bool IsPublishDisabled => string.IsNullOrWhiteSpace(_topic) || string.IsNullOrWhiteSpace(_payload);

        void Publish() => _mqtt.Publish(_topic, _payload, _retained);
    }
}