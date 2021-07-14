using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace RabbitMq
{
    public class Requestor<TPayload,TResponse>
    {
        private string sentTo;
        private string replyTo;
        private IModel channel;
        public event Action<TResponse> Received; 

        public Requestor(IModel channel
            , string sentTo
            , string replyTo)
        {
            this.sentTo = sentTo;
            this.replyTo = replyTo;
            this.channel = channel;
            this.declareQueue();
            this.createConsumers(channel);
        }

        public void Request(TPayload payload)
        {
            var json = JsonSerializer.Serialize(payload);   
            this.createPublisher(this.sentTo, this.replyTo, json);            
        }

        public void deleteQueue()
        {
            channel.QueueDelete(this.sentTo);
            channel.QueueDelete(this.replyTo);
        }

        private void declareQueue()
        {
            channel.QueueDeclare(queue: this.sentTo,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

            channel.QueueDeclare(queue: this.replyTo,
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);
        }        

        private void createConsumers(IModel channel)
        {
            var replyToConsumer = new EventingBasicConsumer(channel);

            replyToConsumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var jsonString = Encoding.UTF8.GetString(body);
                var result = JsonSerializer.Deserialize<TResponse>(jsonString);
                this.Received?.Invoke(result);
            };

            channel.BasicConsume(queue: this.replyTo,
                                 autoAck: true,
                                 consumer: replyToConsumer);
        }

        private void createPublisher(string q, string replyto, string json)
        {
            var props = channel.CreateBasicProperties();
            props.ReplyTo = replyto;
            channel.BasicPublish(exchange: "",
                                routingKey: q,
                                basicProperties: props,
                                body: Encoding.UTF8.GetBytes(json));
        }
    }

}
