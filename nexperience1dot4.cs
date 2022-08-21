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
        public const int SaveVersion = 0;
        internal static byte DeathExpPenalty = 5;
        internal static bool EnableBiomeLevelCapper = true, InfiniteLeveling = false, NTerrariaGraveyard = false, ZombiesDroppingTombstones = false;
        internal static float ExpRate = 1f;
        internal static Asset<Texture2D> HandyTexture, LevelArrowTexture;
        public static ModPacket packet{
            get
            {
                return ThisMod.GetPacket();
            }
        }

        public override void PostSetupContent()
        {
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
            AddGameMode("nterraregularrpg", new Game_Modes.NTerraRegularRPG());
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
                            if(args[1] is DamageClass && args[2] is byte)
                            {
                                AddDamageClass((DamageClass)args[1], (byte)args[2]);
                                return true;
                            }
                        }
                        break;
                    case "AddGameMode":
                        {
                            if(args[1] is string && args[2] is GameModeBase){
                                AddGameMode((string)args[1], (GameModeBase)args[2]);
                                return true;
                            }
                        }
                        break;
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
            if(GameModes.ContainsKey(ID)){
                ActiveGameMode = ID;
                for(byte i = 0; i < 200; i++)
                {
                    NpcMod.UpdateNpcStatus(Main.npc[i]);
                }
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
    }
}