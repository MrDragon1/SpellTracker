using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellTracker.Control
{
    public class RootObject_Gamestats
    {
        public string gameMode { get; set; }
        public string gameTime { get; set; }
        public string mapName { get; set; }
        public string mapNumber { get; set; }
        public string mapTerrain { get; set; }
    }

    public class Items
    {
        public string canUse { get; set; }
        public string consumable { get; set; }
        public int count { get; set; }
        public string displayName { get; set; }
        public int itemID { get; set; }
        public int price { get; set; }
        public string rawDescription { get; set; }
        public string rawDisplayName { get; set; }
        public int slot { get; set; }
    }

    public class Keystone
    {
        public string displayName { get; set; }
        public string id { get; set; }
        public string rawDescription { get; set; }
        public string rawDisplayName { get; set; }
    }

    public class PrimaryRuneTree
    {
        public string displayName { get; set; }
        public string id { get; set; }
        public string rawDescription { get; set; }
        public string rawDisplayName { get; set; }
    }

    public class SecondaryRuneTree
    {
        public string displayName { get; set; }
        public string id { get; set; }
        public string rawDescription { get; set; }
        public string rawDisplayName { get; set; }
    }

    public class Runes
    {
        public Keystone keystone { get; set; }
        public PrimaryRuneTree primaryRuneTree { get; set; }
        public SecondaryRuneTree secondaryRuneTree { get; set; }
    }

    public class Scores
    {
        public string assists { get; set; }
        public string creepScore { get; set; }
        public string deaths { get; set; }
        public string kills { get; set; }
        public string wardScore { get; set; }
    }

    public class SummonerSpellOne
    {
        public string displayName { get; set; }
        public string rawDescription { get; set; }
        public string rawDisplayName { get; set; }
    }

    public class SummonerSpellTwo
    {
        public string displayName { get; set; }
        public string rawDescription { get; set; }
        public string rawDisplayName { get; set; }
    }

    public class SummonerSpells
    {
        public SummonerSpellOne summonerSpellOne { get; set; }
        public SummonerSpellTwo summonerSpellTwo { get; set; }
    }

    public class Player
    {
        public string championName { get; set; }
        public string isBot { get; set; }
        public string isDead { get; set; }
        public List<Items> items { get; set; }
        public string level { get; set; }
        public string position { get; set; }
        public string rawChampionName { get; set; }
        public string rawSkinName { get; set; }
        public string respawnTimer { get; set; }
        public Runes runes { get; set; }
        public Scores scores { get; set; }
        public string skinID { get; set; }
        public string skinName { get; set; }
        public string summonerName { get; set; }
        public SummonerSpells summonerSpells { get; set; }
        public string team { get; set; }
    }

    public class RootObject_Playerlist
    {
        public List<Player> Player { get; set; }
    }
}
