using RabbitMQ.Client;
using System.Text;

namespace Topics.Models
{
    public class Producer : IDisposable
    {
        readonly IModel _channel;

        public string Id { get; }
        public Producer(string id, IConnection connection)
        {
            Id = id;

            _channel = connection.CreateModel();
        }

        public void SendNextMessage(string message, string routingKey)
        {
            byte[] body = Encoding.UTF8.GetBytes(message);

            var config = Program.RabbitMQConfig;
            _channel.BasicPublish(config.ExchangeName, routingKey, null, body);
        }

        public void Dispose()
        {
            _channel.Dispose();
        }
    }
}
