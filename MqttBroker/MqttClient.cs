using System;

namespace MqttBrokerWithDashboard.MqttBroker
{
    public class MqttClient
    {
        public DateTime TimeOfConnection { get; set; }
        public string ClientId { get; set; }
        public bool AllowSend { get; set; }
        public bool AllowReceive { get; set; }
    }
}