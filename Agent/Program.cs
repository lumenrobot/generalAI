using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RabbitMQ.Client;
using RabbitMQ.Client.MessagePatterns;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
namespace Agent
{
    class Program
    {
        
        private static NAudio.Wave.WaveIn sourceStream;
        private static NAudio.Wave.WaveFileWriter streamWriter;

        static void Main(string[] args)
        {
            Welcoming w = new Welcoming();
            w.startWelcoming();
            //testAck();
        }
        static void testAck()
        {
            Console.WriteLine("setting connection");
            Connection connection = new Connection();
            connection.connect();
            CommandHandler command = new CommandHandler(connection);
            command.startHandling();
            command.NS_tts("hello");
            Console.ReadKey();

        }
        
        
    }
}
