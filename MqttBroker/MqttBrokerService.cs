using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Server;
using System;
using System.Text;
using System.Threading.Tasks;
using MQTTnet.Client.Receiving;
using System.Collections.Generic;

namespace MqttBrokerWithDashboard.MqttBroker
{
    public class MqttBrokerService : IMqttServerClientConnectedHandler, IMqttServerClientDisconnectedHandler, IMqttApplicationMessageReceivedHandler, IMqttServerClientMessageQueueInterceptor
    {
        readonly ILogger _log;

        public IMqttServer Server { get; set; }

        readonly object _thisLock = new();

        List<MqttMessage> _messages = new();
        public IReadOnlyList<MqttMessage> Messages
        {
            get
            {
                lock (_thisLock)
                {
                    return _messages?.AsReadOnly();
                }
            }
        }

        Dictionary<string, List<MqttMessage>> _messagesByTopic = new();
        public IReadOnlyDictionary<string, List<MqttMessage>> MessagesByTopic
        {
            get
            {
                lock (_thisLock)
                {
                    return _messagesByTopic as IReadOnlyDictionary<string, List<MqttMessage>>;
                }
            }
        }

        List<MqttClient> _connectedClients = new();
        public IReadOnlyList<MqttClient> ConnectedClients
        {
            get
            {
                lock (_thisLock)
                {
                    return _connectedClients?.AsReadOnly();
                }
            }
        }


        public event Action<MqttServerClientConnectedEventArgs> OnClientConnected;
        public event Action<MqttServerClientDisconnectedEventArgs> OnClientDisconnected;
        public event Action<MqttApplicationMessageReceivedEventArgs> OnMessageReceived;


        public MqttBrokerService(ILogger<MqttBrokerService> log) => _log = log;


        Task IMqttServerClientConnectedHandler.HandleClientConnectedAsync(MqttServerClientConnectedEventArgs e)
        {
            lock (_thisLock) _connectedClients.Add(new MqttClient
            {
                TimeOfConnection = DateTime.Now,
                ClientId = e.ClientId,
                AllowSend = true,
                AllowReceive = true,
            });

            _log.LogInformation($"Client connected: {e.ClientId}");

            OnClientConnected?.Invoke(e);
            return Task.CompletedTask;
        }

        Task IMqttServerClientDisconnectedHandler.HandleClientDisconnectedAsync(MqttServerClientDisconnectedEventArgs e)
        {
            lock (_thisLock)
            {
                var client = _connectedClients.Find(x => x.ClientId == e.ClientId);
                if (client == null)
                {
                    _log.LogError($"Unkownd client disconnected: {e.ClientId}");
                    return Task.CompletedTask;
                }

                _connectedClients.Remove(client);
            }

            _log.LogInformation($"Client disconnected: {e.ClientId}");

            OnClientDisconnected?.Invoke(e);
            return Task.CompletedTask;
        }

        Task IMqttApplicationMessageReceivedHandler.HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            lock (_thisLock)
            {
                var client = _connectedClients.Find(x => x.ClientId == e.ClientId);
                var message = new MqttMessage
                {
                    Timestamp = DateTime.Now,
                    Client = client,
                    Topic = topic,
                    Payload = payload,
                    Original = e.ApplicationMessage,
                };

                _messages.Insert(0, message);

                if (_messagesByTopic.ContainsKey(topic))
                    _messagesByTopic[topic].Insert(0,  message);
                else
                    _messagesByTopic[topic] = new List<MqttMessage> { message };
            }

            _log.LogInformation($"OnMessageReceived: {topic} {payload}");

            OnMessageReceived?.Invoke(e);
            return Task.CompletedTask;
        }

        Task IMqttServerClientMessageQueueInterceptor.InterceptClientMessageQueueEnqueueAsync(MqttClientMessageQueueInterceptorContext context)
        {
            // see https://github.com/chkr1011/MQTTnet/issues/1167
            /*
            if (!string.IsNullOrEmpty(context.SenderClientId))
            {
                var sender = _connectedClients.Find(x => x.ClientId == context.SenderClientId);
                if (sender != null && !sender.AllowSend)
                {
                    context.AcceptEnqueue = false;
                    return Task.CompletedTask;
                }
            }

            if (!string.IsNullOrEmpty(context.ReceiverClientId))
            {
                var receiver = _connectedClients.Find(x => x.ClientId == context.ReceiverClientId);
                if (receiver != null && !receiver.AllowReceive)
                {
                    context.AcceptEnqueue = false;
                    return Task.CompletedTask;
                }
            }
            */
            return Task.CompletedTask;
        }


        public void Publish(MqttApplicationMessage message) => _ = Server?.PublishAsync(message);

        public void Publish(string topic, byte[] payload, bool retain)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithRetainFlag(retain)
                .Build();

            Publish(message);
        }

        public void Publish(string topic, string payload, bool retain)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithRetainFlag(retain)
                .Build();

            Publish(message);
        }
    }
}
