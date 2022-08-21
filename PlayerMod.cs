using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using System;

namespace nexperience1dot4
{
    public class PlayerMod : ModPlayer
    {
        public override bool IsCloneable => false;

        private List<GameModeData> MyGameModes = new List<GameModeData>();
        public GameModeData GetCurrentGamemode { get { return MyGameModes[CurrentGameMode]; } }
        public GameModeData GetPlayerGamemode(string GameModeID)
        {
            foreach(GameModeData gmd in MyGameModes)
            {
                if(gmd.GetGameModeID == GameModeID)
                    return gmd;
            }
            return null;
        }
        private byte CurrentGameMode = 0;
        public float GetHealthPercentageChange { get { return GetCurrentGamemode.HealthPercentageChange; } }
        public float GetManaPercentageChange { get { return GetCurrentGamemode.ManaPercentageChange; } }
        public float ExpPercentage = 1f;
        public int LevelUpEffectTime = -1;
        public const int MaxEffectTime = 180;
        public bool PlayLevelUpEffect = false;

        public override void Unload()
        {
            MyGameModes.Clear();
        }

        public override void ResetEffects()
        {
            ExpPercentage = 1;
        }

        public override void OnEnterWorld(Player player)
        {
            NetplayMod.AskForGameMode(player.whoAmI);
        }

        public override void PreUpdate()
        {
            UpdateLevelUpEffect();
        }

        private void UpdateLevelUpEffect()
        {
            if (LevelUpEffectTime < 0)
            {
                if (PlayLevelUpEffect)
                {
                    PlayLevelUpEffect = false;
                }
                else
                {
                    return;
                }
            }
            LevelUpEffectTime++;
            if(LevelUpEffectTime >= MaxEffectTime)
            {
                LevelUpEffectTime = -1;
            }
        }

        public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
        {
            healValue = (int)(healValue * GetHealthPercentageChange);
        }

        public override void GetHealMana(Item item, bool quickHeal, ref int healValue)
        {
            healValue = (int)(healValue * GetManaPercentageChange);
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            Main.NewText("Damage Received: " + damage);
            return false;
        }

        public override void Initialize()
        {
            foreach (string gamemode in nexperience1dot4.GetGameModeIDs)
            {
                if (gamemode == nexperience1dot4.GetActiveGameModeID)
                    CurrentGameMode = (byte)MyGameModes.Count;
                MyGameModes.Add(new GameModeData(gamemode));
            }
        }

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
        {
            if((damageSource.SourceOtherIndex > -1 && damageSource.SourceOtherIndex < 254 && damageSource.SourceOtherIndex != 13 && damageSource.SourceOtherIndex != 14 && damageSource.SourceOtherIndex != 15))
            {
                damage = (int)(damage * GetHealthPercentageChange);
                if (damage < 1)
                    damage = 1;
            }
            return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);
        }

        public override void PostUpdateEquips()
        {
            GetCurrentGamemode.UpdatePlayer(Player);
        }

        public override void Kill(double damage, int hitDirection, bool pvp, Terraria.DataStructures.PlayerDeathReason damageSource)
        {
            DeathExpPenalty();
        }

        private void DeathExpPenalty()
        {
            SpawnExpText(Player, GetCurrentGamemode.GetDeathExpPenalty());
        }

        public static void AddPlayerExp(Player player, float Exp, Rectangle sourcerect = default(Rectangle))
        {
            AddPlayerExp(player, (int)(Exp), sourcerect);
        }
        
        public static void AddPlayerExp(Player player, int Exp, Rectangle sourcerect = default(Rectangle))
        {
            if (Exp == 0)
                return;
            PlayerMod pm = player.GetModPlayer<PlayerMod>();
            Exp = (int)(Math.Max(1, Exp * nexperience1dot4.ExpRate * pm.ExpPercentage));
            if (player.whoAmI == Main.myPlayer)
            {
                SpawnExpText(player, Exp, sourcerect);
            }
            pm.AddExp(Exp);
        }

        public static void SpawnExpText(Player player, int Exp, Rectangle source = default(Rectangle)){
                if (source == default(Rectangle))
                    source = player.getRect();
                Color color = (Exp == 0 ? Color.White : (Exp > 0 ? Color.Cyan : Color.DarkRed));
                CombatText.NewText(source, color, "Exp " + Exp, true);
        }

        public static BiomeLevelStruct GetPlayerBiomeLevel(Player player)
        {
            return player.GetModPlayer<PlayerMod>().GetCurrentGamemode.GetMyBiome;
        }

        public void AddExp(int Exp)
        {
            int LastLevel = GetCurrentGamemode.GetLevel;
            GetCurrentGamemode.ChangeExp(Exp);
            if(Player.whoAmI == Main.myPlayer){
                if(LastLevel > GetCurrentGamemode.GetLevel)
                {
                        Rectangle rect = new Rectangle((int)Player.Center.X, (int)Player.Top.Y - 8, 4,4);
                        CombatText.NewText(rect, Microsoft.Xna.Framework.Color.Cyan, "Level " + GetCurrentGamemode.GetLevel+ "!", true);
                }
                //    Main.NewText("Achieved level " + GetLevel + "!",Microsoft.Xna.Framework.Color.Cyan);
            }
            else
            {
                NetplayMod.SendExpToPlayer(Player.whoAmI, Exp, Main.myPlayer);
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag.Add("ModVersion", nexperience1dot4.SaveVersion);
            tag.Add("GameModes", MyGameModes.Count);
            for(byte i = 0; i < MyGameModes.Count; i++)
            {
                MyGameModes[i].SaveGameMode(i, tag);
            }
        }

        public override void LoadData(TagCompound tag)
        {
            if (!tag.ContainsKey("ModVersion"))
                return;
            int LastVersion = tag.GetInt("ModVersion");
            int GameModes = tag.GetInt("GameModes");
            for(byte i = 0; i < GameModes; i++)
            {
                bool GameModeExists = false;
                GameModeData gmd = new GameModeData(i, tag, LastVersion);
                if (gmd.GetGameModeID == "")
                    continue;
                for (byte g = 0; g < MyGameModes.Count; g++)
                {
                    if (MyGameModes[g].GetGameModeID == gmd.GetGameModeID)
                    {
                        MyGameModes[g] = gmd;
                        GameModeExists = true;
                        if (gmd.GetGameModeID == nexperience1dot4.GetActiveGameModeID)
                            CurrentGameMode = (byte)g;
                        break;
                    }
                }
                if (!GameModeExists)
                {
                    if (gmd.GetGameModeID == nexperience1dot4.GetActiveGameModeID)
                        CurrentGameMode = (byte)MyGameModes.Count;
                    MyGameModes.Add(gmd);
                }
            }
        }

        private static LevelUpEffect lfx;

        public override void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions)
        {
            lfx = new LevelUpEffect();
            positions.Add(lfx, lfx.GetDefaultPosition());
        }
    }
}
