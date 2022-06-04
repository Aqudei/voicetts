using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers;
using RestSharp.Serializers.Json;
using VoiceTTS.Models;

namespace VoiceTTS
{

    public class VoiceMakerClient
    {
        public const string ApiUrl = "https://developer.voicemaker.in";

        private RestClient _client = new RestClient(ApiUrl);

        //public static IEnumerable<VoiceInfo> GetVoices()
        //{
        //    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        //    {
        //        HasHeaderRecord = false,
        //    };
        //    using var reader = new StreamReader("voices.csv");
        //    using var csv = new CsvReader(reader, config);
        //    var voiceIds = csv.GetRecords<VoiceCsv>();
        //    foreach (var voiceCsv in voiceIds)
        //    {
        //        foreach (var voiceId in voiceCsv.VoiceIds.Split(" ,\r\n\t".ToCharArray()))
        //        {
        //            if (string.IsNullOrWhiteSpace(voiceId))
        //                continue;

        //            yield return new VoiceInfo
        //            {
        //                Language = voiceCsv.LanguageCode,
        //                Engine = voiceCsv.Engine,
        //                VoiceId = voiceId
        //            };
        //        }
        //    }
        //}

        public async Task<IEnumerable<VoiceInfo>> GetVoicesFromApi()
        {
            var req = new RestRequest("/voice/list");
            var response = await _client.ExecutePostAsync<VoiceListResponse>(req);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"{response.ErrorMessage} | {response.Content} | {response.ErrorException?.Message}");
            }
            return response.Data?.Data?.voices_list;
        }

        public async Task<string> GenerateAudio(VoiceMakerRequest requestBody)
        {
            var requestBodySerialized = JsonConvert.SerializeObject(requestBody);
            var req = new RestRequest("/voice/api")
                .AddStringBody(requestBodySerialized, DataFormat.Json);

            var response = await _client.ExecutePostAsync<VoiceMakerResponse>(req);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"{response.ErrorMessage} | {response.Content} | {response.ErrorException?.Message}");
            }

            // var audioPath = response.Data.Path.Replace("https://developer.voicemaker.in", "");
            // var audioData = await RestClient.DownloadDataAsync(new RestRequest(audioPath));

            return response.Data.Path;
        }

        public VoiceMakerClient()
        {
            var token = $"{Environment.GetEnvironmentVariable("VOICEMAKER_KEY", EnvironmentVariableTarget.User)}";
            _client.Authenticator = new RestSharp.Authenticators.JwtAuthenticator(token);
            // RestClient.AddDefaultHeader("Content-Type", "application/json");
            // RestClient.AddDefaultHeader("Authorization", $"Bearer {ApiToken}");
            // RestClient.AddDefaultHeader("Authorization", $"Bearer {token}");
        }

        public class Data
        {
            public IEnumerable<VoiceInfo> voices_list { get; set; }
        }
        public class VoiceListResponse
        {
            public bool Success { get; set; }
            public Data Data { get; set; }
        }
    }
}
