using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace VoiceTTS
{
    public class VoiceCsv
    {
        [Index(0)]
        public string Engine { get; set; }
        [Index(1)]
        public string LanguageCode { get; set; }
        [Index(2)]
        public string VoiceIds { get; set; }
    }
}
