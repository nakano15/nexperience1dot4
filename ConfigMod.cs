using Terraria;
using Terraria.ID;
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

        [Header("GameplaySettings")]

        [DefaultValue(5)]
        [Range(0, 100)]
        public byte DeathExpPenalty;

        [DefaultValue(true)]
        public bool EnableBiomeLevelCapper;

        [DefaultValue(false)]
        public bool SetEverythingToMyLevel;

        public bool AllowManaBoost;

        [DefaultValue(false)]
        public bool InfiniteLeveling;

        public bool Playthrough1dot5;

        public bool CapLevelOnInfiniteLeveling;

        public bool BuffPreHardmodeEnemiesOnHardmode;

        //public bool MobDefenseToHealth;

        public bool PotionsForSale;

        [DefaultValue(100)]
        [Range(0, 1600)]
        public int ExpRate;

        public bool BossesAsToughAsMe;

        [DefaultValue(true)]
        public bool NTerrariaGraveyard;

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
            nexperience1dot4.SetEverythingToMyLevel = SetEverythingToMyLevel;
            nexperience1dot4.AllowManaBoost = AllowManaBoost;
            nexperience1dot4.Playthrough1dot5 = Playthrough1dot5;
            nexperience1dot4.CapLevelOnInfiniteLeveling = CapLevelOnInfiniteLeveling;
            nexperience1dot4.BuffPreHardmodeEnemiesOnHardmode = BuffPreHardmodeEnemiesOnHardmode;
            //nexperience1dot4.MobDefenseToHealth = MobDefenseToHealth; //Add use
            nexperience1dot4.PotionsForSale = PotionsForSale;
            nexperience1dot4.BossesAsToughAsMe = BossesAsToughAsMe;
        }
    }
    public class ClientConfigMod : ModConfig
    {
        [DefaultValue(1)]
        [Range(0, 1f)]
        public float StatusWindowOpacity;
        
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
