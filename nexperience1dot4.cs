using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace nexperience1dot4
{
	public class nexperience1dot4 : Mod
	{
        public const string ContentPath = "nexperience1dot4/Content/";
		private static Dictionary<string, GameModeBase> GameModes = new Dictionary<string, GameModeBase>();
        private static GameModeBase DefaultGameMode = new GameModeBase();
        private static string ActiveGameMode = "";
        public const int SaveVersion = 0;
        internal static byte DeathExpPenalty = 5;
        internal static bool EnableBiomeLevelCapper = true, InfiniteLeveling = false;
        internal static float ExpRate = 1f;
        internal static Asset<Texture2D> HandyTexture, LevelArrowTexture;

        public override void PostSetupContent()
        {
            AddGameMode("basicrpg", new Game_Modes.BasicRPG());
        }

        public override void Load()
        {
            HandyTexture = ModContent.Request<Texture2D>(ContentPath + "Interface/WhiteDot");
            LevelArrowTexture = ModContent.Request<Texture2D>(ContentPath + "Interface/LevelArrow");
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

    }
}