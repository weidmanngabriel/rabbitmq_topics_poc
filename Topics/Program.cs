using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQPoC.Shared;
using System;
using Topics.Configs;
using Topics.Models;

namespace Topics
{
    internal class Program
    {
        private static RabbitMQConfig? _rabbitMQConfig;

        public static IConfiguration Config { get; } = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.local.json", optional: true) // Not in git
            .Build();

        public static RabbitMQConfig RabbitMQConfig => _rabbitMQConfig ??= Config
            .GetRequiredSection(RabbitMQConfig.Identifier)
            .Get<RabbitMQConfig>()!;

        static async Task Main(string[] args)
        {
            using var connection = RabbitMQConnectionFactory.CreateConnection(Config);

            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(RabbitMQConfig.ExchangeName, "topic", durable: true);
            }

            var api = new DocumentAPI(connection);
            var random = new Random();
            api.Consumer.SubscribeToTemp("preview.*", args => true);

            var sync = new SyncService(connection);
            sync.SubscribeToTemp("document.#", args => true);

            var ui = new UIApplikation(connection);
            ui.SubscribeToTemp("document.#", args => true);
            ui.SubscribeToTemp("preview.created", args => true);

            var backup = new BackupService(connection);
            backup.SubscribeToTemp("document.created", args => true);

            var render = new RenderService(connection);
            render.Consumer.SubscribeToTemp("document.version.created", args => HandleRender(api, random));
            render.Consumer.SubscribeToTemp("document.created", args => HandleRender(api, random));

            while (true)
            {
                Console.WriteLine("Input document event and press ENTER to send  . . .");
                string? line = Console.ReadLine();
                if (line == null)
                    continue;

                api.Producer.SendNextMessage("Key: " + line, line);
            }

            await Task.Delay(1000);
        }

        private static bool HandleRender(DocumentAPI api, Random random)
        {
            Console.WriteLine("Start rendering");
            Task.Delay(random.Next(500, 3000)).Wait();
            Console.WriteLine("Rendering done");

            if (random.NextDouble() <= 0.8) // % success rate
                api.Producer.SendNextMessage("Rendering successful", "preview.created");
            else
                api.Producer.SendNextMessage("Rendering failed", "preview.failed");

            return true;
        }

        class DocumentAPI : Both
        {
            public DocumentAPI(IConnection connection) : base(
                new Producer("apiProd", connection),
                new Consumer("apiCons", connection)
                )
            {
            }
        }

        class SyncService : Consumer
        {
            public SyncService(IConnection connection) : base("syncCons", connection)
            {

            }
        }

        class UIApplikation : Consumer
        {
            public UIApplikation(IConnection connection) : base("uiCons", connection)
            {

            }
        }

        class BackupService : Consumer
        {
            public BackupService(IConnection connection) : base("backupCons", connection)
            {

            }
        }

        class RenderService : Both
        {
            public RenderService(IConnection connection) : base(
                new Producer("renderProd", connection),
                new Consumer("renderCons", connection)
                )
            {

            }
        }
    }
}