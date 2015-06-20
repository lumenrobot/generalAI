using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using RabbitMQ.Util;
using Newtonsoft.Json;
using System.Threading;
namespace Agent
{
    public class DataCollect
    {
        public FaceLocation faceLoc;
        public FaceName faceName;
        public UpperBodyLocation upperBodyLocation;
        public soundResult textToSpeech;
        public recognizer speechRecognized;
        public genderResult gender;
        public bool isCollecting = false;
        Connection connection;
        private string prevName = "none";
        public DataCollect(Connection connection)
        {
            this.connection = connection;
            Console.WriteLine("creating new DataCollect");
        }
        public bool startCollecting()
        {
            
            if (connection.isConnected)
            {
                if (!isCollecting)
                {
                    Console.WriteLine("start Collecting data");
                    isCollecting = true;
                    Thread threadFaceLocation = new Thread(getFaceLocation);
                    threadFaceLocation.Start();
                    while (true)
                    {
                        BasicDeliverEventArgs e = (BasicDeliverEventArgs)connection.consumerData.Queue.Dequeue();

                        //Console.WriteLine("Got message for routingkey '{0}'", (object)e.RoutingKey);
                        if (e.RoutingKey == "lumen.visual.face.recognition")
                        {
                            consumerVisual2_Received(this, e);
                        }
                        else if (e.RoutingKey == "lumen.visual.human.detection")
                        {
                            consumerVisual3_Received(this, e);
                        }
                        else if (e.RoutingKey == "lumen.audio.text.to.speech")
                        {
                            consumerAudio1_Received(this, e);
                        }
                        else if (e.RoutingKey == "lumen.audio.speech.recognition")
                        {
                            consumerAudio2_Received(this, e);
                        }
                        else if (e.RoutingKey == "lumen.audio.gender.identification")
                        {
                            consumerAudio3_Received(this, e);
                        }
                        else if (e.RoutingKey == "avatar.NAO.data.tactile")
                        {
                            consumerAvatar1_Received(this, e);
                        }
                        else if (e.RoutingKey == "avatar.NAO.data.recording")
                        {
                            consumerAvatar2_Received(this, e);
                        }
                    }                    
                    return true;
                }
                else
                {
                    //MessageBox.Show("Data Collecting is already running!", "DataCollect");
                    return false;
                }
            }
            else
            {
                //MessageBox.Show("Not Connected to Server!", "DataCollect");
                return false;
            }
        }

        private void getFaceLocation()
        {
            while (true)
            {
                BasicDeliverEventArgs e = (BasicDeliverEventArgs)connection.consumerFaceLocation.Queue.Dequeue();
                if (e.RoutingKey == "lumen.visual.face.detection")
                {
                    consumerVisual1_Received(this, e);
                }
            }
        }
        
        public bool stopCollecting()
        {
            if (isCollecting)
            {
                connection.consumerVisual1.Received -= new BasicDeliverEventHandler(consumerVisual1_Received);
                connection.consumerVisual2.Received -= new BasicDeliverEventHandler(consumerVisual2_Received);
                connection.consumerVisual3.Received -= new BasicDeliverEventHandler(consumerVisual3_Received);
                connection.consumerAudio1.Received -= new BasicDeliverEventHandler(consumerAudio1_Received);
                connection.consumerAudio2.Received -= new BasicDeliverEventHandler(consumerAudio2_Received);
                connection.consumerAudio3.Received -= new BasicDeliverEventHandler(consumerAudio3_Received);
                isCollecting = false;
                return true;
            }
            else
            {
                //MessageBox.Show("Data Collecting is already not running!", "DataCollect");
                return false;
            }
        }
        //defenisi semua event handler untuk consumer
        public delegate void FaceLocation_callback(object sender, FaceLocation faceLoc);
        public event FaceLocation_callback faceLocReceive;
        public void consumerVisual1_Received(object sender, BasicDeliverEventArgs ev)
        {
            //melakukan query terhadap face location
            string body = Encoding.UTF8.GetString(ev.Body);
            JsonSerializerSettings setting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            faceLoc = JsonConvert.DeserializeObject<FaceLocation>(body, setting);
            if (faceLocReceive != null)
            {
                faceLocReceive(this, faceLoc);
            }
        }

        public delegate void FaceName_callback(object sender, FaceName name);
        public event FaceName_callback faceNameReceive;
        public void consumerVisual2_Received(object sender, BasicDeliverEventArgs ev)
        {
            //melakukan query terhadap face recognition
            string body = Encoding.UTF8.GetString(ev.Body);
            JsonSerializerSettings setting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            faceName = JsonConvert.DeserializeObject<FaceName>(body, setting);
            
            if (true)
            {
                if (this.faceNameReceive != null)
                {
                    //Console.WriteLine("incoming name {0}", faceName.Name);
                    this.faceNameReceive(this, faceName);
                }
                prevName = faceName.Name;
            }
        }

        //belum ada event handler nya
        public void consumerVisual3_Received(object sender, BasicDeliverEventArgs ev)
        {
            //melakukan query terhadap human detection
            string body = Encoding.UTF8.GetString(ev.Body);
            JsonSerializerSettings setting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            upperBodyLocation = JsonConvert.DeserializeObject<UpperBodyLocation>(body, setting);
        }

        //belum ada event handler nya
        public void consumerAudio1_Received(object sender, BasicDeliverEventArgs ev)
        {
            //melakukan query terhadap text to speech
            string body = Encoding.UTF8.GetString(ev.Body);
            JsonSerializerSettings setting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            textToSpeech = JsonConvert.DeserializeObject<soundResult>(body, setting);
        }

        public delegate void SpeechRecognition_callback(object sender, recognizer r);
        public event SpeechRecognition_callback SpeechRecognizedReceive;
        public void consumerAudio2_Received(object sender, BasicDeliverEventArgs ev)
        {
            //melakukan query terhadap speech recognition
            string body = Encoding.UTF8.GetString(ev.Body);
            JsonSerializerSettings setting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            speechRecognized = JsonConvert.DeserializeObject<recognizer>(body, setting);
            if (this.SpeechRecognizedReceive != null)
            {
                this.SpeechRecognizedReceive(this, speechRecognized);
            }
        }

        //belum ada event handler nya
        public delegate void GenderIdentification_callback(object sender, genderResult g);
        public event GenderIdentification_callback genderReceive;
        public void consumerAudio3_Received(object sender, BasicDeliverEventArgs ev)
        {
            //melakukan query terhadap gender identification
            string body = Encoding.UTF8.GetString(ev.Body);
            JsonSerializerSettings setting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            gender = JsonConvert.DeserializeObject<genderResult>(body, setting);
            if (genderReceive != null)
            {
                genderReceive(this, gender);
            }
        }

        public delegate void TactileData_callback(object sender, TactileData t);
        public event TactileData_callback tactileDataReceive;
        public void consumerAvatar1_Received(object sender, BasicDeliverEventArgs ev)
        {
            string body = Encoding.UTF8.GetString(ev.Body);
            JsonSerializerSettings setting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            TactileData t = JsonConvert.DeserializeObject<TactileData>(body, setting);
            //Console.WriteLine("incoming...")
            //Console.WriteLine(t.Values[3]);
            if (t.Values[3] == 1.0f)
            {
                if (this.tactileDataReceive != null)
                {
                    this.tactileDataReceive(this, t);
                }
            }
        }

        public delegate void RecordingData_callback(object sender, RecordingData data);
        public event RecordingData_callback recordingDataReceive;
        public void consumerAvatar2_Received(object sender, BasicDeliverEventArgs ev)
        {
            string body = Encoding.UTF8.GetString(ev.Body);
            JsonSerializerSettings setting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            RecordingData r = JsonConvert.DeserializeObject<RecordingData>(body, setting);
            if (this.recordingDataReceive != null)
            {
                this.recordingDataReceive(this, r);
            }
        }

        public delegate void jointData_callback(object sender,string heading);
        public event jointData_callback jointDataReceive;
        public void consumerAvatar3_Received(object sender, BasicDeliverEventArgs ev)
        {
            string body = Encoding.UTF8.GetString(ev.Body);
            JsonSerializerSettings setting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            JointData joint = JsonConvert.DeserializeObject<JointData>(body, setting);
            if (Math.Abs(joint.Angles[0]) > 0.8f)
            {
                string arah = "";
                if (joint.Angles[0] > 0.8f)
                {
                    arah = "left";
                }
                else
                {
                    arah = "right";
                }
                if (jointDataReceive != null)
                {
                    jointDataReceive(this, arah);
                }
            }
            
        }
        



    }
}
