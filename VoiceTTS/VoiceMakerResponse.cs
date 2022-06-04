using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceTTS
{
    public class VoiceMakerResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Path { get; set; }
    }
}
