using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Topics.Configs;

namespace RabbitMQPoC.Shared
{
    public static class RabbitMQConnectionFactory
    {
        public static IConnection CreateConnection(IConfiguration configWithConnection)
        {
            var config = configWithConnection.GetSection("Connection").Get<ConnectionConfig>()!;

            var factory = new ConnectionFactory()
            {
                HostName = config.HostName,
                VirtualHost = config.VirtualHost,
                UserName = config.UserName,
                Password = config.Password,
            };

            return factory.CreateConnection();
        }
    }
}
