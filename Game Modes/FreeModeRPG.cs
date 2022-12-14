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

        public FreeModeRPG()
        {
            SetMaxLevel(int.MaxValue);
        }

        public override void UpdatePlayerStatus(Player player, GameModeData data)
        {
            int Level = data.GetEffectiveLevel;
            float StatusBonus = Level * 0.0001f;
            data.HealthPercentageChange += StatusBonus * 100;
            data.ManaPercentageChange += StatusBonus * 100;
            data.MeleeDamagePercentage += StatusBonus;
            data.RangedDamagePercentage += StatusBonus;
            data.MagicDamagePercentage += StatusBonus;
            data.SummonDamagePercentage += StatusBonus;
            data.GenericDamagePercentage += StatusBonus;
            player.statDefense += (int)(StatusBonus);
        }

        public override void UpdateNpcStatus(NPC npc, GameModeData data)
        {
            float StatusIncrease = data.GetEffectiveLevel * 0.01f;
            if(npc.lifeMax > 5)
                npc.lifeMax += (int)(npc.lifeMax * StatusIncrease);
            npc.damage += (int)(npc.damage * StatusIncrease);
            data.ProjectileNpcDamagePercentage += StatusIncrease;
            data.SetExpReward(npc.lifeMax * 2 + npc.damage * 4 + npc.defense * 8 + data.GetEffectiveLevel * 16);
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
            Vector2 NewPosition = Position * 0.0625f;
            int HorizontalLevel = (int)Math.Abs(NewPosition.X - Main.spawnTileX) / 8,
                VerticalLevel = (int)Math.Abs(NewPosition.Y - Main.spawnTileY) / 8;
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
    }
}