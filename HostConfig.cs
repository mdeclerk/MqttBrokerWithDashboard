using System.IO;
using Newtonsoft.Json;

namespace MqttBrokerWithDashboard
{
    public class HostConfig
    {
        public static string Filename = $"{nameof(HostConfig)}.json";

        public bool HideConfigPanel = false;

        public int TcpPort = 1883;

        public int HttpPort = 5000;

        public static HostConfig LoadFromFile()
        {
            if (File.Exists(Filename))
            {
                using var file = File.OpenText(Filename);
                return (HostConfig)new JsonSerializer().Deserialize(file, typeof(HostConfig));
            }
            return new HostConfig();
        }

        public void SaveToFile()
        {
            using var file = File.CreateText(Filename);
            new JsonSerializer { Formatting = Formatting.Indented }.Serialize(file, this);
        }
    }
}