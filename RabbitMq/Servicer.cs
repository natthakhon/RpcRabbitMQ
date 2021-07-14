using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace RabbitMq
{
    public class Servicer<TPayload, TResponse>
    {
        private string sentTo;
        private IModel channel;
        private Func<TPayload, TResponse> payloadHandler;
        public event Action<TPayload,TResponse> Servicing;
        private EventingBasicConsumer sentToConsumer;

        public Servicer(IModel channel
            , string sentTo
            , Func<TPayload, TResponse> payloadHandler)
        {
            this.sentTo = sentTo;
            this.channel = channel;
            this.payloadHandler = payloadHandler;
            this.declareQueue();
            this.createConsumer();
        }

        private void declareQueue()
        {
            channel.QueueDeclare(queue: this.sentTo,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
        }

        private EventingBasicConsumer createConsumer()
        {
            this.sentToConsumer = new EventingBasicConsumer(channel);
            sentToConsumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var jsonString = Encoding.UTF8.GetString(body);
                var request = JsonSerializer.Deserialize<TPayload>(jsonString);
                var reply = this.payloadHandler(request);
                var json = JsonSerializer.Serialize(reply);

                channel.BasicPublish(exchange: "",
                                routingKey: ea.BasicProperties.ReplyTo,
                                basicProperties: channel.CreateBasicProperties(),
                                body: Encoding.UTF8.GetBytes(json));

                this.Servicing?.Invoke(request, reply);
            };

            return sentToConsumer;
        }

        public void Service()
        {
            channel.BasicConsume(queue: this.sentTo,
                                 autoAck: true,
                                 consumer: sentToConsumer);
        }
    }
}
