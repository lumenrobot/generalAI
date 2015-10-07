using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace Agent
{
    class CommandJson
    {

    }

    public class Command
    {
        [JsonProperty("type")]
        public string type { get; set; }
        [JsonProperty("Method")]
        public string method { get; set; }
        [JsonProperty("parameter")]
        public Parameter parameter { get; set; }
    }

    public class PostureChange
    {
        [JsonProperty("@type")]
        public string type { get; set; }
        public string postureId { get; set; }
        public double speed { get; set; }

        public PostureChange()
        {
            type = "PostureChange";
        }
    }

    public class RecordAudio
    {
        [JsonProperty("@type")]
        public string type { get; }
        [JsonProperty]
        public double duration { get; set; }

        public RecordAudio() {
            type = "RecordAudio";
        }
    }

    public enum ActingScript
    {
        GOOD_BYE,
        PHOTO_POSE,
        DANCE_GANGNAM,
        SING_MANUK,
        SING_UPTOWN
    };

    public class ActingPerformance
    {
        [JsonProperty("@type")]
        public string type { get; }
        [JsonProperty]
        public ActingScript script { get; set; }
        [JsonProperty]
        public double restAfterPerformance { get; set; }

        public ActingPerformance()
        {
            type = "ActingPerformance";
        }
    }

    public class Rest
    {
        [JsonProperty("@type")]
        public string type { get; }

        public Rest()
        {
            type = "Rest";
        }
    }

    public class WakeUp
    {
        [JsonProperty("@type")]
        public string type { get; }

        public WakeUp()
        {
            type = "WakeUp";
        }
    }

    public class CommunicateAction
    {
        [JsonProperty("@type")]
        public string type { get; }
        [JsonProperty]
        public string inLanguage { get; set; }
        [JsonProperty("object")]
        public string theObject { get; set;  }
        [JsonProperty]
        public string avatarId { get; set; }

        public CommunicateAction()
        {
            this.type = "CommunicateAction";
        }
    }

    public class Parameter
    {
        //RobotPosture parameter
        [JsonProperty("postureName")]
        public string postureName { get; set; }
        [JsonProperty("speed")]//belong to motion and posture
        public float speed { get; set; }

        //motion parameter
        [JsonProperty("jointNames")]
        public List<string> jointName { get; set; }
        [JsonProperty("stiffnessses")]
        public List<float> stiffnessess { get; set; }
        [JsonProperty("angles")]
        public List<float> angles { get; set; }
        [JsonProperty("handName")]
        public string handName { get; set; }
        [JsonProperty("x")]
        public float x { get; set; }
        [JsonProperty("y")]
        public float y { get; set; }
        [JsonProperty("tetha")]
        public float tetha { get; set; }
        [JsonProperty("LHand")]
        public bool LHand { get; set; }
        [JsonProperty("RHand")]
        public bool RHand { get; set; }

        //TextToSpeech parameter
        [JsonProperty("text")]
        public string text { get; set; }
        [JsonProperty("language")]
        public string language { get; set; }

        //AudioDeviceParameter
        [JsonProperty("wavFile")]
        public string wavFile { get; set; }
        [JsonProperty("recordingName")]
        public string recordingName { get; set; }
    }


}
