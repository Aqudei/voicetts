namespace VoiceTTS.Models
{
    public class VoiceInfo
    {
        public int Id { get; set; }
        public string Engine { get; set; }
        public string VoiceId { get; set; }
        public string VoiceGender { get; set; }
        public string VoiceWebname { get; set; }
        public string Country { get; set; }
        public string Language { get; set; }
        public string LanguageName { get; set; }
    }
}
