using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Prism.Commands;
using RestSharp;
using RestSharp.Extensions;
using Newtonsoft.Json;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;
using VoiceTTS.Api;
using VoiceTTS.Model;

namespace VoiceTTS
{
    public class VoiceMaker
    {
        private readonly GlobalVariables _globalVariables;
        public const string ApiUrl = "https://developer.voicemaker.in";
        public IRestClient Client { get; set; }

        public static async Task<IEnumerable<VoiceInfo>> GetVoicesAsync()
        {
            var token = Environment.GetEnvironmentVariable("VOICEMAKER_KEY");

            var options = new RestClientOptions(ApiUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            var client = new RestClient(options, configureSerialization: config => config.UseNewtonsoftJson());

            var request = new RestRequest("/voice/list")
            {
                RequestFormat = DataFormat.Json
            };

            var response = await client.ExecutePostAsync<VoiceListResponse>(request);
            return response.Data.Data.VoicesList;
        }

        public async Task<string> GenerateAudioAsync(VoiceMakerRequest requestBody)
        {
            var jsonBody = JsonConvert.SerializeObject(requestBody);

            var req = new RestRequest("/voice/api")
                .AddStringBody(jsonBody, DataFormat.Json);

            var response = await Client.ExecutePostAsync<VoiceMakerResponse>(req);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"{response.ErrorMessage} | {response.Content} | {response.ErrorException?.Message}");
            }

            if (response.Data == null) return response.Data?.Path;

            var uri = new Uri(response.Data.Path);

            // var audioPath = response.Data.Path.Replace("https://developer.voicemaker.in", "");
            var audioData = await Client.DownloadDataAsync(new RestRequest(uri.AbsolutePath));
            var outputName = Path.Combine(_globalVariables.AudioPath, Path.GetFileName(uri.AbsolutePath));
            if (audioData == null)
                return string.Empty;
            File.WriteAllBytes(outputName, audioData);
            return outputName;

        }

        public VoiceMaker(GlobalVariables globalVariables)
        {
            _globalVariables = globalVariables;

            var token = Environment.GetEnvironmentVariable("VOICEMAKER_KEY");
            var options = new RestClientOptions(ApiUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            Client = new RestClient(options, configureSerialization: config => config.UseNewtonsoftJson());
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
