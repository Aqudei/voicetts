using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VoiceTTS.Api
{
    public class Data
    {
        [JsonProperty("voices_list")]
        public IEnumerable<VoiceMaker.VoiceInfo> VoicesList { get; set; }

        public Data()
        {
            VoicesList = new List<VoiceMaker.VoiceInfo>();
        }
    }
    public class VoiceListResponse
    {
        public bool Success { get; set; }
        public Data Data { get; set; }
    }
}
