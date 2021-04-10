using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SpellTracker.Data;

namespace SpellTracker.Control
{
    class Riot
    {
        public const string CdnEndpoint = "https://ddragon.leagueoflegends.com/cdn/";

        public static string ImageEndpoint => CdnEndpoint + "img/";

        private static readonly IDictionary<string, string> CDragonLocale = new Dictionary<string, string>
        {
            ["en_US"] = "en_gb",
            ["es_ES"] = "es_es"
        };

        public static string Locale { get; set; } = "en_US";

        private static string LatestVersion;
        public static async Task<string> GetLatestVersionAsync()
            => LatestVersion ?? (LatestVersion = JsonConvert.DeserializeObject<string[]>(await Client.DownloadStringTaskAsync("https://ddragon.leagueoflegends.com/api/versions.json"))[0]);

        private static WebClient Client => new WebClient { Encoding = Encoding.UTF8 };

        public static async Task<SummonerSpell[]> GetSummonerSpellsAsync()
        {
            string url = $"{CdnEndpoint}{await GetLatestVersionAsync()}/data/{Locale}/summoner.json";

            return await WebCache.CustomJson(url, jobj =>
            {
                return jobj["data"].Children().Select(o =>
                {
                    var p = o as JProperty;
                    return new SummonerSpell
                    {
                        ID = p.Value["key"].ToObject<int>(),
                        Key = p.Value["id"].ToObject<string>(),
                        Name = p.Value["name"].ToObject<string>(),
                        SummonerLevel = p.Value["summonerLevel"].ToObject<int>(),
                        ImageURL = $"{CdnEndpoint}{LatestVersion}/img/spell/" + p.Value["image"]["full"].ToObject<string>(),
                        SummonerCD = p.Value["cooldownBurn"].ToObject<int>()
                    };
                })
                .OrderBy(o => o.SummonerLevel)
                .ThenBy(o => o.Name)
                .ToArray();
            });
        }
    }

}
