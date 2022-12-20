namespace Topics.Models
{
    public abstract class Both : IDisposable
    {
        public Producer Producer { get; }
        public Consumer Consumer { get; }

        public Both(Producer producer, Consumer consumer)
        {
            Producer = producer;
            Consumer = consumer;
        }

        public void Dispose()
        {
            Producer.Dispose();
            Consumer.Dispose();
        }
    }
}
