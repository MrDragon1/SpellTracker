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

        public class Summoner
        {
            public int level;
            public List<int> items;
            public int reduction;
            public string summonerName;
            public string championName;
            public FadeImage[] SpellImg = new FadeImage[2];
            public string[] summonerSpell = new string[2];
            public int[] SpellTime = new int[2];
            public int[] SpellTotalTime = new int[2];

            public Summoner()
            {
                this.SpellImg = new FadeImage[2];
                this.summonerSpell = new string[2];
                this.SpellTime = new int[2];
                this.SpellTotalTime = new int[2];
                this.level = 0;
                this.items = new List<int>();
                this.reduction = 0;
                this.championName = "";
                this.summonerName = "";
            }
        }
        public Summoner[] summoner = new Summoner[5];
        //public List<int> level = new List<int>();
        //public List<int> items = new List<int>();
        //public List<int> reduction = new List<int>();
        //public List<string> summonerName = new List<string>();
        //public List<string> championName = new List<string>();
        //public string[] summonerSpell = new string[10];
        //public FadeImage[] SpellImg = new FadeImage[10];
        //public int[] SpellTime = new int[10];
        //public int[] SpellTotalTime = new int[10];
        public int shift = 10;
        public string playerteam = "";
        public bool IsSync = false;
        public bool FlashOnly = true;
        public int FlashPos = 0;
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
            //for win7 
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            summoner = new Summoner[5];
            for(int i = 0; i < 5; i++)
            {
                summoner[i] = new Summoner();
            }
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

            for(int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    summoner[i].SpellTime[j] = 0;
                    summoner[i].summonerSpell[j] = "SummonerFlash";
                }
                summoner[i].level = 0;
                summoner[i].summonerName = "";
                summoner[i].championName = "";
            }
            
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
            for (int i = 0; i < 5; i++)
            {
                for(int j = 0; j < 2; j++)
                {
                    if (summoner[i].SpellTime[j] == 1)
                    {
                        IfType = true;
                        summoner[i].SpellImg[j].IfFade = true;
                        summoner[i].SpellImg[j].Source = await ImageCache.Instance.Get(Dic[summoner[i].summonerSpell[j]].ImageURL);
                    }
                    else if (summoner[i].SpellTime[j] > 0)
                    {
                        summoner[i].SpellImg[j].IfFade = false;
                        double theta = ((double)summoner[i].SpellTotalTime[j] - (double)summoner[i].SpellTime[j]) / (double)summoner[i].SpellTotalTime[j];
                        summoner[i].SpellImg[j].Source = await ImageCache.Instance.Get(Dic[summoner[i].summonerSpell[j]].ImageURL, (int)(theta * 360));
                    }
                    if (summoner[i].SpellTime[j] > 0) summoner[i].SpellTime[j]--;
                }
            }
            if (IfType) Type();
        }

        public async Task TicTok(int i,int j)
        {
            if (IsSync)
            {
    
                Log.Info("click " + i.ToString() + "," + j.ToString() + "  " + summoner[i].summonerSpell[j]);
                if (summoner[i].SpellTime[j] == 0)
                {
                    summoner[i].SpellTime[j] = GetSpellCD(i,j);
                    summoner[i].SpellImg[j].IfFade = true;
                    summoner[i].SpellImg[j].Source = await ImageCache.Instance.Get(Dic[summoner[i].summonerSpell[j]].ImageURL, 0);
                    //(ImageSource)Application.Current.FindResource("img/CD" + summonerSpell[id] + ".png");
                }
                else
                {
                    summoner[i].SpellTime[j] = 0;
                    summoner[i].SpellImg[j].IfFade = true;
                    summoner[i].SpellImg[j].Source = await ImageCache.Instance.Get(Dic[summoner[i].summonerSpell[j]].ImageURL);
                }
            }
            else
            {
                Log.error("Invalid click on " + i.ToString() + "," + j.ToString() + "  " + summoner[i].summonerSpell[j]);
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
            Log.Info("In" + url);
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
            catch (Exception ex)
            {
                Log.error("wrong url : " + url + ex);
                return "";
            }
        }

        public async Task Parse()
        {
            try
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
                int index = 0;
                foreach (Player p in playerlist.Player)
                {
                    if (p.team != playerteam)
                    {
                        summoner[index].level = (int)Convert.ToSingle(p.level);
                        summoner[index].reduction = 0;
                        summoner[index].summonerName = p.summonerName;
                        summoner[index].summonerSpell[0] = p.summonerSpells.summonerSpellOne.rawDisplayName.Split('_')[2];
                        summoner[index].summonerSpell[1] = p.summonerSpells.summonerSpellTwo.rawDisplayName.Split('_')[2];
                        if (FlashPos != 0)
                        {
                            if (summoner[index].summonerSpell[2-FlashPos] == "SummonerFlash")
                            {
                                var tmp = summoner[index].summonerSpell[0];
                                summoner[index].summonerSpell[0] = summoner[index].summonerSpell[1];
                                summoner[index].summonerSpell[1] = tmp;
                            }
                        }
                        summoner[index].championName = p.championName;
                        index++;
                    }
                }
                for (int i = 0; i < 5; i++)
                {
                    for(int j = 0;j<2;j++)
                    {
                        summoner[i].SpellTotalTime[j] = Dic[summoner[i].summonerSpell[j]].SummonerCD;
                    }
                    
                }
                await LoadPic();
                IsSync = true;
            }
            catch (Exception e)
            {
                Log.error("Cannot get data from client!" + e);
            } 
        }

        private async Task LoadPic()     
        {
            for (int i = 0; i < 5; i++)
            {
                for(int j = 0;j<2;j++)
                {
                    summoner[i].SpellImg[j].Source = await ImageCache.Instance.Get(Dic[summoner[i].summonerSpell[j]].ImageURL);
                }
                
            }
        }

        public void Type()
        {
            if (IsSync == false) return;
            string[] pos = { "TOP", "TOP", "JUG", "JUG", "MID", "MID", "AD", "AD", "SUP", "SUP" };
            //Thread.Sleep(2000);
            string str = "";
            for(int i = 0; i < 5; i++)
            {
                for(int j = 0;j<2;j++)
                {
                    if (FlashOnly)
                    {
                        if (summoner[i].SpellTime[j] > 0 && (summoner[i].summonerSpell[j] == "SummonerFlash" || summoner[i].summonerSpell[j] == "SummonerTeleport")) str += pos[i] + GetSpellShortName(summoner[i].summonerSpell[j]) + GetTimeInMinute(GameTime + summoner[i].SpellTime[j]) + (" ");
                    }
                    else
                    {
                        if (summoner[i].SpellTime[j] > 0) str += pos[i] + GetSpellShortName(summoner[i].summonerSpell[j]) + GetTimeInMinute(GameTime + summoner[i].SpellTime[j]) + (" ");
                    }
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

        private int GetSpellCD(int i,int j)
        {
            string str = summoner[i].summonerSpell[j];
            if (str == "SummonerTeleport")
            {
                string Url = "https://127.0.0.1:2999/liveclientdata/playerlist";
                string result = "{\n\"Player\": " + Get_UrlData(Url) + "\n}";
                RootObject_Playerlist playerlist = JsonConvert.DeserializeObject<RootObject_Playerlist>(result);
                summoner[i].level = 0;
                int tmp = 0;
                foreach (Player p in playerlist.Player)
                {
                    if (p.team != playerteam)
                    {
                        summoner[tmp].level = (int)Convert.ToSingle(p.level);
                    }
                }
                Log.Info("tp cd:" + (432 - 12 * summoner[i].level - shift).ToString());
                summoner[i].SpellTotalTime[j] = 432 - 12 * summoner[i].level - shift;
                return 432 - 12 * summoner[i].level - shift;
            }
            else
            {
                return summoner[i].SpellTotalTime[j] - shift;
            }
        }
    }
}
