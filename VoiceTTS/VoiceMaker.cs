using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using RestSharp;
using Newtonsoft.Json;
using RestSharp.Serializers.NewtonsoftJson;
using VoiceTTS.Api;

namespace VoiceTTS
{
    public class VoiceMaker
    {
        public const string ApiUrl = "https://developer.voicemaker.in";

        public RestClient RestClient { get; set; }

        public static async Task<IEnumerable<VoiceInfo>> GetVoicesAsync()
        {
            var token = Environment.GetEnvironmentVariable("VOICEMAKER_KEY");
            var client = new RestClient("https://developer.voicemaker.in/voice")
                .AddDefaultHeader("Authorization", $"Bearer {token}")
                .UseNewtonsoftJson();

            var request = new RestRequest("/list");
            var response = await client.ExecutePostAsync<VoiceListResponse>(request);
            return response.Data.Data.VoicesList;
        }

        public async Task<string> GenerateAudioAsync(VoiceMakerRequest requestBody)
        {
            var jsonBody = JsonConvert.SerializeObject(requestBody);

            var req = new RestRequest("/voice/api")
                .AddStringBody(jsonBody, DataFormat.Json);

            var response = await RestClient.ExecutePostAsync<VoiceMakerResponse>(req);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"{response.ErrorMessage} | {response.Content} | {response.ErrorException?.Message}");
            }

            if (response.Data == null) return response.Data?.Path;

            var audioPath = response.Data.Path.Replace("https://developer.voicemaker.in", "");
            var audioData = await RestClient.DownloadDataAsync(new RestRequest(audioPath));

            return response.Data?.Path;
        }

        public VoiceMaker()
        {
            RestClient = new RestClient(ApiUrl)
                .AddDefaultHeader("Content-Type", "application/json")
                .AddDefaultHeader("Authorization", $"Bearer {Environment.GetEnvironmentVariable("VOICEMAKER_KEY")}")
                .UseNewtonsoftJson();

        }


        public class VoiceInfo
        {
            public string Engine { get; set; }
            // public string LanguageCode { get; set; }
            public string VoiceId { get; set; }
            public string VoiceGender { get; set; }
            public string VoiceWebname { get; set; }
            public string Country { get; set; }
            public string Language { get; set; }
            public string LanguageName { get; set; }



        }
    }
}
