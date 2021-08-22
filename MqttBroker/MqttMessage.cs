using System;
using MQTTnet;

namespace MqttBrokerWithDashboard.MqttBroker
{
    public class MqttMessage
    {
        public DateTime Timestamp { get; set; }
        public MqttClient Client { get; set; }
        public string Topic { get; set; }
        public string Payload { get; set; }
        public MqttApplicationMessage Original { get; set; }
    }
}