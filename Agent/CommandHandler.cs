using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;

namespace Agent
{
    public class CommandHandler
    {
        IModel channel;
        public bool isHandling;
        NAudio.Wave.WaveIn sourceStream;
        NAudio.Wave.WaveFileWriter streamWriter;
        public bool isRecording = false;
        Connection connection;
        QueueingBasicConsumer replyConsumer;
        QueueDeclareOk replyQueue;

        public CommandHandler(Connection connection)
        {
            this.connection = connection;
            isHandling = false;
        }

        public bool startHandling()
        {
            if (connection.isConnected)
            {
                if (!isHandling)
                {
                    channel = connection.channelSend;
                    Debug.WriteLine("Create model...");
                    IModel channelForReply = connection.connection.CreateModel();
                    Debug.WriteLine("QueueDeclare...");
                    replyQueue = channelForReply.QueueDeclare("", false, false, true, null);
                    Debug.WriteLine("QueueingBasicConsumer...");
                    replyConsumer = new QueueingBasicConsumer(channelForReply);
                    Debug.WriteLine("BasicConsume...");
                    channelForReply.BasicConsume(replyQueue.QueueName, true, replyConsumer);
                    isHandling = true;

                    // rest first on app start!
                    NS_rest();

                    Debug.WriteLine("startHandling finished");
                    return true;
                }
                else
                {
                    return false;

                }
            }
            else
            {
                Console.WriteLine("NOT CONNECTED!");
                return false;
            }
        }

        public bool stopHandling()
        {
            if (isHandling)
            {
                isHandling = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void sendCommand(string command, string routingKey)
        {
            //Console.WriteLine("executing command");

            var properties = channel.CreateBasicProperties();
            string corId = Guid.NewGuid().ToString();
            properties.CorrelationId = corId;
            properties.ReplyTo = replyQueue.QueueName;
            //Console.WriteLine("sending command corrId={0} replyTo={1}", properties.CorrelationId, properties.ReplyTo);
            byte[] buffer = Encoding.UTF8.GetBytes(command);
            if (routingKey == "avatar.nao1.command" || routingKey == "lumen.speech.expression")
            {
                channel.BasicPublish("amq.topic", routingKey, properties, buffer);
                connection.corrId = corId;
                Console.WriteLine("Waiting for {0}'s reply for {1} {2}", routingKey, corId, replyQueue.QueueName);
                replyConsumer.Queue.Dequeue();
                Console.WriteLine("{0}'s reply received for {1} {2}", routingKey, corId, replyQueue.QueueName);
            }
            else
            {
                channel.BasicPublish("amq.topic", routingKey, null, buffer);
            }
            //Console.WriteLine("command sent");
        }

        //define command that will be send to NAO server with NS_ code
        public void NS_wakeUp()
        {
            if (isHandling)
            {
                var com = new WakeUp();
                string body = JsonConvert.SerializeObject(com);
                this.sendCommand(body, "avatar.nao1.command");
            }
            else
            {
                //MessageBox.Show("Command Handler is not started yet", "CommandHandler");
            }
        }
        public void NS_rest()
        {
            if (isHandling)
            {
                Console.WriteLine("NS_rest...");
                Rest rest = new Rest();
                string body1 = JsonConvert.SerializeObject(rest);
                this.sendCommand(body1, "avatar.nao1.command");
                Console.WriteLine("NS_rest sent.");
                //Parameter par = new Parameter { jointName = new List<string> { "HeadYaw", "HeadPitch" }, angles = new List<float> { 0.8f, 0.8f } };
                //Command com2 = new Command { type = "motion", method = "setAngles", parameter = par };
                //string body2 = JsonConvert.SerializeObject(com2);
                //this.sendCommand(body2, "avatar.nao1.command");
            }
            else
            {
                //MessageBox.Show("Command Handler is not started yet", "CommandHandler");
            }
        }
        public void NS_setAngles(List<string> jointName, List<float> angles, float speed)
        {
            if (isHandling)
            {
                Parameter par = new Parameter { jointName = jointName, angles = angles, speed = speed };
                Command com = new Command { type = "motion", method = "setAngles", parameter = par };
                string body = JsonConvert.SerializeObject(com);
                this.sendCommand(body, "avatar.nao1.command");
            }
            else
            {
                //MessageBox.Show("Command Handler is not started yet", "CommandHandler");
            }
        }
        public void NS_changeAngles(List<string> jointName, List<float> angles, float speed)
        {
            if (isHandling)
            {
                Parameter par = new Parameter { jointName = jointName, angles = angles, speed = speed };
                Command com = new Command { type = "motion", method = "changeAngles", parameter = par };
                string body = JsonConvert.SerializeObject(com);
                this.sendCommand(body, "avatar.nao1.command");
            }
            else
            {
                //MessageBox.Show("Command Handler is not started yet", "CommandHandler");
            }
        }
        public void NS_setStiffness(List<string> jointName, List<float> stiffnesses)
        {
            if (isHandling)
            {
                Parameter par = new Parameter { jointName = jointName, stiffnessess = stiffnesses };
                Command com = new Command { type = "motion", method = "setStiffnesses", parameter = par };
                string body = JsonConvert.SerializeObject(com);
                this.sendCommand(body, "avatar.nao1.command");
            }
            else
            {
                //MessageBox.Show("Command Handler is not started yet", "CommandHandler");
            }
        }
        public void NS_moveInit()
        {
            if (isHandling)
            {
                Command com = new Command { type = "motion", method = "moveInit" };
                string body = JsonConvert.SerializeObject(com);
                this.sendCommand(body, "avatar.nao1.command");
            }
            else
            {
                // MessageBox.Show("Command Handler is not started yet", "CommandHandler");
            }
        }
        public void NS_walkTo(float x, float y, float tetha)
        {
            if (isHandling)
            {
                Parameter par = new Parameter { x = x, y = y, tetha = tetha };
                Command com = new Command { type = "motion", method = "moveTo", parameter = par };
                string body = JsonConvert.SerializeObject(com);
                this.sendCommand(body, "avatar.nao1.command");
            }
            else
            {
                //MessageBox.Show("Command Handler is not started yet", "CommandHandler");
            }
        }
        public void NS_tts(string toSay)
        {
            if (isHandling)
            {
                Console.WriteLine("TTS: {0}", toSay);
                var com = new Speech { markup = toSay, avatarId = "nao1" };
                string body = JsonConvert.SerializeObject(com);
                this.sendCommand(body, "lumen.speech.expression");
            }
            else
            {
                Console.WriteLine("Command Handler is not started for TTS: {0}", toSay);
                //MessageBox.Show("Command Handler is not started yet", "CommandHandler");
            }
        }
        public void NS_goToPosture(string postureId, float speed)
        {
            if (isHandling)
            {
                var com = new PostureChange { postureId = postureId, speed = speed };
                string body = JsonConvert.SerializeObject(com);
                this.sendCommand(body, "avatar.nao1.command");
            }
            else
            {
                //MessageBox.Show("Command Handler is not started yet", "CommandHandler");
            }
        }
        public void NS_sendRemoteBufferToOutput(string filePath)
        {
            if (isHandling)
            {
                byte[] buffer = File.ReadAllBytes(filePath);
                string wav = Convert.ToBase64String(buffer);
                Parameter par = new Parameter { wavFile = wav };
                Command com = new Command { type = "audiodevice", method = "sendremotebuffertooutput", parameter = par };
                string body = JsonConvert.SerializeObject(com);
                this.sendCommand(body, "avatar.nao1.command");
            }
            else
            {

            }
        }
        public void NS_record(string recordingName)
        {
            if (isHandling)
            {

                NS_tts(recordingName);

                // USE TEXT INPUT
                //Console.Write("What do you want to say? TYPE HERE: ");
                //string inp = Console.ReadLine();
                //recognizer hasil = new recognizer { name = "aaa", result = inp, date = DateTime.Now.ToString() };
                //string body = JsonConvert.SerializeObject(hasil); //serialisasi data menjadi string
                //this.sendCommand(body, "lumen.speech.recognition");

                // USE SPEECH RECOGNITION
                Console.WriteLine("please say something");
                Parameter par = new Parameter { recordingName = recordingName };
                var recordAudio = new RecordAudio { duration = 5.0 };
                string body = JsonConvert.SerializeObject(recordAudio);
                this.sendCommand(body, "avatar.nao1.audio");
            }
            else
            {
                Console.WriteLine("Command Handler is not started yet");
                //MessageBox.Show("Command Handler is not started yet", "CommandHandler");
            }
        }
        public void NS_photoPose()
        {
            if (isHandling)
            {
                var actingPerformance = new ActingPerformance();
                actingPerformance.script = ActingScript.PHOTO_POSE;
                string body = JsonConvert.SerializeObject(actingPerformance);
                this.sendCommand(body, "avatar.nao1.acting");
            }
            else
            {
                //MessageBox.Show("Command Handler is not started yet", "CommandHandler");
            }
        }
        public void NS_goodBye()
        {
            if (isHandling)
            {
                var actingPerformance = new ActingPerformance();
                actingPerformance.script = ActingScript.GOOD_BYE;
                string body = JsonConvert.SerializeObject(actingPerformance);
                this.sendCommand(body, "avatar.nao1.acting");
            }
            else
            {
                //MessageBox.Show("Command Handler is not started yet", "CommandHandler");
            }
        }
        public void NS_dance()
        {
            if (isHandling)
            {
                var actingPerformance = new ActingPerformance();
                actingPerformance.script = ActingScript.DANCE_GANGNAM;
                string body = JsonConvert.SerializeObject(actingPerformance);
                this.sendCommand(body, "avatar.nao1.acting");
            }
            else
            {
                //MessageBox.Show("Command Handler is not started yet", "CommandHandler");
            }
        }
        public void NS_sing()
        {
            if (isHandling)
            {
                var actingPerformance = new ActingPerformance();
                actingPerformance.script = ActingScript.SING_MANUK;
                string body = JsonConvert.SerializeObject(actingPerformance);
                this.sendCommand(body, "avatar.nao1.acting");
            }
            else
            {
                //MessageBox.Show("Command Handler is not started yet", "CommandHandler");
            }
        }


        //define command that will be send to Lumen Audio with LA_ code
        public void LA_speechRecognize(string buffer, string speechName)
        {
            if (isHandling)
            {
                // TODO: still needed?
                string wavString = buffer;
                sound wav = new sound { name = speechName, content = wavString, language = "en-us" };
                string body = JsonConvert.SerializeObject(wav);
                this.sendCommand(body, "lumen.audio.wav.stream");
            }
            else
            {
                //MessageBox.Show("Command Handler is not started yet", "CommandHandler");
            }
        }
        public void LA_textToSpeech(string toSay)
        {
            if (isHandling)
            {
                textData text = new textData { name = "textToSpeech", text = toSay, date = System.DateTime.Now.ToString() };
                string body = JsonConvert.SerializeObject(text);
                this.sendCommand(body, "lumen.audio.text.stream");
            }
            else
            {
                //MessageBox.Show("Command Handler is not started yet", "CommandHandler");
            }
        }
        public void LA_genderIdentify(string buffer, string speechName)
        {
            if (isHandling)
            {
                sound wav = new sound { name = "speech", content = buffer };
                string body = JsonConvert.SerializeObject(wav);
                this.sendCommand(body, "lumen.audio.get.wave");
            }
            else
            {
                //MessageBox.Show("Command Handler is not started yet", "CommandHandler");
            }
        }

        public void LV_saveName(string name)
        {
            if (isHandling)
            {
                this.sendCommand(name, "lumen.visual.command");
            }
            else
            {
            }
        }





    }
}
