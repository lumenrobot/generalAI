using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Diagnostics;
using System.Threading;
using System.IO;
using AIMLbot;

namespace Agent
{
    //this is program to define behavior in welcoming visitor
    public class Welcoming
    {
        Connection connection;
        DataCollect dataCollect;
        CommandHandler command;
        private bool isStanding = false;
        System.Timers.Timer t_Stand = new System.Timers.Timer(100000000);
        DateTime d = new DateTime();
        private bool hasMistakeName = false;
        private bool hasGreet = false;
        string prevName;
        string greeting;
        private NAudio.Wave.WaveIn sourceStream;
        private NAudio.Wave.WaveFileWriter streamWriter;
        private object eventLock = new object();
        ElapsedEventHandler timerHandler;
        string userGender = "";
        int state = 1;
        int hasAskName = 0;
        Bot myBot;
        User myUser;

        public void startWelcoming()
        {
            //inisialisi AIML
            myBot = new Bot();
            myBot.loadSettings();
            myUser = new User("consoleUser", myBot);
            myBot.isAcceptingUserInput = false;
            myBot.loadAIMLFromFiles();
            myBot.isAcceptingUserInput = true; 

            connection = new Connection();
            connection.connect();
            dataCollect = new DataCollect(connection);
            timerHandler = new ElapsedEventHandler(t_ElapsedStand);
            command = new CommandHandler(connection);
            t_Stand.Elapsed += timerHandler;
            dataCollect.faceLocReceive += new DataCollect.FaceLocation_callback(dataCollect_faceLocReceive);
            dataCollect.faceNameReceive += new DataCollect.FaceName_callback(dataCollect_faceNameReceive);
            dataCollect.SpeechRecognizedReceive += new DataCollect.SpeechRecognition_callback(dataCollect_SpeechRecognizedReceive);
            dataCollect.tactileDataReceive += new DataCollect.TactileData_callback(dataCollect_tactileDataReceive);
            dataCollect.recordingDataReceive += new DataCollect.RecordingData_callback(dataCollect_recordingDataReceive);
            dataCollect.genderReceive+=new DataCollect.GenderIdentification_callback(dataCollect_genderReceive);
            if (DateTime.Now.Hour < 11)
            {
                greeting = "good morning";
            }
            else if (DateTime.Now.Hour >= 11 && DateTime.Now.Hour < 18)
            {
                greeting = "good afternoon";
            }
            else if (DateTime.Now.Hour >= 18)
            {
                greeting = "good night";
            }
            Console.WriteLine("starting welcoming behavior...");

            command.startHandling();

            //command.NS_goToPosture("Stand", 0.9f);
            command.NS_tts("assalamoo alaikoom wurrohmatoollahi wabarokatuh");
            command.NS_tts(greeting);
            command.NS_tts("my name is lumen, I am robot guide");
            command.NS_tts("show me your face?");
            state = 1;
            // FIXME: hack to make it just work for demo
            //state = 8;
            //command.NS_record("what can I help you");
            //Console.WriteLine("HACK FORCE change state to 8");

            dataCollect.startCollecting();
        }

        public void stopWelcoming()
        {
            dataCollect.faceLocReceive -= new DataCollect.FaceLocation_callback(dataCollect_faceLocReceive);
            dataCollect.faceNameReceive -= new DataCollect.FaceName_callback(dataCollect_faceNameReceive);
            dataCollect.SpeechRecognizedReceive -= new DataCollect.SpeechRecognition_callback(dataCollect_SpeechRecognizedReceive);
            dataCollect.tactileDataReceive -= new DataCollect.TactileData_callback(dataCollect_tactileDataReceive);
            dataCollect.genderReceive-=new DataCollect.GenderIdentification_callback(dataCollect_genderReceive);
        }

        //defenisi event handler
        public void dataCollect_genderReceive(object sender, genderResult gender)
        {
            if (gender.gender == "male")
            {
                userGender = "sir";
            }
            else
            {
                userGender = "ma'am";
            }
        }
        public void dataCollect_SpeechRecognizedReceive(object sender, RecognizedSpeech speech)
        {
            if (state!=1)
            {
                if (speech.results.Count > 0 && speech.results[0].alternatives.Count > 0)
                {
                    this.getResponse("aaa", speech.results[0].alternatives[0].transcript.ToLower());
                } else
                {
                    this.getResponse("aaa", "");
                }
            }
        }

        public void dataCollect_faceLocReceive(object sender, FaceLocation loc)
        {
            if (state == 1)
            {
                command.NS_goToPosture("Stand", 0.9f);
                command.NS_tts("assalamu alaikum");
                command.NS_tts(greeting);
                command.NS_tts("my name is lumen, I am robot guide");
                // FIXME: hack to make it just work for demo
                //state = 2;
                //Console.WriteLine("change state to 2");
                state = 8;
                command.NS_record("what can I help you");
                Console.WriteLine("HACK FORCE change state to 8");
            }
            restartTimer();
        }

        public void dataCollect_faceNameReceive(object sender, FaceName name)
        {
            Console.WriteLine("incoming name handler {0}", (object) name.Name);
            if (state == 2)
            {
                if (name.Name == "unknown")
                {
                    command.NS_tts("may I know your name");
                    hasMistakeName = true;
                    command.NS_record("getName");
                    state = 3;
                    hasAskName += 1;
                }
                else
                {
                    command.NS_tts("how are you today " + name.Name + "?");
                    hasMistakeName = true; //temporary, delete if not used
                    command.NS_record("how are you");
                    prevName = name.Name;
                    state = 4;
                    Console.WriteLine("change state to 4");
                }
            }
        }
        public void dataCollect_tactileDataReceive(object sender, TactileData tactile)
        {

        }

        public void dataCollect_recordingDataReceive(object sender, RecordingData record)
        {
            Console.WriteLine("recording finished, waiting for recognition process...");
            byte[] check = Convert.FromBase64String(record.content);
            // write to temporary file before sending to RabbitMQ
            File.WriteAllBytes("D:/hellowav.wav", check);
            command.LA_genderIdentify(record.content, record.name);
            command.LA_speechRecognize(record.content,record.name);
        }

        //event handler untuk timer 
        public void restartTimer()
        {
            if (!t_Stand.Enabled)
            {
                //Console.WriteLine("timer started");
                t_Stand.Enabled = true;
            }
            else
            {
               // Console.WriteLine("timer restarted");
                t_Stand.Enabled = false;
                t_Stand.Enabled = true;
            }
        }

        public void t_ElapsedStand(object sender, EventArgs e)
        {
            if (state != 1)
            {
                state = 1;
                userGender = "";
                command.NS_rest();
                Console.WriteLine("Nothing's happening, I am changing state back to 1");
            }
        }

        //method untuk respon terhadap ucapan
        private int askCondition = 0;
        private int numOfQuestion = 0;
        public void getResponse(string key, string text)
        {
            #region if key of text is how are you
            if (state == 3)
            {
                string name = "";
                if(text.Contains("my"))
                   name =  text.Replace("my", "");
                if(text.Contains("name"))
                   name =  name.Replace("name", "");
                if(text.Contains("is"))
                   name =  name.Replace("is", "");

                name = name.Replace(" ", "");
                command.LV_saveName(name);
                Console.WriteLine("the name result : " + name);
                if (name == "" && hasAskName<3)
                {
                    command.NS_tts("I am sorry, would you like to repeat your name?");
                    hasMistakeName = true;
                    command.NS_record("getName");
                    state = 3;
                    hasAskName += 1;
                }
                else if (name == "" && hasAskName >= 3)
                {
                    command.NS_tts("what can I help you?");
                    command.NS_record("what can I help you");
                    state = 8;
                    Console.WriteLine("change state to 8");
                }
                else
                {
                    command.NS_tts("hello " + userGender + " " + name);
                    command.NS_tts("how are you today?");
                    command.NS_record("how are you");
                    state = 4;
                }
               
            }
            else if (state == 4)
            {
                if (text.Contains("fine") && !text.Contains("not"))
                {
                    command.NS_tts("I am happy to hear that");
                    command.NS_tts("what can I help you?");
                    command.NS_record("what can I help you");
                    state = 8;
                    Console.WriteLine("change state to 8");
                }
                else if (text.Contains("not"))
                {
                    command.NS_tts("I am sorry to hear that");
                    command.NS_tts("get well soon");
                    command.NS_tts("what can I help you?");
                    command.NS_record("what can I help you");
                    state = 8;
                    Console.WriteLine("change state to 8");
                }
                else
                {
                    if (askCondition < 3)
                    {
                        command.NS_tts("how are you?");
                        command.NS_record("how are you");
                        askCondition++;
                        state = 4;
                    }
                    else
                    {
                        askCondition = 0;
                        command.NS_tts("what can I help you " + userGender + "?");
                        command.NS_record("what can I help you");
                        state = 8;
                        Console.WriteLine("change state to 8");
                    }
                }

            }
            #endregion

            #region if key of text is what can I help you
            else if (state == 8 || state == 12)
            {
                if (text.Contains("explain") )
                {
                    if (text.Contains("your")||text.Contains("stand"))
                    {
                        command.NS_tts("my name is lumen, I am robot guide");
                        command.NS_tts("and you are now in Lumen Super Intelligent agent stand");
                        command.NS_tts("I was made to be a tour guide robot");
                        command.NS_tts("I am able to explain about my stand");
                        command.NS_tts("I can also amuse you with dancing and singing");
                        command.NS_tts("I was made by Syarif, taki, and putri");
                        command.NS_tts("That is all about me");
                        command.NS_tts("any question " + userGender);
                        command.NS_record("any question");
                        state = 12;
                        Console.WriteLine("change state to 12");
                    }
                    else if (text.Contains("exhibition") || text.Contains("event"))
                    {
                        command.NS_tts("you are now in electrical engineering days exhibition");
                        command.NS_tts("it's an exhibition to show final project of the students");
                        command.NS_tts("it is held by electrical engineering department of I T B");
                        command.NS_tts("there are 49 stands of bachelor students including this stand");
                        command.NS_tts("That is all about e e days");
                        command.NS_tts("any question " + userGender);
                        command.NS_record("any question");
                        state = 12;
                        Console.WriteLine("change state to 12");
                    }
                    else
                    {
                        command.NS_tts("you are now in electrical engineering days exhibition");
                        command.NS_tts("it's an exhibition to show final project of the students");
                        command.NS_tts("it is held by electrical engineering department of I T B");
                        command.NS_tts("there are 49 stands of bachelor students including this stand");
                        command.NS_tts("That is all about e e days");
                        command.NS_tts("any question " + userGender);
                        command.NS_record("any question");
                        state = 12;
                        Console.WriteLine("change state to 12");
                    }
                }
               
                else if (text.Contains("dance") || text.Contains("dancing"))
                {
                    command.NS_tts("of course i can dance");
                    command.NS_goToPosture("Stand", 0.9f);
                    command.NS_tts("i will dance a gangnam style");
                    command.NS_tts("watch carefully, ok");
                    t_Stand.Elapsed -= timerHandler;
                    command.NS_dance();
                    t_Stand.Elapsed += timerHandler;
                    command.NS_tts("it was great, right!");
                    //command.NS_rest();
                    command.NS_tts("anything else can I help you " + userGender + "?");
                    command.NS_record("what can I help you");
                    state = 8;
                    Console.WriteLine("change state to 8");
                }
                else if (text.Contains("sing") || text.Contains("singing"))
                {
                    command.NS_tts("of course i can sing");
                    command.NS_goToPosture("Stand", 0.9f);
                    command.NS_tts("i will sing manuk jajali song");
                    command.NS_tts("i will switch my voice to female voice");
                    t_Stand.Elapsed -= timerHandler;
                    command.NS_sing();
                    t_Stand.Elapsed += timerHandler;
                    command.NS_tts("it was great, right!");
                    //command.NS_rest();
                    command.NS_tts("anything else can I help you " + userGender + "?");
                    command.NS_record("what can I help you");
                    state = 8;
                    Console.WriteLine("change state to 8");
                }
                else if (text.Contains("photo") || text.Contains("picture"))
                {
                    command.NS_tts("of course we can");
                    command.NS_tts("let me take my pose");
                    t_Stand.Elapsed -= timerHandler;
                    command.NS_goToPosture("Stand", 0.9f);
                    command.NS_photoPose();
                    Thread.Sleep(30000);
                    command.NS_goToPosture("Stand", 0.9f);
                    //command.NS_rest();
                    t_Stand.Elapsed += timerHandler;
                    command.NS_tts("anything else can I help you " + userGender + "?");
                    command.NS_record("what can I help you");
                    state = 8;
                    Console.WriteLine("change state to 8");
                }
                //else if (text.Contains("no") || text.Contains("enough"))
                //{
                //    command.NS_tts("thank you for coming");
                //    command.NS_tts("see you again");
                //    command.NS_goodBye();
                //    command.NS_rest();
                //    state = 1;
                //    Console.WriteLine("change state to 8");
                //}

                //list pertanyaan
                else if (text.Contains("what") && text.Contains("your") && text.Contains("name"))
                {
                    state = 12;
                    command.NS_tts("my name is lumen");
                    command.NS_tts("anything else " + userGender);
                    command.NS_record("any question");

                }
                else if (text.Contains("weather"))
                {
                    state = 12;
                    command.NS_tts("I think it is nice");
                    command.NS_tts("I don't care anyway");
                    command.NS_tts("anything else " + userGender);
                    command.NS_record("any question");
                }
                else if (text.Contains("kind") || text.Contains("stand"))
                {
                    state = 12;
                    command.NS_tts("There are 49 stands");
                    command.NS_tts("In each stand presented the final product of electrical engineering students");
                    command.NS_tts("For more information, you can ask the stand directly");  
                        
                    command.NS_tts("anything else " + userGender);
                    command.NS_record("any question");
                }
                else if (text.Contains("toilet") || text.Contains("pray") || text.Contains("room") || text.Contains("door"))
                {
                    state = 12;
                    command.NS_tts("oh, I am sorry. I don't know where it is");
                    command.NS_tts("may be you can ask the crew or security");
                    command.NS_tts("anything else " + userGender);
                    command.NS_record("any question");
                }
                else if (text.Contains("made") || text.Contains("create"))
                {
                    state = 12;
                    command.NS_tts("I am NAO robot platform");
                    command.NS_tts("I am from aldebaran robotics, a french robotics company");
                    command.NS_tts("but Lumen is programmed by syarif, taki, and putri");
                    command.NS_tts("anything else " + userGender);
                    command.NS_record("any question");
                }
                else if (text.Contains("what") && text.Contains("do"))
                {
                    state = 12;
                    command.NS_tts("I can recognize human face");
                    command.NS_tts("i can understand human language and respond to them");
                    command.NS_tts("i can also amuse you with my dancing and singing");
                    command.NS_tts("I can even walk, you know?");
                    command.NS_tts("anything else " + userGender);
                    command.NS_record("any question");
                }
                else if (text.Contains("can") && text.Contains("walk"))
                {
                    state = 12;
                    command.NS_tts("well, actually I can");
                    command.NS_tts("but today is a busy day");
                    command.NS_tts("I need to be in this position for a while");
                    command.NS_tts("I am sorry");
                    command.NS_tts("anything else " + userGender);
                    command.NS_record("any question");
                }
                else if (text.Contains("can") && text.Contains("sit"))
                {
                    state = 12;
                    command.NS_tts("well, actually I can");
                    command.NS_tts("but today is a busy day");
                    command.NS_tts("I need to be in this position for a while");
                    command.NS_tts("I am sorry");
                    command.NS_tts("anything else " + userGender);
                    command.NS_record("any question");
                }
                else if (text.Contains("can") && text.Contains("run"))
                {
                    state = 12;
                    command.NS_tts("I want to, but no");
                    command.NS_tts("I can't run");
                    command.NS_tts("anything else " + userGender);
                    command.NS_record("any question");
                }
                else if (text.Contains("speak") && text.Contains("slow"))
                {
                    state = 12;
                    command.NS_tts("I am sorry");
                    command.NS_tts("I can only talk with this tempo");
                    command.NS_tts("anything else " + userGender);
                    command.NS_record("any question");
                }
                else if (text.Contains("tall") || text.Contains("high"))
                {
                    state = 12;
                    command.NS_tts("I am about 57 cm high");
                    command.NS_tts("anything else " + userGender);
                    command.NS_record("any question");
                }
                else if (text.Contains("weight") || text.Contains("fat"))
                {
                    state = 12;
                    command.NS_tts("my weight is 5 point 2 kg");
                    command.NS_tts("I am not fat, right?");
                    command.NS_tts("anything else " + userGender);
                    command.NS_record("any question");
                }
                else if (text.Contains("play"))
                {
                    state = 12;
                    command.NS_tts("well, I want to play with you");
                    command.NS_tts("but, i can't play around");
                    command.NS_tts("I need to be in this stand");
                    command.NS_tts("but I can show you my dancing and singing");
                    command.NS_tts("anything else " + userGender);
                    command.NS_record("any question");
                }
                else if (text.Contains("programmed") || text.Contains("program"))
                {
                    state = 12;
                    command.NS_tts("I was programmed by syarif, taki, and putri");
                    command.NS_tts("anything else " + userGender);
                    command.NS_record("any question");
                }
                else if (text.Contains("lumen"))
                {
                    state = 12;
                    command.NS_tts("Lumen is a humanoid robot designed to be an exhibition guide");
                    command.NS_tts("anything else " + userGender);
                    command.NS_record("any question");
                }
                else if (text.Contains("aldebaran"))
                {
                    state = 12;
                    command.NS_tts("aldebaran is a robotics company from french");
                    command.NS_tts("that is all i can tell you");
                    command.NS_tts("anything else " + userGender);
                    command.NS_record("any question");
                }
                else if (text.Contains("old"))
                {
                    command.NS_tts("I am very young");
                    command.NS_tts("anything else " + userGender);
                    command.NS_record("any question");
                    state = 12;
                }

                // basic postures
                else if (text.Contains("please") && (text.Contains("rest") || text.Contains("relax")))
                {
                    command.NS_rest();
                    command.NS_record("any question");
                    state = 12;
                }
                else if (text.Contains("please") && (text.Contains("stand") || text.Contains("get up") || text.Contains("rise")))
                {
                    command.NS_goToPosture("Stand", 0.9f);
                    command.NS_record("any question");
                    state = 12;
                }
                else if (text.Contains("please") && text.Contains("sit"))
                {
                    command.NS_goToPosture("Sit", 0.9f);
                    command.NS_record("any question");
                    state = 12;
                }
                else if (text.Contains("please") && (text.Contains("crouch") || text.Contains("get down")))
                {
                    command.NS_goToPosture("Crouch", 0.9f);
                    command.NS_record("any question");
                    state = 12;
                }

                else if (text.Contains("go away"))
                {
                    command.NS_tts("okay, see you later");
                    command.NS_rest();
                    Console.WriteLine("Going away!");
                    state = 1;
                }

                else if (text == string.Empty)
                {
                    command.NS_record("");
                    Debug.WriteLine("anything else");
                }
                else
                {
                    //command.NS_tts("would you like to repeat your request " + userGender);
                    //command.NS_record("what can I help you");
                    //state = 8;
                    //Console.WriteLine("change state to 8");

                    Request r = new Request(text, myUser, myBot);
                    Result res = myBot.Chat(r);

                    Console.WriteLine("answer: {0}", (object)res.Output);
                    command.NS_tts(res.Output);
                    command.NS_tts("anything else " + userGender);
                    command.NS_record("any question");
                    state = 8;
                }

                if(text!=null)
                    Debug.WriteLine(text);

            }
            #endregion

            
        }
    }
}
