using Terraria;
using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;
using Newtonsoft.Json;

namespace nexperience1dot4
{
    public class ServerConfigMod : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;
        [JsonIgnore]
        private static string[] _GameModeInfos = new string[0];
        private string GameModeID { get { return nexperience1dot4.GetActiveGameModeID; } set { nexperience1dot4.ChangeActiveGameMode(value); } }

        [Header("Gameplay Settings")]

        [Label("Death Exp Penalty Percentage")]
        [Tooltip("Allows you to change the exp penalty upon death.")]
        [DefaultValue(5)]
        [Range(0, 100)]
        public byte DeathExpPenalty;

        [Label("Enable Biome Level Capper?")]
        [Tooltip("When enabled, your character level will be capped to\nbiome maximum level, if overleveled to it.\nHaving a higher leveled monster around\nwill increase the level capper.\nYou can still gain exp and level\nup while under effect of the level capper.")]
        [DefaultValue(true)]
        public bool EnableBiomeLevelCapper;

        [Label("Infinite Leveling?")]
        [Tooltip("Enabling this, will make so you can level up past level cap.\nThe mod will try to make up the exp progression past max level.")]
        [DefaultValue(false)]
        public bool InfiniteLeveling;

        [Label("Exp Rate")]
        [Tooltip("Allows you to change the rate at which your character gains exp.\nValue is in percentage.")]
        [DefaultValue(100)]
        [Range(0, 1600)]
        public int ExpRate;

        [Label("N Terraria Graveyard Biome")]
        [Tooltip("Yes! Yes! Yes!")]
        [DefaultValue(true)]
        public bool NTerrariaGraveyard;

        [Label("Zombies Drops Tombstones?")]
        [DefaultValue(true)]
        public bool ZombieDroppingTombstone;

        public static void PopulateGameModes()
        {
            List<string> GameModes = new List<string>();
            int Number = 0;
            foreach(string s in nexperience1dot4.GetGameModeIDs)
            {
                GameModes.Add(Number + ": " + nexperience1dot4.GetGameMode(s).Name);
                Number++;
            }
            _GameModeInfos = GameModes.ToArray();
            GameModes.Clear();
        }

        public static void EraseGameModesList()
        {
            _GameModeInfos = null;
        }

        public override void OnLoaded()
        {

        }

        public override void OnChanged()
        {
            nexperience1dot4.DeathExpPenalty = DeathExpPenalty;
            nexperience1dot4.EnableBiomeLevelCapper = EnableBiomeLevelCapper;
            nexperience1dot4.InfiniteLeveling = InfiniteLeveling;
            nexperience1dot4.ExpRate = ExpRate * 0.01f;
            nexperience1dot4.NTerrariaGraveyard = NTerrariaGraveyard;
            nexperience1dot4.ZombiesDroppingTombstones = ZombieDroppingTombstone;
        }
    }
    public class ClientConfigMod : ModConfig
    {
        [Label("Status Window Transparency")]
        [Tooltip("This will change the opacity of the status window.")]
        [DefaultValue(1)]
        [Range(0, 1f)]
        public float StatusWindowOpacity;
        
        [Label("Display Exp Reward as Percentage")]
        [Tooltip("Changes the exp reward displayed to be in percentage.")]
        [DefaultValue(false)]
        public bool ExpRewardAsPercentage;

        public override ConfigScope Mode => ConfigScope.ClientSide;

        public override void OnChanged()
        {
            Interfaces.LevelInfos.Transparency = StatusWindowOpacity;
            nexperience1dot4.DisplayExpRewardAsPercentage = ExpRewardAsPercentage;
        }
    }
}
