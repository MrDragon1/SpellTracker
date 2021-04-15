using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security.Policy;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpellTracker.Data;
using System.Diagnostics;
using System.Windows.Media;
using System.Timers;
using System.Windows;
using SpellTracker.Control;
using System.Threading;
using System.Collections.Concurrent;

namespace SpellTracker.Control
{
    public class RiotParse
    {
        public int GameTime;
        public string GameMode, PlayerName;
        public List<int> level = new List<int>();
        public List<string> summonerName = new List<string>();
        public List<string> championName = new List<string>();
        public string[] summonerSpell = new string[10];
        public FadeImage[] SpellImg = new FadeImage[10];
        public int[] SpellTime = new int[10];
        public int[] SpellTotalTime = new int[10];
        public int shift = 10;
        public string playerteam = "";
        public bool IsSync = false;
        public bool FlashOnly = true;
        public SummonerSpell[] spells;
        private readonly IDictionary<string, SummonerSpell > Dic = new ConcurrentDictionary<string, SummonerSpell>();

        private string typestr = "";
        public string TypeStr { 
            get { return typestr; } 
        }

        public RiotParse()
        {
            //验证服务器证书回调自动验证
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                         new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
            Reset();
        }

        public async Task GetSpells()
        {
            this.spells = await Riot.GetSummonerSpellsAsync();
            foreach(SummonerSpell p in spells)
            {
                Dic[p.Key] = p;
            }
            
        }

        private void Reset()
        {
            for (int i = 0; i < 10; i++)
            {
                SpellTime[i] = 0;
                summonerSpell[i] = "SummonerFlash";
            }
            level.Clear();
            summonerName.Clear();
            championName.Clear();
        }

        protected bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   // 总是接受  
            return true;
        }

        public async Task Update()
        {
            if (!IsSync) return;
#if !DEBUG
            if (Process.GetProcessesByName("League of legends").Length == 0)
                        {
                            IsSync = false;
                            Reset();
                            return;
                        }
#endif
            GameTime++;
            bool IfType = false;
            if (GameTime % 10 == 0) IfType = true;
            for (int i = 0; i < 10; i++)
            {
                
                if (SpellTime[i] == 1)
                {
                    IfType = true;
                    SpellImg[i].IfFade = true;
                    SpellImg[i].Source = await ImageCache.Instance.Get(Dic[summonerSpell[i]].ImageURL);
                }
                else if (SpellTime[i] > 0)
                {
                    SpellImg[i].IfFade = false;
                    double theta = ((double)SpellTotalTime[i] - (double)SpellTime[i]) / (double)SpellTotalTime[i];
                    SpellImg[i].Source = await ImageCache.Instance.Get(Dic[summonerSpell[i]].ImageURL, (int)(theta * 360));
                }
                if (SpellTime[i] > 0) SpellTime[i]--;
            }
            if (IfType) Type();
        }

        public async Task TicTok(int id)
        {
            
            if (IsSync)
            {
                Log.Info("click " + id.ToString() + "  " + summonerSpell[id]);
                if (SpellTime[id] == 0)
                {
                    SpellTime[id] = GetSpellCD(id);
                    SpellImg[id].IfFade = true;
                    SpellImg[id].Source = await ImageCache.Instance.Get(Dic[summonerSpell[id]].ImageURL, 0);
                    //(ImageSource)Application.Current.FindResource("img/CD" + summonerSpell[id] + ".png");
                }
                else
                {
                    SpellTime[id] = 0;
                    SpellImg[id].IfFade = true;
                    SpellImg[id].Source = await ImageCache.Instance.Get(Dic[summonerSpell[id]].ImageURL);
                }
            }
            else
            {
                Log.error("Invalid click on " + id.ToString() + "  " + summonerSpell[id]);
            }
            
        }

        private string GetSpellShortName(string str)
        {
            if (str == "SummonerFlash") return ("");
            else if (str == "SummonerTeleport") return ("tp");
            else if (str == "SummonerDot") return ("dr");
            else if (str == "SummonerSmite") return ("cj");
            else if (str == "SummonerBarrier") return ("pz");
            else if (str == "SummonerBoost") return ("jh");
            else if (str == "SummonerExhaust") return ("xr");
            else if (str == "SummonerHaste") return ("jp");
            else if (str == "SummonerHeal") return ("zl");
            else if (str == "SummonerMana") return ("qxs");
            return ("");
        }

        private string Get_UrlData(string url)
        {
            try
            {
                string result = "";
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(new Uri(url));
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream stream = resp.GetResponseStream();
                try
                {
                    //获取内容
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        result = reader.ReadToEnd();
                    }
                }
                finally
                {
                    stream.Close();
                }
                return result;
            }
            catch
            {
                Log.error("wrong url : " + url );
                return "";
            }
        }

        public async Task Parse()
        {

#if DEBUG
            string Url = "./gamestats.json";
            RootObject_Gamestats gamestats = JsonConvert.DeserializeObject<RootObject_Gamestats>(System.IO.File.ReadAllText(Url));//Get_UrlData(Url)
            GameTime = (int)Convert.ToSingle(gamestats.gameTime);
            GameMode = gamestats.gameMode;

            Url = "./activeplayername.json";
            PlayerName = System.IO.File.ReadAllText(Url).Replace("\"", "");

            Url = "./playerlist.json";
            string result = "{\n\"Player\": " + System.IO.File.ReadAllText(Url) + "\n}";
            RootObject_Playerlist playerlist = JsonConvert.DeserializeObject<RootObject_Playerlist>(result);
#else
            if (Process.GetProcessesByName("League of legends").Length == 0)
            {
                IsSync = false;
                Log.error("Can not find League of legends.exe! Please make sure you have run the game.");
                return;
            }

            string Url = "https://127.0.0.1:2999/liveclientdata/gamestats";
            RootObject_Gamestats gamestats = JsonConvert.DeserializeObject<RootObject_Gamestats>(Get_UrlData(Url));//Get_UrlData(Url)
            GameTime = (int)Convert.ToSingle(gamestats.gameTime);
            GameMode = gamestats.gameMode;

            Url = "https://127.0.0.1:2999/liveclientdata/activeplayername";

            PlayerName = Get_UrlData(Url).Replace("\"", "");
            Url = "https://127.0.0.1:2999/liveclientdata/playerlist";
            string result = "{\n\"Player\": " + Get_UrlData(Url) + "\n}";
            RootObject_Playerlist playerlist = JsonConvert.DeserializeObject<RootObject_Playerlist>(result);
#endif

            foreach (Player p in playerlist.Player)
            {
                if (p.summonerName == PlayerName)
                {
                    playerteam = p.team;
                    break;
                }
            }
            level.Clear();
            summonerName.Clear();
            championName.Clear();
            int index = 0;
            foreach (Player p in playerlist.Player)
            {
                if (p.team != playerteam)
                {
                    level.Add((int)Convert.ToSingle(p.level));
                    summonerName.Add(p.summonerName);
                    summonerSpell[index++] = p.summonerSpells.summonerSpellOne.rawDisplayName.Split('_')[2];
                    summonerSpell[index++] = p.summonerSpells.summonerSpellTwo.rawDisplayName.Split('_')[2];
                    championName.Add(p.championName);
                }
            }
            for(int i = 0; i < 10; i++)
            {
                SpellTotalTime[i] = Dic[summonerSpell[i]].SummonerCD;
            }
            await LoadPic();
            IsSync = true;
            }

        private async Task LoadPic()     
        {
            for (int i = 0; i < 10; i++)
            {
                SpellImg[i].Source = await ImageCache.Instance.Get(Dic[summonerSpell[i]].ImageURL);
            }
        }

        public void Type()
        {
            if (IsSync == false) return;
            string[] pos = { "TOP", "TOP", "JUG", "JUG", "MID", "MID", "AD", "AD", "SUP", "SUP" };
            //Thread.Sleep(2000);
            string str = "";
            for(int i = 0; i < 10; i++)
            {
                if (FlashOnly)
                {
                    if (SpellTime[i] > 0 && (summonerSpell[i] == "SummonerFlash" || summonerSpell[i] == "SummonerTeleport")) str += pos[i] + GetSpellShortName(summonerSpell[i]) + GetTimeInMinute(GameTime + SpellTime[i]) + (" ");
                }
                else
                {
                    if (SpellTime[i] > 0) str += pos[i] + GetSpellShortName(summonerSpell[i]) + GetTimeInMinute(GameTime + SpellTime[i]) + (" ");
                }
            }
            typestr = str;
        }

        //返回按分秒显示的冷却时间
        private string GetTimeInMinute(int time)
        {
            time -= time % 5;
            int minute = time / 60;
            int second = time % 60;
            string t = "";
            if (minute < 10) t = "0";
            t += minute.ToString() + ":";
            if (second < 10) t += "0";
            t += second.ToString();
            return t;
        }

        private int GetSpellCD(int id)
        {
            string str = summonerSpell[id];
            if (str == "SummonerTeleport")
            {
                string Url = "https://127.0.0.1:2999/liveclientdata/playerlist";
                string result = "{\n\"Player\": " + Get_UrlData(Url) + "\n}";
                RootObject_Playerlist playerlist = JsonConvert.DeserializeObject<RootObject_Playerlist>(result);
                level.Clear();
                foreach (Player p in playerlist.Player)
                {
                    if (p.team != playerteam)
                    {
                        level.Add((int)Convert.ToSingle(p.level));
                    }
                }
                Log.Info("tp cd:" + (420 - 10 * level[id / 2] - shift).ToString());
                SpellTotalTime[id] = 420 - 10 * level[id / 2] - shift;
                return 432 - 12 * level[id / 2] - shift;
            }
            else
            {
                return SpellTotalTime[id] - shift;
            }
        }
    }
}
