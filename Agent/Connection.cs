using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using RabbitMQ.Util;

namespace Agent
{
    public class Connection
    {
        public bool isConnected = false;
        public IModel channelSend,channelData;
        private IModel channelVisual1, channelVisual2, channelVisual3, channelAudio1, channelAudio2, channelAudio3, channelAvatar1,channelAvatar3;
        public EventingBasicConsumer consumerVisual1, consumerVisual2, consumerVisual3, consumerAudio1, consumerAudio2, consumerAudio3, consumerAvatar1, consumerAvatar2;
        public IConnection connection;
        public QueueingBasicConsumer consumerData,consumerFaceLocation,consumerDataJoint; // buat ack command
        public bool isAck = false;
        public string ackRoutingKey;
        public string corrId;
        public Connection()
        {
            Console.WriteLine("creating new Connection");
            isConnected = false;
        }
        public void connect()
        {
            if (!isConnected)
            {
                try
                {
                    string routingKey;
                    ConnectionFactory factory = new ConnectionFactory();
                    //factory.Uri = "amqp://lumen:lumen@localhost/%2F";
                    factory.Uri = "amqp://localhost/%2F";
                    connection = factory.CreateConnection();
                    channelSend = connection.CreateModel(); // untuk mengirim
                    channelData = connection.CreateModel();
                    channelAvatar3 = connection.CreateModel();

                    QueueDeclareOk queueData = channelData.QueueDeclare("", true, false, true, null);
                    QueueDeclareOk queueFaceLocation = channelData.QueueDeclare("", true, false, true, null);

                    channelData.QueueBind(queueFaceLocation.QueueName, "amq.topic", "lumen.visual.face.detection");

                    channelData.QueueBind(queueData.QueueName, "amq.topic", "lumen.visual.face.recognition");
                    channelData.QueueBind(queueData.QueueName, "amq.topic", "lumen.visual.human.detection");
                    channelData.QueueBind(queueData.QueueName, "amq.topic", "lumen.audio.text.to.speech");
                    channelData.QueueBind(queueData.QueueName, "amq.topic", "lumen.audio.speech.recognition");
                    channelData.QueueBind(queueData.QueueName, "amq.topic", "lumen.audio.gender.identification");
                    channelData.QueueBind(queueData.QueueName, "amq.topic", "avatar.nao1.data.tactile");
                    channelData.QueueBind(queueData.QueueName, "amq.topic", "avatar.nao1.data.recording");

                    consumerData = new QueueingBasicConsumer(channelData);
                    consumerFaceLocation = new QueueingBasicConsumer(channelData);
                    consumerDataJoint = new QueueingBasicConsumer(channelAvatar3);

                    channelData.BasicConsume(queueData.QueueName, true, consumerData);
                    channelData.BasicConsume(queueFaceLocation.QueueName, true, consumerFaceLocation);
                   
                    isConnected = true;
                    Console.WriteLine("program is connected to server");
                    //Program.panel.btn_connect.Text = "Disconnect";
                }
                catch
                {
                    //MessageBox.Show("unable to connect to server", "connection");
                }
            }
            else
            {
                //MessageBox.Show("already connected to server!", "connection");
            }
        }

        public void disconnect()
        {
            if (isConnected)
            {
                if (!this.isProcessRunning())
                {
                    isConnected = false;
                    //Program.panel.btn_connect.Text = "Connect";
                    this.connection.Close();
                }
                else
                {
                    //MessageBox.Show("Stop Process Before Disconnecting", "Connection");
                }
            }
        }

        public void consumerAck_Received(object sender, BasicDeliverEventArgs ev)
        {
            Console.WriteLine("received reply : corrId={0}", ev.BasicProperties.CorrelationId);
            if (ev.BasicProperties.CorrelationId == this.corrId)
            {
                this.isAck = true;
            }
        }
        public bool isProcessRunning()
        {
            return true;
            //if ((Program.panel.dataCollect.isCollecting) || (Program.panel.command.isHandling))
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
        }
    }

}
