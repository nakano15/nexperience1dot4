using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;

namespace nexperience1dot4
{
	public class nexperience1dot4 : Mod
	{
        private static Mod ThisMod;
        public const string ContentPath = "nexperience1dot4/Content/";
		private static Dictionary<string, GameModeBase> GameModes = new Dictionary<string, GameModeBase>();
        private static List<StatusTranslator> StatusList = new List<StatusTranslator>();
        private static GameModeBase DefaultGameMode = new GameModeBase();
        private static string ActiveGameMode = "";
        public const int SaveVersion = 1;
        internal static byte DeathExpPenalty = 5;
        internal static bool EnableBiomeLevelCapper = true, InfiniteLeveling = false, NTerrariaGraveyard = false, ZombiesDroppingTombstones = false;
        public static bool DisplayExpRewardAsPercentage = false;
        internal static float ExpRate = 1f;
        internal static Asset<Texture2D> HandyTexture, LevelArrowTexture;
        public static ModPacket packet{
            get
            {
                return ThisMod.GetPacket();
            }
        }
        public const string RegularRPGModeID = "nterraregularrpg", FreeModeRPGID = "freemode";
        private static string GetSaveFileDirectory { get { return Main.SavePath; } }

        public override void PostSetupContent()
        {
            if(ModLoader.HasMod("ClickerClass"))
            {
                DamageClass FoundClass;
                if(ModContent.TryFind("ClickerClass", "ClickerDamage", out FoundClass))
                {
                    AddDamageClass(FoundClass, StatusTranslator.DC_Melee);
                }
            }
            /*if (ModLoader.HasMod("terraguardians")) //No need for
            {
                ModLoader.GetMod("terraguardians").Call("AddGroupInterfaceHook", ModCompatibility.TerraGuardiansMod.GroupMemberExpProgressHook);
                ModCompatibility.TerraGuardiansMod.Load();
            }*/
            //ModCompatibility.Calamity.Load();
            ServerConfigMod.PopulateGameModes();
            LoadSettings();
        }

        public Mod GetMod(string ModName)
        {
            if(ModLoader.HasMod(ModName)) return ModLoader.GetMod(ModName);
            return null;
        }

        public override void Load()
        {
            ThisMod = this;
            HandyTexture = ModContent.Request<Texture2D>(ContentPath + "Interface/WhiteDot");
            LevelArrowTexture = ModContent.Request<Texture2D>(ContentPath + "Interface/LevelArrow");
            AddDamageClass(DamageClass.Generic, StatusTranslator.DC_Generic);
            AddDamageClass(DamageClass.Melee, StatusTranslator.DC_Melee);
            AddDamageClass(DamageClass.Ranged, StatusTranslator.DC_Ranged);
            AddDamageClass(DamageClass.Magic, StatusTranslator.DC_Magic);
            AddDamageClass(DamageClass.MagicSummonHybrid, StatusTranslator.DC_Magic);
            AddDamageClass(DamageClass.Summon, StatusTranslator.DC_Summon);
            AddDamageClass(DamageClass.Throwing, StatusTranslator.DC_Ranged);
            AddGameMode(RegularRPGModeID, new Game_Modes.NTerraRegularRPG());
            AddGameMode(FreeModeRPGID, new Game_Modes.FreeModeRPG());
        }

        public void AddDamageClass(DamageClass dc, byte CountsAs)
        {
            if(!StatusList.Any(x => x.GetDamageClass == dc)) StatusList.Add(new StatusTranslator(dc, CountsAs));
        }

        internal static List<StatusTranslator> GetStatusLists(){
            return StatusList;
        }

        public override object Call(params object[] args)
        {
            if(args.Length > 0 && args[0] is string)
            {   
                switch((string)args[0])
                {
                    case "AddDamageClass":
                        {
                            if(args.Length >= 3 && args[1] is DamageClass && args[2] is byte)
                            {
                                AddDamageClass((DamageClass)args[1], (byte)args[2]);
                                return true;
                            }
                        }
                        break;
                    case "AddGameMode":
                        {
                            if(args.Length >= 3 && args[1] is string && args[2] is GameModeBase){
                                AddGameMode((string)args[1], (GameModeBase)args[2]);
                                return true;
                            }
                        }
                        break;
                    case "AddMonsterLevel":
                        {
                            ///1 = Game Mode
                            ///2 = Mob ID
                            ///3 = Level
                            ///4 = Counts Towards Level Cap
                            ///5 = Requirement
                            if(args.Length >= 4 && args[1] is string && 
                            (args[2] is int || args[2] is int[]) && 
                            args[3] is int)
                            {
                                string GameModeID = (string)args[1];
                                GameModeBase getGameMode = GetGameMode(GameModeID);
                                if(getGameMode == null)
                                    return false;
                                int Level = (int)args[3];
                                bool CountsTowardsLevelCap = args.Length >= 5 && args[4] is bool ? (bool)args[4] : true;
                                System.Func<bool> Requirement = args.Length >= 6 && args[5] is System.Func<bool> ? (System.Func<bool>)args[5] : null;
                                if(args[2] is int)
                                {
                                    getGameMode.AddMobLevel((int)args[2], Level, CountsTowardsLevelCap, Requirement);
                                    return true;
                                }
                                else if(args[2] is int[])
                                {
                                    getGameMode.AddMobLevel((int[])args[2], Level, CountsTowardsLevelCap, Requirement);
                                    return true;
                                }
                            }
                        }
                        break;
                    case "AddBiomeLevel":
                        {
                            ///1 = Game Mode
                            ///2 = Biome Name
                            ///3 = Min Level
                            ///4 = Max Level
                            ///5 = (int)NightMinLevel : (System.Func<Player, bool>)Biome Active Req
                            ///6 = (int)NightMaxLevel : (bool) CountsTowardsLevelCap
                            ///7 = None: (System.Func<Player, bool>)Biome Active Req
                            ///8 = None: (bool) CountsTowardsLevelCap
                            if((args.Length >= 6 && args[1] is string && args[2] is string && 
                                args[3] is int && args[4] is int && args[5] is System.Func<Player, bool>) ||
                                (args.Length >= 8 && args[1] is string && args[2] is string && 
                                args[3] is int && args[4] is int && args[5] is int && args[6] is int && args[7] is System.Func<Player, bool>))
                            {
                                string GameModeID = (string)args[1];
                                GameModeBase BaseMod = GetGameMode(GameModeID);
                                if(BaseMod == null)
                                    return false;
                                string BiomeName = (string)args[2];
                                int MinLevel = (int)args[3];
                                int MaxLevel = (int)args[4];
                                int NightMinLevel = args.Length >=6 && args[5] is int ? (int)args[5] : -1;
                                int NightMaxLevel = args.Length >=7 && args[6] is int ? (int)args[6] : -1;
                                System.Func<Player, bool> BiomeActiveReq = args.Length >= 6 && args[5] is System.Func<Player, bool> ? (System.Func<Player, bool>)args[5] : 
                                    (args.Length >= 8 && args[7] is System.Func<Player, bool> ? (System.Func<Player, bool>)args[7] : null);
                                bool CountsTowardsLevelCap = args.Length >= 7 && args[6] is bool ? (bool)args[6] : 
                                    (args.Length >= 9 && args[8] is bool ? (bool)args[8] : true);
                                BaseMod.AddBiome(BiomeName, MinLevel, MaxLevel, NightMinLevel, 
                                    NightMaxLevel, BiomeActiveReq, CountsTowardsLevelCap);
                                return true;
                            }
                        }
                        break;
                    case "GetGameModeID":
                        return GetActiveGameModeID;
                    case "GetPlayerLevelFunc": //Returns a Func<Player, int>, which returns the player level when called.
                        return delegate(Player player) { return PlayerMod.GetPlayerLevel(player); };
                    case "SetPlayerLevelFunc": //Returns a Action<Player, int> which allows changing player level at will.
                        return delegate(Player player, int Level) {
                            PlayerMod pm = player.GetModPlayer<PlayerMod>();
                            pm.GetCurrentGamemode.SetLevel(Level);
                         };
                    case "GivePlayerExp":
                        if (args.Length > 2 && args[1] is Player && args[2] is int)
                        {
                            (args[1] as Player).GetModPlayer<PlayerMod>().AddExp((int)args[2]);
                            return true;
                        }
                        return false;
                    case "GivePlayerExpReward":
                        if (args.Length > 3 && args[1] is Player && args[2] is float && args[3] is float)
                        {
                            (args[1] as Player).GetModPlayer<PlayerMod>().GetExpReward((float)args[2], (float)args[3], args.Length == 4 || (bool)args[4]);
                            return true;
                        }
                        return false;
                    case "NormalizeStatusPointsInvested":
                        if (args.Length > 1 && args[1] is Player)
                        {
                            (args[1] as Player).GetModPlayer<PlayerMod>().GetCurrentGamemode.NormalizePointsInvested();
                            return true;
                        }
                        return false;
                }
            }
            return false;
        }

        public override void Unload()
        {
            DefaultGameMode = null;
            foreach (GameModeBase gmb in GameModes.Values)
                gmb.OnUnload();
            GameModes.Clear();
            GameModes = null;
            ActiveGameMode = null;
            HandyTexture = null;
            LevelArrowTexture = null;
            StatusList.Clear();
            StatusList = null;
            Interfaces.LevelInfos.OnUnload();
            ModCompatibility.Calamity.Unload();
            ModCompatibility.TerraGuardiansMod.Unload();
            ServerConfigMod.EraseGameModesList();
            Game_Modes.FreeModeRPG.Unload();
        }

        #region Game Mode Stuff
        /// <summary>
        /// Adds a new game mode with the id specified bellow.
        /// </summary>
        /// <param name="GameModeID">ID of the game mode.</param>
        /// <param name="gamemode">The object containing the game mode infos.</param>
        /// <returns></returns>
        public static bool AddGameMode(string GameModeID, GameModeBase gamemode)
        {
            if (GameModes.ContainsKey(GameModeID))
                return false;
            if (ActiveGameMode == "")
                ActiveGameMode = GameModeID;
            GameModes.Add(GameModeID, gamemode);
            return true;
        }

        public static GameModeBase GetGameMode(string ID)
        {
            if (GameModes.ContainsKey(ID)) return GameModes[ID];
            return DefaultGameMode;
        }

        public static bool HasGameMode(string ID)
        {
            return GameModes.ContainsKey(ID);
        }

        public static bool ChangeActiveGameMode(string ID)
        {
            if(GameModes.ContainsKey(ID))
            {
                ActiveGameMode = ID;
                for(byte i = 0; i < 200; i++)
                {
                    NpcMod.UpdateNpcStatus(Main.npc[i]);
                }
                PlayerMod.UpdatePlayersGameModes();
                SaveSettings();
                return true;
            }
            return false;
        }

        public static string GetActiveGameModeID { get { return ActiveGameMode; } }

        public static GameModeBase GetActiveGameMode()
        {
            if (GameModes.ContainsKey(ActiveGameMode))
                return GameModes[ActiveGameMode];
            return DefaultGameMode;
        }

        public static string[] GetGameModeIDs
        {
            get
            {
                return GameModes.Keys.ToArray();
            }
        }
        #endregion;

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            NetplayMod.ReceivedMessages(reader, whoAmI);
        }
        
        internal static void SaveSettings()
        {
            string FileDirectory = GetSaveFileDirectory + "nexperience.sav";
            if (File.Exists(FileDirectory)) File.Delete(FileDirectory);
            using (FileStream stream = new FileStream(FileDirectory, FileMode.CreateNew))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(SaveVersion);
                    writer.Write(ActiveGameMode);
                }
            }
        }

        internal static void LoadSettings()
        {
            string FileDirectory = GetSaveFileDirectory + "nexperience.sav";
            if (!File.Exists(FileDirectory))
            {
                return;
            }
            using (FileStream stream = new FileStream(FileDirectory, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    int LastVersion = reader.ReadInt32();
                    ActiveGameMode = reader.ReadString();
                }
            }
            if(!HasGameMode(ActiveGameMode))
            {
                ActiveGameMode = GameModes.Keys.First();
            }
            ChangeActiveGameMode(ActiveGameMode);
        }
    }
}