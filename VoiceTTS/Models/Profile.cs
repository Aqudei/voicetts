using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace VoiceTTS.Models
{
    public class Profile : BindableBase
    {
        private string _profileName;
        public int Id { get; set; }
        public string Engine { get; set; }
        //public string LanguageCode { get; set; }
        public string VoiceId { get; set; }
        public string Effect { get; set; }
        public int MasterSpeed { get; set; }
        public int MasterVolume { get; set; }
        public int MasterPitch { get; set; }

        public string ProfileName
        {
            get => _profileName;
            set => SetProperty(ref _profileName, value);
        }

        public string AccentCode { get; set; }

        public string DefaultEffect { get; set; } = "breathing";
        public string DefaultPause { get; set; } = "5s";
        public string DefaultEmphasis { get; set; } = "strong";
        public string DefaultSpeed { get; set; } = "slow";
        public string DefaultPitch { get; set; } = "high";
        public string DefaultVolume { get; set; } = "loud";
        public string DefaultSayAs { get; set; } = "spell-out";

        // public string OutputDevice { get; set; }

        public string KeyManualSend { get; set; }
        public string KeyVolumeUp { get; set; }
        public string KeyVolumeDown { get; set; }
        public string KeyPitchDown { get; set; }
        public string KeyPitchUp { get; set; }
        public string KeySpeedUp { get; set; }
        public string KeySpeedDown { get; set; }

        public string KeyAppendVoice { get; set; }

        //Voice Effects
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
        public string KeyAppendDefault { get; set; }
        public string KeyAppendSad { get; set; }
        public string KeyAppendAngry { get; set; }
        public string KeyAppendExcited { get; set; }
        public string KeyAppendFriendly { get; set; }
        public string KeyAppendHopeful { get; set; }
        public string KeyAppendShouting { get; set; }
        public string KeyAppendTerrified { get; set; }
        public string KeyAppendUnFriendly { get; set; }
        //End Voice Effects

        public string KeyAppendPause { get; set; }
        public string KeyAppendEmphasis { get; set; }
        public string KeyAppendSpeed { get; set; }
        public string KeyAppendPitch { get; set; }
        public string KeyAppendVolume { get; set; }
        public string KeyAppendSayAs { get; set; }
        public string KeyAppRestart { get; set; }
    }
}
