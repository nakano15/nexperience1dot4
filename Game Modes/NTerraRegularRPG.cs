using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace nexperience1dot4.Game_Modes
{
    public class NTerraRegularRPG : GameModeBase
    {
        private static int[] MaxExpTable;

        public override string Name => "Regular RPG Mode";
        public override string Description => "The default leveling mode of N Terraria, with ascending progression available.";
        public override float InitialStatusPoints => 0;
        public override float StatusPointsPerLevel => 1;
        public override float InitialStatusPointsDistribution => 5;
        public override GameModeStatusInfo[] GameModeStatus => new GameModeStatusInfo[]{
            new GameModeStatusInfo("Strength",
            "(+) Melee Damage, (+/2) Melee Critical Rate, \n(+/2) Critical Damage, (+/2) Defense", "STR", 0, 150),
            new GameModeStatusInfo("Agility",
            "(+) Melee Speed, (+) Movement Speed, (+/2) Ranged Damage.", "AGI", 0, 150),
            new GameModeStatusInfo("Vitality",
            "(+) Defense, (+) Max Health, (+) Health Regeneration.", "VIT", 0, 150),
            new GameModeStatusInfo("Intelligence",
            "(+) Magic Damage, (+) Mana Cost, \n(+) Critical Damage, (+/2) Summon Damage", "INT", 0, 150),
            new GameModeStatusInfo("Dexterity",
            "(+) Ranged Damage, (+) Critical Damage, \n(+/2) Ranged Critical Chance", "DEX", 0, 150),
            new GameModeStatusInfo("Luck",
            "(+) Critical Rate, (+) Luck Strike Chance", "LUK", 0, 150),
            new GameModeStatusInfo("Charisma",
            "(+) Summon Damage, (+/2) Max Health, (+) Max Summon.", "CHA", 0, 150),
            new GameModeStatusInfo("Wisdom",
            "(-) Mana Cost, (+) Mana Regeneration, \n(+) Critical Damage, (+/2) Magic Damage.", "WIS", 0, 150)
        };

        public NTerraRegularRPG(){
            CreateExpTable();
            SetupBiomes();
            SetupNpcLevels();
        }

        private void CreateExpTable(){
            if(MaxExpTable == null){
                List<int> ExpTable = new List<int>();
                int CurrentExp = 400;
                float Growth = 0.25f, GrowthLevels = 3;
                while(ExpTable.Count <= 150)
                {
                    ExpTable.Add(Math.Abs(CurrentExp));
                    int Exp = (int)(CurrentExp * (Growth - (((float)ExpTable.Count / GrowthLevels) * 0.01f)));
                    GrowthLevels += 0.02f;
                    CurrentExp += Exp;
                }
                MaxExpTable = ExpTable.ToArray();
                ExpTable.Clear();
            }
        }

        public override int GetLevelExp(int Level)
        {
            return CalculateOverflowedExpStack(Level, MaxExpTable);
        }

        public override void UpdatePlayerStatus(Player player, GameModeData data)
        {
            int STR = data.GetEffectiveStatusValue(0),
                AGI = data.GetEffectiveStatusValue(1),
                VIT = data.GetEffectiveStatusValue(2),
                INT = data.GetEffectiveStatusValue(3),
                DEX = data.GetEffectiveStatusValue(4),
                LUK = data.GetEffectiveStatusValue(5),
                CHA = data.GetEffectiveStatusValue(6),
                WIS = data.GetEffectiveStatusValue(7);
            int Level = data.GetEffectiveLevel;
            float HealthChangeValue = 1f, ManaChangeValue = 1f, DefenseChangeValue = 1f;
            float SecondaryBonus = 0.5f, Bonus = 1f, TotalBonus = 0.06f; //SecondaryBonus = 1.5f, Bonus = 0.5f;
            HealthChangeValue += (VIT + CHA * SecondaryBonus + Level * Bonus) * TotalBonus;
            ManaChangeValue += (WIS + INT * SecondaryBonus + Level * Bonus) * TotalBonus;
            data.MeleeDamagePercentage += (STR + DEX * SecondaryBonus) * TotalBonus;
            data.RangedDamagePercentage += (DEX + AGI * SecondaryBonus) * TotalBonus;
            data.MagicDamagePercentage += (INT + WIS * SecondaryBonus) * TotalBonus;
            data.SummonDamagePercentage += (CHA + INT * SecondaryBonus) * TotalBonus;
            data.GenericDamagePercentage += Level * Bonus * TotalBonus;
            DefenseChangeValue += (VIT + STR * SecondaryBonus + Level * Bonus) * TotalBonus;
            data.MeleeSpeedPercentage += (AGI + Level * 0.5f) * 0.0055f;
            player.moveSpeed += (AGI + Level * 0.5f) * 0.0055f;
            data.MeleeCriticalPercentage += (LUK + STR * 0.33f) * 0.0133f;
            data.RangedCriticalPercentage += (LUK + DEX * 0.33f) * 0.0133f;
            data.MagicCriticalPercentage += (LUK + INT * 0.33f) * 0.0133f;
            player.manaCost += (INT * 0.5f - WIS * 0.25f) * 0.05f;
            player.maxMinions += (int)(Math.Min(150, CHA) * 0.05f);

            const int BoostLevelStart = 100;
            if(Level > BoostLevelStart){
                float Boost = (Level - BoostLevelStart) * 0.025f;
                HealthChangeValue += Boost;
                data.MeleeDamagePercentage += Boost;
                data.RangedDamagePercentage += Boost;
                data.MagicDamagePercentage += Boost;
                data.SummonDamagePercentage += Boost;
                data.GenericDamagePercentage += Boost;
                DefenseChangeValue += Boost;
            }

            const int HealthReductionLevels = 50;
            if(Level < HealthReductionLevels)
            {
                float Penalty = 1f - (Level * (1f / HealthReductionLevels));
                HealthChangeValue *= 1f - Penalty * 0.5f;
            }

            player.statLifeMax2 = (int)(player.statLifeMax2 * HealthChangeValue);
            player.statManaMax2 = (int)(player.statManaMax2 * ManaChangeValue);
            player.statDefense = player.statDefense * DefenseChangeValue;
        }

        public override void UpdateNpcStatus(NPC npc, GameModeData data)
        {
            int Level = data.GetEffectiveLevel;
            data.NpcDamageMult += Level * 0.1f;
            data.NpcDefense += Level * 0.1f;
            data.ProjectileNpcDamagePercentage += Level * 0.1f;
            if(npc.lifeMax > 5)
            {
                npc.lifeMax += (int)(npc.lifeMax * Level * 0.1f);
            }
            if(npc.lifeMax > 5)
            {
                float ExpReward = npc.lifeMax + (npc.damage * data.NpcDamageMult + npc.defense * data.NpcDefense) * 8;
                const int ExpReductionMaxLevel = 60;
                const float ExpReductionPercentage = 1f / ExpReductionMaxLevel;
                if(Level < ExpReductionMaxLevel)
                {
                    ExpReward -= ExpReward * ((ExpReductionMaxLevel - Level) * ExpReductionPercentage);
                }
                data.SetExpReward(ExpReward);
            }
        }

        public override void OnUnload()
        {
            MaxExpTable = null;
        }

        public void SetupBiomes(){
            //PHM
            AddBiome("Underworld Island", 1, 7, 7, 14, delegate (Player p){ 
                if (!Main.remixWorld) return false;
                float xpos = p.Center.X * (1f / 16);
                return p.ZoneUnderworldHeight && xpos >= Main.maxTilesX * 0.38f + 50 && xpos <= Main.maxTilesX * 0.62f;
            });
            AddBiome("Forest", 1, 7, 7, 14, delegate (Player p){ return true;});
            //Caves
            AddBiome("Underground", 9, 15, delegate (Player p){ return p.ZoneDirtLayerHeight;});
            AddBiome("Cavern", 15, 23, delegate (Player p){ return !Main.remixWorld && p.ZoneRockLayerHeight;});
            //Snowland
            AddBiome("Tundra", 1, 7, 8, 15, delegate (Player p){ return !Main.remixWorld && p.ZoneSnow;});
            AddBiome("Ice Caves", 9, 15, delegate (Player p){ return p.ZoneSnow && p.ZoneDirtLayerHeight && !Main.dayTime;});
            AddBiome("Ice Caves", 16, 22, delegate (Player p){ return p.ZoneSnow && p.ZoneRockLayerHeight && !Main.dayTime;});
            //Desert
            AddBiome("Desert", 1, 7, 8, 15, delegate (Player p){ return !Main.remixWorld && p.ZoneDesert;});
            AddBiome("Antlion Hive", 18, 24, delegate (Player p){ return p.ZoneUndergroundDesert;});
            AddBiome("Beach", 1, 7, 7, 14, delegate (Player p){ return p.ZoneBeach;});
            //Graveyard
            AddBiome("Graveyard", 20, 30, delegate (Player p){ return p.ZoneGraveyard;});
            //Corruption
            AddBiome("Corruption", 23, 27, delegate (Player p){ return p.ZoneCorrupt;});
            AddBiome("Crimson", 23, 27, delegate (Player p){ return p.ZoneCrimson;});
            //Hallow (Behaves like Forest)
            AddBiome("Hallow", 1, 7, 7, 14, delegate (Player p){ return !Main.remixWorld && p.ZoneHallow;});
            //Jungle
            AddBiome("Jungle", 30, 37, delegate (Player p){ return !Main.remixWorld && p.ZoneJungle;});
            AddBiome("Underground Jungle", 34, 40, delegate (Player p){ return p.ZoneJungle && (p.ZoneDirtLayerHeight || p.ZoneRockLayerHeight);});

            //HM
            //
            AddBiome("Forest", 1, 7, 56, 64, delegate (Player p){ return !Main.remixWorld && Main.hardMode;});
            //Caves
            AddBiome("Underground", 60, 68, delegate (Player p){ return Main.hardMode && p.ZoneDirtLayerHeight;});
            AddBiome("Cavern", 64, 72, delegate (Player p){ return Main.hardMode && p.ZoneRockLayerHeight;});
            //Snowland
            AddBiome("Tundra", 1, 7, 61, 69, delegate (Player p){ return !Main.remixWorld && Main.hardMode && p.ZoneSnow;});
            AddBiome("Ice Caves", 59, 65, delegate (Player p){ return Main.hardMode && p.ZoneSnow && p.ZoneDirtLayerHeight && !Main.dayTime;});
            AddBiome("Ice Caves", 66, 76, delegate (Player p){ return Main.hardMode && p.ZoneSnow && p.ZoneRockLayerHeight && !Main.dayTime;});
            //Desert
            AddBiome("Desert", 63, 70, delegate (Player p){ return !Main.remixWorld && Main.hardMode && p.ZoneDesert;});
            AddBiome("Antlion Hive", 72, 80, delegate (Player p){ return Main.hardMode && p.ZoneUndergroundDesert;});
            AddBiome("Beach", 65, 72, delegate (Player p){ return Main.hardMode && p.ZoneBeach;});
            //Graveyard
            AddBiome("Graveyard", 70, 80, delegate (Player p){ return Main.hardMode && p.ZoneGraveyard;});
            //Corruption
            AddBiome("Corruption", 54, 62, delegate (Player p){ return Main.hardMode && p.ZoneCorrupt;});
            AddBiome("Corrupted Caves", 63, 75, delegate (Player p){ return Main.hardMode && p.ZoneCorrupt && p.ZoneRockLayerHeight;});
            AddBiome("Crimson", 54, 62, delegate (Player p){ return Main.hardMode && p.ZoneCrimson;});
            AddBiome("Crimson Caves", 63, 75, delegate (Player p){ return Main.hardMode && p.ZoneCrimson && p.ZoneRockLayerHeight;});
            //Hallow (Behaves like Forest)
            AddBiome("Hallow", 50, 58, 56, 64, delegate (Player p){ return !Main.remixWorld && Main.hardMode && p.ZoneHallow;});
            AddBiome("Hallowed Caves", 63, 75, delegate (Player p){ return Main.hardMode && p.ZoneHallow && p.ZoneRockLayerHeight;});
            //Jungle
            AddBiome("Jungle", 80, 87, delegate (Player p){ return Main.hardMode && p.ZoneJungle;});
            AddBiome("Underground Jungle", 88, 100, delegate (Player p){ return Main.hardMode && p.ZoneJungle && p.ZoneRockLayerHeight;});

            //Immutable Order
            AddBiome("Dungeon", 40, 50, delegate (Player p){ return p.ZoneDungeon;});
            AddBiome("Desu~", 9999, 9999, delegate (Player p){ return p.ZoneDungeon && !NPC.downedBoss3;}, false);

            //Remix Diferences
            AddBiome("Dark World", 40, 50, delegate(Player p)
            {
                return Main.remixWorld && p.ZoneOverworldHeight;
            });
            AddBiome("Dark World", 90, 100, delegate(Player p)
            {
                return Main.remixWorld && p.ZoneOverworldHeight && Main.hardMode;
            });
            AddBiome("Dungeon", 50, 60, delegate (Player p){ return Main.remixWorld && p.ZoneDungeon;});
            
            //Immutable Order
            AddBiome("Lihzahrd Temple", 100, 110, delegate (Player p){ return Main.hardMode && p.ZoneLihzhardTemple;});

            //Underworld
            AddBiome("Underworld", 50, 60, delegate (Player p){ 
                if (Main.remixWorld)
                {
                    float xpos = p.Center.X * (1f / 16);
                    return p.ZoneUnderworldHeight && (xpos < Main.maxTilesX * 0.38f + 50 || xpos > Main.maxTilesX * 0.62f);
                }
                return p.ZoneUnderworldHeight;});
            //
            AddBiome("Dungeon", 110, 120, delegate (Player p){ return Main.hardMode && NPC.downedPlantBoss && p.ZoneDungeon;});
            //Underworld
            AddBiome("Underworld", 80, 90, delegate (Player p){ 
                if (!(Main.hardMode && p.ZoneUnderworldHeight && NPC.downedMechBossAny)) return false;
                if (Main.remixWorld)
                {
                    float xpos = p.Center.X * (1f / 16);
                    return p.ZoneUnderworldHeight && (xpos < Main.maxTilesX * 0.38f + 50 || xpos > Main.maxTilesX * 0.62f);
                }
                return true;
                });
            
            //Towers
            AddBiome("Stardust Tower", 140, 150, delegate (Player p){ return p.ZoneTowerStardust;});
            AddBiome("Nebula Tower", 140, 150, delegate (Player p){ return p.ZoneTowerNebula;});
            AddBiome("Solar Tower", 140, 150, delegate (Player p){ return p.ZoneTowerSolar;});
            AddBiome("Vortex Tower", 140, 150, delegate (Player p){ return p.ZoneTowerVortex;});

        }

        public void SetupNpcLevels(){
            AddMobLevel(23, 35); //Meteor Head
            //Goblin Army
            AddMobLevel(73, 27); //Goblin Scout
            AddMobLevel(26, 26); //Goblin Peon
            AddMobLevel(29, 24); //Goblin Sorcerer
            AddMobLevel(27, 22); //Goblin Thief
            AddMobLevel(28, 28); //Goblin Warrior
            AddMobLevel(111, 27); //Goblin Archer
            AddMobLevel(471, 70); //Goblin Summoner
            //Frost Legion
            AddMobLevel(144, 74); //Mister Stabby
            AddMobLevel(143, 69); //Snowman Gangsta
            AddMobLevel(145, 65); //Snow Balla
            //Pirate Army
            AddMobLevel(212, 72); //Pirate Deckhand
            AddMobLevel(213, 76); //Pirate Corsair
            AddMobLevel(214, 75); //Pirate Deadeye
            AddMobLevel(215, 78); //Pirate Crossbower
            AddMobLevel(216, 83); //Pirate Captain
            AddMobLevel(662, 83); //Pirate's Curse
            AddMobLevel(252, 70); //Parrot
            AddMobLevel(491, 85); //Flying Dutchman
            //Martian Madness
            AddMobLevel(381, 113); //Martian Ranger
            AddMobLevel(382, 113); //Martian Ranger
            AddMobLevel(383, 120); //Martian Officer
            AddMobLevel(385, 110); //Martian Grunty
            AddMobLevel(386, 115); //Martian Engineer
            AddMobLevel(387, 115); //Tesla Turret
            AddMobLevel(388, 116); //Gigazapper
            AddMobLevel(390, 120); //Scutlix Gunner
            AddMobLevel(391, 120); //Scutlix
            AddMobLevel(520, 122); //Martian Walker
            AddMobLevel(395, 125); //Martian Saucer
            //Solar Eclipse
            AddMobLevel(166, 70); //Swamp Thing
            AddMobLevel(158, 75); //Vampire
            AddMobLevel(159, 75); //Vampire
            AddMobLevel(251, 80); //Eyezor
            AddMobLevel(162, 73); //Frankenstein
            AddMobLevel(469, 83); //The Possessed
            AddMobLevel(462, 77); //Fritz
            AddMobLevel(461, 85); //Creature from the Deep
            AddMobLevel(253, 90); //Reaper
            AddMobLevel(477, 100); //Mothron
            AddMobLevel(478, 100); //Mothron Egg
            AddMobLevel(479, 100); //Baby Mothron
            AddMobLevel(460, 105); //Butcher
            AddMobLevel(467, 110); //Deadly Sphere
            AddMobLevel(466, 107); //Psycho
            AddMobLevel(468, 115); //Dr Man Fly
            AddMobLevel(463, 120); //Nailhead
            //Bosses and Minibosses
            AddMobLevel(4, 15); //Eye of Cthulhu
            AddMobLevel(5, 15); //Eye of Cthulhu
            
            AddMobLevel(13, 25); //Eater of Worlds
            AddMobLevel(14, 25); //Eater of Worlds
            AddMobLevel(15, 25); //Eater of Worlds

            AddMobLevel(266, 25); //Brain of Cthulhu
            AddMobLevel(267, 25); //Creeper

            AddMobLevel(35, 40); //Skeletron Head
            AddMobLevel(36, 40); //Skeletron Hand
            
            AddMobLevel(50, 30); //King Slime
            
            AddMobLevel(222, 45); //Queen Bee?

            AddMobLevel(668, 50); //Deerclops
            
            AddMobLevel(113, 60); //Wall of Flesh
            AddMobLevel(114, 60); //Wall of Flesh Eye
            AddMobLevel(115, 60); //The Hungry
            AddMobLevel(116, 60); //The Hungry II
            
            AddMobLevel(125, 70); //The Twins
            AddMobLevel(126, 70); //The Twins
            
            AddMobLevel(134, 80); //The Destroyer
            AddMobLevel(135, 80);
            AddMobLevel(136, 80);
            AddMobLevel(139, 80); //Probe
            
            AddMobLevel(127, 90); //Skeletron Prime and its parts
            AddMobLevel(128, 90); 
            AddMobLevel(129, 90); 
            AddMobLevel(130, 90); 
            AddMobLevel(131, 90);
            
            AddMobLevel(657, 65); //Queen Slime

            AddMobLevel(243, 70); //Ice Golem
            AddMobLevel(541, 70); //Sand Elemental

            AddMobLevel(262, 100); //Plantera
            AddMobLevel(263, 100); //Plantera
            AddMobLevel(264, 100); //Plantera
            AddMobLevel(265, 100); //Plantera
            
            AddMobLevel(245, 110); //Golem
            AddMobLevel(246, 110); //Golem
            AddMobLevel(247, 110); //Golem
            AddMobLevel(248, 110); //Golem
            AddMobLevel(249, 110); //Golem
            
            AddMobLevel(636, 120); //Empress of Light
            
            AddMobLevel(370, 130); //Duke Fishron
            AddMobLevel(371, 130); //Duke Fishron
            AddMobLevel(372, 130); //Duke Fishron
            AddMobLevel(373, 130); //Duke Fishron
            
            AddMobLevel(439, 140); //Lunatic Cultist
            AddMobLevel(440, 140);
            AddMobLevel(454, 140);
            AddMobLevel(455, 140);
            AddMobLevel(456, 140);
            AddMobLevel(457, 140);
            AddMobLevel(458, 140);
            AddMobLevel(459, 140);
            AddMobLevel(521, 140);
            AddMobLevel(522, 140);
            AddMobLevel(523, 140);
            
            AddMobLevel(396, 150); //Moon Lord
            AddMobLevel(397, 150);
            AddMobLevel(398, 150);
            AddMobLevel(400, 150);
            AddMobLevel(401, 150);
            
            AddMobLevel(493, 145); //Towers
            AddMobLevel(507, 145);
            AddMobLevel(422, 145);
            AddMobLevel(517, 145);

            AddMobLevel(87, 70); //Wyvern

            //Blood Moon Minibosses
            AddMobLevel(618, 70); //Dreadnautilus
            AddMobLevel(619, 70); //Blood Squid

            //Add Event Mobs
            //DD2
            AddMobLevel(new int[]{
                NPCID.DD2DarkMageT1,
                NPCID.DD2GoblinBomberT1,
                NPCID.DD2WyvernT1,
                NPCID.DD2GoblinT1,
                NPCID.DD2SkeletonT1,
                NPCID.DD2JavelinstT1
            }, 30); //Tier 1
            AddMobLevel(new int[]{
                NPCID.DD2OgreT2,
                NPCID.DD2DrakinT2,
                NPCID.DD2GoblinT2,
                NPCID.DD2WyvernT2,
                NPCID.DD2JavelinstT2,
                NPCID.DD2KoboldFlyerT2,
                NPCID.DD2WitherBeastT2,
                NPCID.DD2GoblinBomberT2,
                NPCID.DD2KoboldWalkerT2
            }, 70); //Tier 2
            AddMobLevel(new int[]{
                NPCID.DD2OgreT3,
                NPCID.DD2DrakinT3,
                NPCID.DD2GoblinT3,
                NPCID.DD2WyvernT3,
                NPCID.DD2DarkMageT3,
                NPCID.DD2SkeletonT3,
                NPCID.DD2JavelinstT3,
                NPCID.DD2KoboldFlyerT3,
                NPCID.DD2WitherBeastT3,
                NPCID.DD2GoblinBomberT3,
                NPCID.DD2KoboldWalkerT3,
                NPCID.DD2LightningBugT3
            }, 100); //Tier 3
            AddMobLevel(NPCID.DD2EterniaCrystal, 20);
            AddMobLevel(NPCID.DD2EterniaCrystal, 70, true, delegate(){ return Terraria.GameContent.Events.DD2Event.ReadyForTier2; });
            AddMobLevel(NPCID.DD2EterniaCrystal, 100, true, delegate(){ return Terraria.GameContent.Events.DD2Event.ReadyForTier3; });
            
            AddMobLevel(NPCID.DD2Betsy, 100);

            //Pumpkin Moon
            AddMobLevel(new int[]{305, 306, 307, 308, 309, 310, 311, 312, 313, 314}, 120); //Scarecrows
            AddMobLevel(326, 124); //Splinterling
            AddMobLevel(329, 128); //Hellhound
            AddMobLevel(330, 132); //Poltergeist
            AddMobLevel(315, 136); //Headless Horseman
            AddMobLevel(325, 140); //Mourning Wood
            AddMobLevel(new int[]{327, 328}, 140); //Pumpking

            //Frost Legion
            AddMobLevel(341, 130); //Present Mimic
            AddMobLevel(new int[]{338,339,340}, 120); //Zombie Elf
            AddMobLevel(342, 128); //Gingerbread Man
            AddMobLevel(350, 124); //Elf Archer
            AddMobLevel(new int[]{348, 349}, 128); //Nutcracker
            AddMobLevel(344, 140); //Everscream
            AddMobLevel(347, 132); //Elf Copter
            AddMobLevel(346, 140); //Santa-NK1
            AddMobLevel(351, 136); //Krampus
            AddMobLevel(352, 128); //Flocko
            AddMobLevel(343, 136); //Yeti
            AddMobLevel(345, 140); //Ice Queen
        }

        public override int GetExpReward(float RewardLevel, float Percentage)
        {
            float LevelValue = RewardLevel * 0.01f * GetMaxLevel;
            if(LevelValue == 0) LevelValue = 1;
            else if (RewardLevel > 10)
            {
                LevelValue -= LevelValue / (LevelValue + 10);
            }
            if(LevelValue > GetMaxLevel) LevelValue = GetMaxLevel;
            //if(LevelValue < 0) LevelValue = 0;
            return (int)(GetLevelExp((int)LevelValue) * Percentage);
        }
    }
}