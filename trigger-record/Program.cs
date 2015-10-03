using Agent;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace trigger_record
{
    class Program
    {
        private static IConnection connection;
        private static IModel channelSend;
        static void Main(string[] args)
        {
            ConnectionFactory factory = new ConnectionFactory();
            //factory.Uri = "amqp://lumen:lumen@localhost/%2F";
            factory.Uri = "amqp://localhost/%2F";
            Console.WriteLine("Connecting to RabbitMQ ...");
            connection = factory.CreateConnection();
            channelSend = connection.CreateModel(); // untuk mengirim

            //IModel channelForReply = connection.CreateModel();
            var queue = channelSend.QueueDeclare("", false, false, true, null);

            while (true)
            {
                var properties = channelSend.CreateBasicProperties();
                string corId = Guid.NewGuid().ToString();
                properties.CorrelationId = corId;
                properties.ReplyTo = queue.QueueName;

                var recordingName = "what can I help you";
                Parameter par = new Parameter { recordingName = recordingName };
                Command com = new Command { type = "audiodevice", method = "record", parameter = par };
                string doRecordStr = JsonConvert.SerializeObject(com);
                var doRecordBytes = Encoding.UTF8.GetBytes(doRecordStr);
                Console.WriteLine("Sending {0} ...", (object)doRecordStr);
                channelSend.BasicPublish("amq.topic", "avatar.nao1.command", properties,
                    doRecordBytes);

                Console.WriteLine("Waiting (Terminate App to stop)...");
                Thread.Sleep(6000);
            }
            Console.WriteLine("Done!");
        }
    }
}
