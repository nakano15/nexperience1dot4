using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace nexperience1dot4.Game_Modes
{
    public class FreeModeRPG : GameModeBase
    {
        public override string Name => "Free RPG";
        public override string Description => "";
        public override float InitialStatusPoints => 0;
        public override float StatusPointsPerLevel => 0.01f;
        public override bool EnableLevelCapping => false;
        private static int ZoneLevel = -1;
        private static string ZoneLevelText = null;
        public override float LevelChangeFactor => 0.1f;
        public override GameModeStatusInfo[] GameModeStatus => GetStatus();

        public FreeModeRPG()
        {
            SetMaxLevel(int.MaxValue);
        }

        private GameModeStatusInfo[] GetStatus()
        {
            GameModeStatusInfo[] s = new GameModeStatusInfo[7];
            s[0] = new GameModeStatusInfo("Swordsman", 
                "MHP [++++], Melee-Damage [+++], DEF [++], Ranged-Damage [+]",
                "SWD");
            s[1] = new GameModeStatusInfo("Ranger", 
                "Ranged-Damage [++++], Summon-Damage [+++], Melee-Damage [++], DEF [+]",
                "RNG");
            s[2] = new GameModeStatusInfo("Magician", 
                "Magic-Damage [++++], Minion-Damage [+++], Ranged-Damage [++], Melee-Damage [+]",
                "MGC");
            s[3] = new GameModeStatusInfo("Thief", 
                "Melee-Damage [++++], Ranged-Damage [+++], MHP [++], Defense [+]",
                "THF");
            s[4] = new GameModeStatusInfo("Acolyte", 
                "MHP [++++], Magic-Damage [+++], Melee-Damage [++], Summon-Damage [+]",
                "ACO");
            s[5] = new GameModeStatusInfo("Summoner", 
                "Summon-Damage [++++], Ranged-Damage [+++], Magic-Damage [++], MHP [+]",
                "SUM");
            s[6] = new GameModeStatusInfo("Sorcerer", 
                "Summon-Damage [++++], Magic-Damage [+++], Ranged-Damage [++], MHP [+]",
                "SUM");
            return s;
        }

        public override void UpdatePlayerStatus(Player player, GameModeData data)
        {
            int Level = data.GetEffectiveLevel;
            float StatusBonus = Level * 0.0001f;
            float HealthChangeValue = StatusBonus * 100;
            float ManaChangeValue = StatusBonus * 100;
            //data.MeleeDamagePercentage += StatusBonus;
            //data.RangedDamagePercentage += StatusBonus;
            //data.MagicDamagePercentage += StatusBonus;
            //data.SummonDamagePercentage += StatusBonus;
            data.GenericDamagePercentage += StatusBonus;
            player.statDefense += (int)(StatusBonus * 100);

            int fgt = data.GetStatusValue(0),
                ran =  data.GetStatusValue(1),
                mag =  data.GetStatusValue(2),
                thi =  data.GetStatusValue(3),
                aco =  data.GetStatusValue(4),
                sum =  data.GetStatusValue(5),
                sor =  data.GetStatusValue(6);
            
            player.statLifeMax2 += 40 * fgt;
            data.MeleeDamagePercentage += .3f * fgt;
            player.statDefense += 2 * fgt;
            data.RangedDamagePercentage += fgt * .1f;

            data.RangedDamagePercentage += .4f * ran;
            data.SummonDamagePercentage += .3f * ran;
            data.MeleeDamagePercentage += .2f * ran;
            player.statDefense += ran;

            data.MagicDamagePercentage += mag * .4f;
            data.SummonDamagePercentage += mag * .3f;
            data.RangedDamagePercentage += mag * .2f;
            data.MeleeDamagePercentage += mag * .1f;

            data.MeleeDamagePercentage += thi * .4f;
            data.RangedDamagePercentage += thi * .3f;
            player.statLifeMax2 += thi * 20;
            player.statDefense += thi;

            player.statLifeMax2 += aco * 40;
            data.MagicDamagePercentage += aco * .3f;
            data.MeleeDamagePercentage += aco * .2f;
            data.SummonDamagePercentage += aco * .1f;

            data.SummonDamagePercentage += sum * .4f;
            data.RangedDamagePercentage += sum * .3f;
            data.MagicDamagePercentage += sum * .2f;
            player.statLifeMax2 += sum * 10;

            data.SummonDamagePercentage += sor * .4f;
            data.MagicDamagePercentage += sor * .3f;
            data.RangedDamagePercentage += sor * .2f;
            player.statLifeMax2 += sor * 10;

            try
            {
                checked
                {
                    player.statLifeMax2 += (int)(HealthChangeValue * player.statLifeMax2);
                }
            }
            catch
            {
                player.statLifeMax2 = int.MaxValue;
            }
            if (nexperience1dot4.AllowManaBoost)
            {
                try
                {
                    checked
                    {
                        player.statManaMax2 += (int)(player.statManaMax2 * ManaChangeValue);
                    }
                }
                catch
                {
                    player.statManaMax2 = int.MaxValue;
                }
            }
        }

        public override void UpdateNpcStatus(NPC npc, GameModeData data)
        {
            float StatusIncrease = data.GetEffectiveLevel * 0.01f;
            if(npc.lifeMax > 5)
            {
                data.NpcHealthMult+= StatusIncrease;
                //npc.lifeMax += (int)(npc.lifeMax * StatusIncrease);
            }
            data.NpcDamageMult += StatusIncrease;
            //npc.damage += (int)(npc.damage * StatusIncrease);
            data.ProjectileNpcDamagePercentage += StatusIncrease;
            //data.SetExpReward(npc.lifeMax * 2 + npc.damage * 4 * data.NpcDamageMult + npc.defense * 8 + data.GetEffectiveLevel * 16);
        }

        public override int GetExpReward(NPC npc, GameModeData data)
        {
            return (int)(npc.lifeMax * 2 + npc.damage * 4 * data.NpcDamageMult + npc.defense * 8 + data.GetEffectiveLevel * 16);
        }

        public override int GetNpcLevelProcedural(NPC npc)
        {
            int Level = 1;
            const int LowestStatus = 14 + 6 * 8;
            int npcstatus = npc.lifeMax + npc.damage * 8 + npc.defense * 8;
            bool IsBoss = Terraria.ID.NPCID.Sets.ShouldBeCountedAsBoss[npc.type];
            Level += (npcstatus - LowestStatus) / 8 + GetLevelBonus(npc.Center);
            if(IsBoss)
                Level /= 8;
            if(npc.type >= 134 && npc.type <= 136)
                Level /= 16;
            return Level;
        }

        private static int GetLevelBonus(Vector2 Position)
        {
            const float TileDistanceChange = 1f / (8 * 16);
            //Vector2 NewPosition = Position * 0.0625f;
            int HorizontalLevel = (int)(Math.Abs(Position.X - Main.spawnTileX * 16) * TileDistanceChange),
                VerticalLevel = (int)(Math.Abs(Position.Y - Main.spawnTileY * 16) * TileDistanceChange);
            return HorizontalLevel + VerticalLevel + ProgressionIncrement();
        }

        private static int ProgressionIncrement()
        {
            int LevelBonus = 0;
            if (NPC.downedBoss1)
                LevelBonus += 10;
            if (NPC.downedBoss2)
                LevelBonus += 20;
            if (NPC.downedBoss3)
                LevelBonus += 30;
            if (NPC.downedQueenBee)
                LevelBonus += 20;
            if (NPC.downedGoblins)
                LevelBonus += 25;
            if (NPC.downedSlimeKing)
                LevelBonus += 30;
            if (NPC.downedDeerclops)
                LevelBonus += 40;
            if (Main.hardMode)
                LevelBonus += 50;
            if (NPC.downedMechBoss1)
                LevelBonus += 30;
            if (NPC.downedMechBoss2)
                LevelBonus += 30;
            if (NPC.downedMechBoss3)
                LevelBonus += 30;
            if (NPC.downedPirates)
                LevelBonus += 20;
            if (NPC.downedPlantBoss)
                LevelBonus += 30;
            if (NPC.downedGolemBoss)
                LevelBonus += 30;
            if (NPC.downedEmpressOfLight)
                LevelBonus += 40;
            if (NPC.downedAncientCultist)
                LevelBonus += 30;
            if (NPC.downedMartians)
                LevelBonus += 25;
            if (NPC.downedFrost)
                LevelBonus += 20;
            if (NPC.downedTowerSolar)
                LevelBonus += 25;
            if (NPC.downedTowerStardust)
                LevelBonus += 25;
            if (NPC.downedTowerVortex)
                LevelBonus += 25;
            if (NPC.downedTowerNebula)
                LevelBonus += 25;
            if (NPC.downedMoonlord)
                LevelBonus += 100;
            return LevelBonus;
        }

        public override string GetLevelInfo(GameModeData data, bool Inventory)
        {
            return "Rank: " + RomanAlgarismMaker((int)(data.GetLevel * 0.1f) + 1);
        }

        public static string RomanAlgarismMaker(int Number)
        {
            string Text = "";
            byte MCounter = 0;
            if (Number == 0) return "O";
            while (Number > 0)
            {
                if (Number >= 1000)
                {
                    MCounter++;
                    //Text += "M";
                    Number -= 1000;
                }
                else if (Number >= 900)
                {
                    Text += "CM";
                    Number -= 900;
                }
                else if (Number >= 500)
                {
                    Text += "D";
                    Number -= 500;
                }
                else if (Number >= 400)
                {
                    Text += "CD";
                    Number -= 400;
                }
                else if (Number >= 100)
                {
                    Text += "C";
                    Number -= 100;
                }
                else if (Number >= 90)
                {
                    Text += "XC";
                    Number -= 90;
                }
                else if (Number >= 50)
                {
                    Text += "L";
                    Number -= 50;
                }
                else if (Number >= 40)
                {
                    Text += "XL";
                    Number -= 40;
                }
                else if (Number >= 10)
                {
                    Text += "X";
                    Number -= 10;
                }
                else if (Number >= 9)
                {
                    Text += "IX";
                    Number -= 9;
                }
                else if (Number >= 5)
                {
                    Text += "V";
                    Number -= 5;
                }
                else if (Number >= 4)
                {
                    Text += "IV";
                    Number -= 1000;
                }
                else
                {
                    Text += "I";
                    Number--;
                }
            }
            if (MCounter > 3)
            {
                Text = (MCounter - 3) + "M+MMM" + Text;
                MCounter = 0;
            }
            while (MCounter > 0)
            {
                Text = "M" + Text;
                MCounter--;
            }
            return Text;
        }

        public override string GetZoneInformation(GameModeData data, Player player)
        {
            int NewLevel = GetLevelBonus(player.Center);
            if (NewLevel != ZoneLevel)
            {
                ZoneLevel = NewLevel;
                ZoneLevelText = RomanAlgarismMaker(NewLevel);
            }
            return "(Difficulty: Rank "+ZoneLevelText+")";
        }

        public static void Unload()
        {
            ZoneLevelText = null;
        }
    }
}