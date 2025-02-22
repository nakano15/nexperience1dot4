﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace nexperience1dot4.Game_Modes
{
    public class BasicRPG : GameModeBase
    {
        private static int[] MaxExpTable;

        string GetTranslation(string Key)
        {
            return Terraria.Localization.Language.GetTextValue("Mods.nexperience1dot4.GameModes.BasicRPG." + Key);
        }

        public override string Name => GetTranslation("Name");
        public override string Description => GetTranslation("Description");
        public override float InitialStatusPoints => 0;
        public override float StatusPointsPerLevel => 2;
        public override GameModeStatusInfo[] GameModeStatus => new GameModeStatusInfo[]{
            new GameModeStatusInfo(GetTranslation("PhyStatName"), GetTranslation("PhyStatDesc"), GetTranslation("PhyStatShort")),
            new GameModeStatusInfo(GetTranslation("MagStatName"), GetTranslation("MagStatDesc"), GetTranslation("MagStatShort")),
            new GameModeStatusInfo(GetTranslation("HPStatName"), GetTranslation("HPStatDesc"), GetTranslation("HPStatShort")),
            new GameModeStatusInfo(GetTranslation("MPStatName"), GetTranslation("MPStatDesc"), GetTranslation("MPStatShort"))
        };

        public BasicRPG()
        {
            MaxExpTable = new int[100];
            for(byte i = 0; i < MaxExpTable.Length; i++)
            {
                MaxExpTable[i] = GetExpFromLevel(i);
            }
            MobLevels();
            BiomeLevels();
        }

        public void BiomeLevels()
        {
            //phm
            AddBiome("Underworld Island", 1, 5, delegate (Player player) { 
                if (!Main.remixWorld) return false;
                float xpos = player.Center.X * (1f / 16);
                return player.ZoneUnderworldHeight && xpos >= Main.maxTilesX * 0.38f + 50 && xpos <= Main.maxTilesX * 0.62f;
            });
            AddBiome("Forest", 1, 5, delegate (Player player) { return true; });
            AddBiome("Night Forest", 3, 7, delegate (Player player) { return !Main.dayTime; });
            AddBiome("Underground", 3, 7, delegate (Player player) { return player.ZoneNormalUnderground; });
            AddBiome("Cavern", 5, 10, delegate (Player player) { return player.ZoneNormalCaverns; });
            AddBiome("Desert", 1, 5, delegate (Player player) { return player.ZoneDesert && !player.ZoneUndergroundDesert; });
            AddBiome("Antlion Hive", 1, 5, delegate (Player player) { return player.ZoneUndergroundDesert; });
            AddBiome("Corruption", 10, 15, delegate (Player player) { return player.ZoneCorrupt; });
            AddBiome("Crimson", 10, 15, delegate (Player player) { return player.ZoneCrimson; });
            AddBiome("Jungle", 5, 10, delegate (Player player) { return player.ZoneJungle; });
            AddBiome("Underground Jungle", 7, 12, delegate (Player player) { return player.ZoneJungle && player.ZoneRockLayerHeight; });
            AddBiome("Beach", 5, 10, delegate (Player player) { return player.ZoneBeach; });
            AddBiome("Dungeon", 15, 20, delegate (Player player) { return player.ZoneDungeon; });
            AddBiome("Underworld", 15, 20, delegate (Player player) { 
                if (Main.remixWorld)
                {
                    float xpos = player.Center.X * (1f / 16);
                    return player.ZoneUnderworldHeight && (xpos < Main.maxTilesX * 0.38f + 50 || xpos > Main.maxTilesX * 0.62f);
                }
                return player.ZoneUnderworldHeight;
            });
            //hm

            //desu
            AddBiome("Desu", 9999, 9999, delegate (Player player) { return player.ZoneDungeon && !NPC.downedBoss3; }, false);
        }

        public void MobLevels()
        {
            AddMobLevel(NPCID.EyeofCthulhu, 10);
            AddMobLevel(NPCID.EaterofWorldsHead, 20);
            AddMobLevel(NPCID.SkeletronHead, 30);
            AddMobLevel(NPCID.QueenBee, 30);
            AddMobLevel(NPCID.KingSlime, 20);
            AddMobLevel(NPCID.Deerclops, 35);
            AddMobLevel(NPCID.WallofFlesh, 40);
        }

        public override void UpdatePlayerStatus(Player player, GameModeData data)
        {
            int PATK = data.GetEffectiveStatusValue(0),
                MATK = data.GetEffectiveStatusValue(1),
                HP = data.GetEffectiveStatusValue(2),
                MP = data.GetEffectiveStatusValue(3);

            player.statLifeMax2 = (int)((player.statLifeMax2 + HP * 5) * (0.5f + 0.035f * (data.GetEffectiveLevel - 1)));
            player.statManaMax2 = (int)((player.statManaMax2 + MP * 5) * (1 + 0.03f * (data.GetEffectiveLevel - 1)));

            float Increase = 1f + (data.GetEffectiveLevel - 1) * 0.04f;
            data.MeleeDamagePercentage *= Increase + PATK * 0.05f;
            data.RangedDamagePercentage *= Increase + PATK * 0.05f;
            data.MagicDamagePercentage *= Increase + MATK * 0.05f;
            data.SummonDamagePercentage *= Increase + MATK * 0.05f;
            data.GenericDamagePercentage *= Increase + (PATK + MATK) * 0.025f;

            player.statDefense = player.statDefense * Increase;

            Increase = 0.5f + (data.GetEffectiveLevel - 1) * 0.035f;

            data.MeleeCriticalPercentage = Increase;
            data.RangedCriticalPercentage = Increase;
            data.MagicCriticalPercentage = Increase;
        }

        public override void UpdateNpcStatus(NPC npc, GameModeData data)
        {
            if(npc.lifeMax > 5)
            {
                data.NpcHealthMult += 0.04f * (data.GetEffectiveLevel - 1);
                data.NpcHealthSum += data.GetEffectiveLevel * 20;
            }
            float Increase = 1f + 0.12f * (data.GetEffectiveLevel - 1);
            data.NpcDamageMult = Increase;
            data.NpcDefense = Increase;
        }

        public override int GetExpReward(NPC npc, GameModeData data)
        {
            if (npc.lifeMax <= 5) return 0;
            return (int)(npc.lifeMax * 0.5f);
        }

        public override void OnUnload()
        {
            MaxExpTable = null;
        }

        public override int GetLevelExp(int Level)
        {
            int MaxExp = 0;
            if(Level >= 100)
            {
                MaxExp = MaxExpTable[99] * (Level / 100);
                Level = Level % 100;
            }
            MaxExp += MaxExpTable[Level];
            return MaxExp;
        }

        private int GetExpFromLevel(int Level)
        {
            return 100 + 25 * Level * Level + 40 * (Level - 1);
        }
    }
}
