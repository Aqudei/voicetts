using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceTTS
{
    public class VoiceMakerRequest
    {
        public string Engine { get; set; } = "standard"; // standard, neural
        public string VoiceId { get; set; } = "ai3-en-PH-Luwalhati";
        public string LanguageCode { get; set; } = "en-PH";
        public string AccentCode { get; set; } = "en-US";
        public string Text { get; set; } = "Hello there";
        public string OutputFormat { get; set; } = "mp3";
        public string SampleRate { get; set; } = "48000";
        public string Effect { get; set; } = "default";
        public string MasterSpeed { get; set; } = "0";
        public string MasterVolume { get; set; } = "0";
        public string MasterPitch { get; set; } = "0";
    }
}
