using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Topics.Models
{
    public class Consumer : IDisposable
    {
        private readonly IModel _channel;

        public string Id { get; }

        public Consumer(string id, IConnection connection)
        {
            Id = id;

            _channel = connection.CreateModel();
            _channel.BasicQos(0, 1, false); // Only one item at a time
        }

        /// <summary>
        /// Subscribes to an existing queue with the given name.
        /// </summary>
        /// <param name="handleResult"> 
        /// A function that passes the recieved message.<br/>
        /// The returned bool marks if handling was successful. 
        /// </param>
        public void SubscribeToExisting(string queue, Func<BasicDeliverEventArgs, bool> handleResult)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (sender, ea) =>
            {
                Console.WriteLine($"{Id}: Recieved message");

                if (handleResult(ea))
                    consumer.Model.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(consumer, queue);
        }

        /// <summary>
        /// Creates its own exclusive queue with the given consumerTag.
        /// </summary>
        /// <remarks>
        /// The queue will be filled if the consumer tag gets matched by incoming data.
        /// </remarks>
        /// <param name="handleResult"> 
        /// A function that passes the recieved message.<br/>
        /// The returned bool marks if handling was successful. 
        /// </param>
        public void SubscribeToTemp(string consumerTag, Func<BasicDeliverEventArgs, bool> handleResult)
        {
            var consumer = new EventingBasicConsumer(_channel);

            var queueDeclare = _channel.QueueDeclare(
                queue: string.Empty,
                durable: false,
                exclusive: true,
                autoDelete: false,
                arguments: null
            );

            _channel.QueueBind(queueDeclare.QueueName, Program.RabbitMQConfig.ExchangeName, consumerTag);

            consumer.Received += (sender, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"{Id}: Recieved message: '{message}'");

                if (handleResult(ea))
                    consumer.Model.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(consumer, queueDeclare.QueueName);
        }

        public void Dispose()
        {
            _channel.Dispose();
        }
    }
}
