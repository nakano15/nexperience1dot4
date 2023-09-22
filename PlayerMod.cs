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
        int LastHealthRegenValue = 0;

        public override void Unload()
        {
            MyGameModes.Clear();
        }

        internal static void UpdatePlayersGameModes()
        {
            for(int i = 0; i < 255; i++)
            {
                if (Main.player[i].active)
                {
                    PlayerMod pm = Main.player[i].GetModPlayer<PlayerMod>();
                    for(byte gm = 0; gm < pm.MyGameModes.Count; gm++)
                    {
                        if (pm.MyGameModes[gm].GetGameModeID == nexperience1dot4.GetActiveGameModeID)
                        {
                            pm.CurrentGameMode = gm;
                            break;
                        }
                    }
                }
            }
        }

        public override void ResetEffects()
        {
            if(Player.ZoneGraveyard)
                Player.townNPCs = -200;
            ExpPercentage = 1;
        }

        public override void OnEnterWorld()
        {
            NetplayMod.AskForGameMode(Player.whoAmI);
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

        public override void UpdateLifeRegen()
        {
            if (LastHealthRegenValue > 0 && Player.lifeRegenCount >= 0)
            {
                if (LastHealthRegenValue > Player.lifeRegenCount)
                {
                    Player.statLife += (int)(GetHealthPercentageChange * (LastHealthRegenValue / 120 + 1));
                }
            }
            else if (LastHealthRegenValue < 0 && Player.lifeRegenCount <= 0)
            {
                if (LastHealthRegenValue > Player.lifeRegenCount)
                {
                    Player.statLife -= (int)(GetHealthPercentageChange * (LastHealthRegenValue / -120 + 1));
                }
            }
            LastHealthRegenValue = Player.lifeRegenCount;
        }

        public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
        {
            healValue = (int)(healValue * GetHealthPercentageChange);
        }

        public override void GetHealMana(Item item, bool quickHeal, ref int healValue)
        {
            healValue = (int)(healValue * GetManaPercentageChange);
        }

        private int LastLifeRegenCount = 0;
        private float HealthRegenStack = 0;
        private byte TicksPassed = 0;

        public override void PostUpdateMiscEffects()
        {
        }

        public override void NaturalLifeRegen(ref float regen)
        {
            const float HealthChangePerFrame = 0.0138888888889f;
            bool UpdateHealthRegen = Player.lifeRegenCount != LastLifeRegenCount; //(int)((Player.lifeRegenCount) * DivisionBy60) != (int)((LastLifeRegenCount) * DivisionBy60);
            bool BadRegen = Player.lifeRegenCount < 0 || LastLifeRegenCount < 0;
            LastLifeRegenCount = Player.lifeRegenCount;
            const byte MaxTicksForLifeRegen = 30;
            if(UpdateHealthRegen)
            {
                TicksPassed += (byte)(Math.Abs(Player.lifeRegen));
                float HealthChangeValue = (GetHealthPercentageChange - 1) * HealthChangePerFrame;
                if(HealthChangeValue <= 0)
                {
                    return;
                }
                else
                {
                    if(!BadRegen)
                    {
                        HealthRegenStack += HealthChangeValue;
                        if(HealthRegenStack >= 1 && TicksPassed >= MaxTicksForLifeRegen)
                        {
                            TicksPassed -= MaxTicksForLifeRegen;
                            int HealthChange = (int)HealthRegenStack;
                            HealthRegenStack -= HealthChange;
                            Player.statLife += HealthChange;
                            if(Player.statLife > Player.statLifeMax2)
                                Player.statLife = Player.statLifeMax2;
                        }
                    }
                    else
                    {
                        if (Player.starving)
                        {
                            TicksPassed -= MaxTicksForLifeRegen;
                            int StarvationDamage = (int)(Math.Min(2, Player.statLifeMax2 * 0.05f) * 120 * HealthChangeValue * ((Player.ZoneDesert || Player.ZoneSnow) ? 2 : 1));
                            HealthRegenStack -= StarvationDamage;
                            if(HealthRegenStack <= -1 && TicksPassed >= MaxTicksForLifeRegen)
                            {
                                int HealthChange = (int)HealthRegenStack;
                                HealthRegenStack += HealthChange;
                                Player.statLife += HealthChange;
                                CombatText.NewText(Player.getRect(), CombatText.LifeRegen, -HealthChange, dot: true);
                                if (Player.statLife <= 0 && Player.whoAmI == Main.myPlayer)
                                {
                                    Player.KillMe(PlayerDeathReason.ByOther(18), 10, 0);
                                }
                            }
                            return;
                        }
                        HealthRegenStack -= HealthChangeValue;
                        if(HealthRegenStack <= -1 && TicksPassed >= MaxTicksForLifeRegen)
                        {
                            TicksPassed -= MaxTicksForLifeRegen;
                            int HealthChange = (int)HealthRegenStack;
                            HealthRegenStack -= HealthChange;
                            Player.statLife += HealthChange;
                            CombatText.NewText(Player.getRect(), CombatText.LifeRegen, -HealthChange, dot: true);
                            if(Player.statLife <= 0 && Player.whoAmI == Main.myPlayer)
                            {
                                if (Player.burned || (Player.tongued && Main.expertMode))
                                {
                                    Player.KillMe(PlayerDeathReason.ByOther(8), 10, 0);
                                }
                                else if(Player.suffocating)
                                {
                                    Player.KillMe(PlayerDeathReason.ByOther(8), 10, 0);
                                }
                                else if (Player.poisoned || Player.venom)
                                {
                                    Player.KillMe(PlayerDeathReason.ByOther(9), 10, 0);
                                }
                                else if(Player.electrified)
                                {
                                    Player.KillMe(PlayerDeathReason.ByOther(10), 10, 0);
                                }
                                else
                                {
                                    Player.KillMe(PlayerDeathReason.ByOther(8), 10, 0);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                TicksPassed = 0;
            }
        }

        public override void Initialize()
        {
            foreach (string gamemode in nexperience1dot4.GetGameModeIDs)
            {
                if (gamemode == nexperience1dot4.GetActiveGameModeID)
                    CurrentGameMode = (byte)MyGameModes.Count;
                MyGameModes.Add(new GameModeData(gamemode, true));
            }
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if((modifiers.DamageSource.SourceOtherIndex > -1 && modifiers.DamageSource.SourceOtherIndex < 254 && modifiers.DamageSource.SourceOtherIndex != 13 && modifiers.DamageSource.SourceOtherIndex != 14 && modifiers.DamageSource.SourceOtherIndex != 15))
            {
                modifiers.FinalDamage = modifiers.FinalDamage * GetHealthPercentageChange;
                //if (damage < 1)
                //    damage = 1;
            }
        }

        public override void PostUpdateEquips()
        {
            GetCurrentGamemode.UpdatePlayer(Player);
        }

        public override void Kill(double damage, int hitDirection, bool pvp, Terraria.DataStructures.PlayerDeathReason damageSource)
        {
            DeathExpPenalty();
        } 
        
        public static int GetPlayerLevel(Player player){
            return GetPlayerLevel(player.whoAmI);
        }

        public static int GetPlayerLevel(int PlayerID){
            if(PlayerID > 255 || PlayerID < 0)
                return 1;
            return Main.player[PlayerID].GetModPlayer<PlayerMod>().GetCurrentGamemode.GetLevel;
        }

        private void DeathExpPenalty()
        {
            SpawnExpText(Player, GetCurrentGamemode.DoDeathExpPenalty());
        }

        public static void AddPlayerExp(Player player, float Exp, Rectangle sourcerect = default(Rectangle), float ExtraExpIncrease = 0)
        {
            AddPlayerExp(player, (int)(Exp), sourcerect, ExtraExpIncrease);
        }
        
        public static void AddPlayerExp(Player player, int Exp, Rectangle sourcerect = default(Rectangle), float ExtraExpIncrease = 0)
        {
            if (Exp == 0)
                return;
            PlayerMod pm = player.GetModPlayer<PlayerMod>();
            float ExtraPercentage = pm.GetExtraExpPercentage() + ExtraExpIncrease;
            int BoostedExp = (int)(Math.Max(1, Exp * ExtraPercentage) * nexperience1dot4.ExpRate);
            if (player.whoAmI == Main.myPlayer)
            {
                SpawnExpText(player, (int)(Math.Max(1, Exp * nexperience1dot4.ExpRate)), ExtraPercentage, sourcerect);
            }
            pm.AddExp(BoostedExp);
        }

        public float GetExtraExpPercentage()
        {
            return ExpPercentage;
        }

        public static void SpawnExpText(Player player, int RawExp, float ExtraExpPercent = 1, Rectangle source = default(Rectangle)){
            if (source == default(Rectangle))
                source = player.getRect();
            Color color = (RawExp == 0 ? Color.White : (RawExp > 0 ? Color.Cyan : Color.DarkRed));
            PlayerMod pm = player.GetModPlayer<PlayerMod>();
            string ExpText = "Exp ";
            if(!nexperience1dot4.DisplayExpRewardAsPercentage)
                ExpText += RawExp.ToString();
            else
            {
                float Percentage = (float)MathF.Round((float)RawExp * 100 / pm.GetCurrentGamemode.GetMaxExp, 2);
                if(RawExp >= 0 && Percentage < 0.01f)
                {
                    ExpText += "< 0.01%";
                }
                else
                {
                    ExpText += Percentage + "%";
                }
            }
            //Add extra exp percentage show.
            if(ExtraExpPercent != 1)
            {
                ExtraExpPercent -= 1;
                int PercentValue = (int)(MathF.Round(ExtraExpPercent * 100));
                ExpText += " (" + (PercentValue >= 0 ? "+" : "") + PercentValue + "%)";
            }
            CombatText.NewText(source, color, ExpText, true);
        }

        public static BiomeLevelStruct GetPlayerBiomeLevel(Player player)
        {
            return player.GetModPlayer<PlayerMod>().GetCurrentGamemode.GetMyBiome;
        }

        public void AddExp(int Exp)
        {
            bool LeveledUp = GetCurrentGamemode.ChangeExp(Exp);
            if(Main.netMode < 2)
            {
                if(Player.whoAmI == Main.myPlayer && LeveledUp)
                {
                    CombatText.NewText(Player.getRect(), Microsoft.Xna.Framework.Color.Yellow, "Level Up!", true);
                    //Main.NewText("Achieved level " + GetCurrentGamemode.GetLevel + "!",Microsoft.Xna.Framework.Color.Blue);
                    if(Main.netMode == 1)
                    {
                        NetplayMod.SendPlayerLevel(Player.whoAmI, -1, Player.whoAmI);
                    }
                }
            }
            else
            {
                NetplayMod.SendExpToPlayer(Player.whoAmI, Exp, Main.myPlayer);
            }
        }

        public static Player[] GetPlayerTeammates(Player player)
        {
            List<Player> players = new List<Player>();
            for(byte p2 = 0; p2 < 255; p2++)
            {
                Player otherplayer = Main.player[p2];
                if(!otherplayer.active || (Main.netMode > 0 && otherplayer.team != player.team) || Math.Abs(otherplayer.Center.X - player.Center.X) > 1500 || Math.Abs(otherplayer.Center.Y - player.Center.Y) > 1500) continue;
                players.Add(otherplayer);
            }
            return players.ToArray();
        }

        public void GetExpReward(float RewardLevel, float Percentage, bool ShowNotification = true)
        {
            int ExpReward = GetCurrentGamemode.GetBase.GetExpReward(RewardLevel, Percentage);
            if(ExpReward < 1)
                ExpReward = 1;
            AddPlayerExp(Player, ExpReward);
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

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            NetplayMod.SendPlayerLevel(Player.whoAmI, -1, fromWho);
            NetplayMod.SendPlayerStatus(Player.whoAmI, -1, fromWho);
        }
    }
}
