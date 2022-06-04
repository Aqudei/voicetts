using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceTTS.Models
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

        #region "HotKey voice effects"
        public string KeyAppendBreathing { get; set; }
        public string KeyAppendSoft { get; set; }
        public string KeyAppendWhispered { get; set; }
        public string KeyAppendConversational { get; set; }
        public string KeyAppendNews { get; set; }
        public string KeyAppendCustomerSupport { get; set; }
        public string KeyAppendAssistant { get; set; }
        public string KeyAppendHappy { get; set; }
        public string KeyAppendEmphatic { get; set; }
        public string KeyAppendClam { get; set; }
        public string KeyAppendSad { get; set; }
        public string KeyAppendAngry { get; set; }
        public string KeyAppendExcited { get; set; }
        public string KeyAppendFriendly { get; set; }
        public string KeyAppendHopeful { get; set; }
        public string KeyAppendShouting { get; set; }
        public string KeyAppendTerrified { get; set; }
        public string KeyAppendUnFriendly { get; set; }
        #endregion


        public string KeyAppendPause { get; set; }
        public string KeyAppendEmphasis { get; set; }
        public string KeyAppendSpeed { get; set; }
        public string KeyAppendPitch { get; set; }
        public string KeyAppendVolume { get; set; }
        public string KeyAppendSayAs { get; set; }
    }
}
