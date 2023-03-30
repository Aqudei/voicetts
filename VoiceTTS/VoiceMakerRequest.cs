using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace VoiceTTS
{
    public class VoiceMakerRequest
    {
        [JsonProperty("Engine")]
        public string Engine { get; set; } = "standard"; // standard, neural

        [JsonProperty("VoiceId")]
        public string VoiceId { get; set; } = "ai3-en-PH-Luwalhati";

        [JsonProperty("LanguageCode")]
        public string LanguageCode { get; set; } = "en-PH";

        [JsonProperty("AccentCode")]
        public string AccentCode { get; set; } = "en-US";

        [JsonProperty("Text")]
        public string Text { get; set; } = "Hello there";

        [JsonProperty("OutputFormat")]
        public string OutputFormat { get; set; } = "mp3";

        [JsonProperty("SampleRate")]
        public string SampleRate { get; set; } = "48000";

        [JsonProperty("Effect")]
        public string Effect { get; set; } = "default";

        [JsonProperty("MasterSpeed")]
        public string MasterSpeed { get; set; } = "0";

        [JsonProperty("MasterVolume")]
        public string MasterVolume { get; set; } = "0";

        [JsonProperty("MasterPitch")]
        public string MasterPitch { get; set; } = "0";
    }
}
