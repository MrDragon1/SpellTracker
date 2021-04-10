﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SpellTracker.Control;
using System.Diagnostics;

namespace SpellTracker.Data
{
    internal static class WebCache
    {
        private class CacheData
        {
            public string GameVersion;
            public string CultureName;
            public IDictionary<string, string> FileCache = new Dictionary<string, string>();
            public IDictionary<string, object> ObjectCache = new Dictionary<string, object>();

            [JsonIgnore]
            public IDictionary<string, string> SoftFileCache = new Dictionary<string, string>();
            [JsonIgnore]
            public IDictionary<string, object> SoftObjectCache = new Dictionary<string, object>();
        }

        private const string CachePath = "cache/data.json";

        private static CacheData Data = new CacheData();
        private static object WriteLock = new object();
        private static bool HasInit = false;

        private static HttpClient Client => new HttpClient();

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public static string CacheGameVersion
        {
            get => Data.GameVersion;
            set => Data.GameVersion = value;
        }

        public static string CacheLocale
        {
            get => Data.CultureName;
            set => Data.CultureName = value;
        }

        public static bool InTestMode { get; set; }

        public static void Init()
        {
            if (HasInit)
                return;

            HasInit = true;

            if (InTestMode)
                return;

            Log.debug("Initializing WebCache");

            if (!File.Exists(CachePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CachePath));
                Save();
            }
            else
            {
                string text;

                lock (WriteLock)
                    text = Encoding.UTF8.GetString(Convert.FromBase64String(File.ReadAllText(CachePath)));

                try
                {
                    Data = JsonConvert.DeserializeObject<CacheData>(text, JsonSettings);
                }
                catch (JsonSerializationException ex)
                {
                    Log.error("Failed to load web cache file", ex);
                    Data = new CacheData();
                }
            }
        }

        private static void Save()
        {
            if (InTestMode)
                return;

            Init();

            Log.debug("Saving web cache");

            string text;

            lock (Data)
                text = JsonConvert.SerializeObject(Data, JsonSettings);

            lock (WriteLock)
                File.WriteAllText(CachePath, Convert.ToBase64String(Encoding.UTF8.GetBytes(text)));
        }

        public static void Clear()
        {
            Init();
            Log.debug("Clearing web cache");

            Data = new CacheData();
            Save();
        }

        public static async Task<string> String(string url, HttpClient client = null, bool soft = false)
        {
            Init();
            Log.debug(string.Format("Cache string requested (Soft={0}): {1}", soft, url));

            var dic = soft ? Data.SoftFileCache : Data.FileCache;

            if (!dic.TryGetValue(url, out var value))
            {
                var (time, response) = await Timer.Time(() => (client ?? Client).SendAsync(new HttpRequestMessage(HttpMethod.Get, url)));

                if (!response.IsSuccessStatusCode)
                {
                    Log.error(string.Format("Failed: {1}", url, response.StatusCode));
                    return null;
                }

                Log.debug(string.Format("Gotten in {0}", time));

                value = await response.Content.ReadAsStringAsync();

                lock (Data)
                    dic[url] = value;

                Save();
            }
            else
            {
                Log.debug("Cache hit");
            }

            return value;
        }

        public static async Task<T> Json<T>(string url, HttpClient client = null, bool soft = false)
        {
            Init();
            Log.debug(string.Format("Cache json object requested (Soft={0}): {1}", soft, url));

            var dic = soft ? Data.SoftObjectCache : Data.ObjectCache;

            if (!dic.TryGetValue(url, out var value))
            {
                value = JsonConvert.DeserializeObject<T>(await String(url, client));

                lock (Data)
                    dic[url] = value;

                Save();
            }
            else
            {
                Log.debug("Cache hit");
            }

            return (T)value;
        }

        public static async Task<T> CustomJson<T>(string url, Func<JObject, T> converter, HttpClient client = null, bool soft = false)
        {
            Init();
            Log.debug(string.Format("Cache custom json object requested (Soft={0}): {1}", soft, url));

            var dic = soft ? Data.SoftObjectCache : Data.ObjectCache;

            if (!dic.TryGetValue(url, out var value))
            {
                string json = await String(url, client);

                lock (Data)
                    dic[url] = value = converter(JObject.Parse(json));

                Save();
            }
            else
            {
                Log.debug("Cache hit");
            }

            return (T)value;
        }
    }
    public static class Timer
    {
        public static async Task<(TimeSpan Time, T Result)> Time<T>(Func<Task<T>> func)
        {
            var sw = Stopwatch.StartNew();
            var result = await func();

            return (sw.Elapsed, result);
        }
    }
}
