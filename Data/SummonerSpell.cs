using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellTracker.Data
{
    class SummonerSpell
    {
        public int ID { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public int SummonerLevel { get; set; }
        public string ImageURL { get; set; }
        public int SummonerCD { get; set; }
    }
}
