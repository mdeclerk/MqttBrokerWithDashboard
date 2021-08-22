using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MQTTnet.AspNetCore.Extensions;

namespace MqttBrokerWithDashboard
{
    public class Program
    {
        public static HostConfig HostConfig { get; set; }

        static IHost _host;

        static CancellationTokenSource _manualResartCts;

        public static async Task Main(string[] args)
        {
            HostConfig = HostConfig.LoadFromFile();

        RestartHost:
            _host = CreateHostBuilder(args).Build();

            _manualResartCts = new CancellationTokenSource();
            await _host.RunAsync(_manualResartCts.Token);

            if (_manualResartCts.IsCancellationRequested)
            {
                System.Console.WriteLine("Restarting host ...");

                _host.Dispose();
                _host = null;

                goto RestartHost;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(options =>
                    {
                        options.ListenAnyIP(HostConfig.TcpPort, l => l.UseMqtt());
                        options.ListenAnyIP(HostConfig.HttpPort);
                    });
                    webBuilder.UseStartup<Startup>();
                });

        public static void RestartHost() => _manualResartCts.Cancel();
    }
}
