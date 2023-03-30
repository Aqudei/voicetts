using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceTTS.Model
{
    public class HotkeySet
    {
        public string KeyManualSend { get; set; }
        public string KeyVolumeUp { get; set; }
        public string KeyVolumeDown { get; set; }
        public string KeyPitchDown { get; set; }
        public string KeyPitchUp { get; set; }
        public string KeySpeedUp { get; set; }
        public string KeySpeedDown { get; set; }

        public string KeyAppendVoice { get; set; }
        public string KeyAppendPause { get; set; }
        public string KeyAppendEmphasis { get; set; }
        public string KeyAppendSpeed { get; set; }
        public string KeyAppendPitch { get; set; }
        public string KeyAppendVolume { get; set; }
        public string KeyAppendSayAs { get; set; }
    }
}
