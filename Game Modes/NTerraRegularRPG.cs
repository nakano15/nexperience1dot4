using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace nexperience1dot4.Game_Modes
{
    public class NTerraRegularRPG : GameModeBase
    {
        private static int[] MaxExpTable;

        public override string Name => "Regular RPG Mode";
        public override string Description => "The default leveling mode of N Terraria, with ascending progression disponible.";
        public override float InitialStatusPoints => 1;
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
            "(+) Summon Damage, (+/2) Max Health.", "CHA", 0, 150),
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
            const int HealthReductionLevels = 50;
            float HealthChangeValue = 1f, ManaChangeValue = 1f, DefenseChangeValue = 1f;
            if(Level < HealthReductionLevels)
            {
                float Penalty = 1f - (Level * (1f / HealthReductionLevels));
                HealthChangeValue -= Penalty * 0.5f;
            }
            float SecondaryBonus = 1.5f, Bonus = 0.5f, TotalBonus = 0.06f;
            HealthChangeValue += (VIT + CHA * SecondaryBonus + Level * Bonus) * TotalBonus;
            ManaChangeValue += (WIS + INT * SecondaryBonus + Level * Bonus) * TotalBonus;
            data.MeleeDamagePercentage += (STR + DEX * SecondaryBonus + Level * Bonus) * TotalBonus;
            data.RangedDamagePercentage += (DEX + AGI * SecondaryBonus + Level * Bonus) * TotalBonus;
            data.MagicDamagePercentage += (INT + WIS * SecondaryBonus + Level * Bonus) * TotalBonus;
            data.SummonDamagePercentage += (CHA + INT * SecondaryBonus + Level * Bonus) * TotalBonus;
            data.GenericDamagePercentage += Level * Bonus * TotalBonus;
            DefenseChangeValue += (VIT + STR * SecondaryBonus + Level * Bonus) * TotalBonus;
            data.MeleeSpeedPercentage += (AGI + Level * 0.5f) * 0.0055f;
            player.moveSpeed += (AGI + Level * 0.5f) * 0.0055f;
            data.MeleeCriticalPercentage += (LUK + STR * 0.33f) * 0.0133f;
            data.RangedCriticalPercentage += (LUK + DEX * 0.33f) * 0.0133f;
            data.MagicCriticalPercentage += (LUK + INT * 0.33f) * 0.0133f;
            player.manaCost += (INT * 0.5f - WIS * 0.25f) * 0.05f;
            player.maxMinions += (int)(Math.Min(150, CHA) * 0.005f);

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

            player.statLifeMax2 = (int)(player.statLifeMax2 * HealthChangeValue);
            player.statManaMax2 = (int)(player.statManaMax2 * ManaChangeValue);
            player.statDefense = (int)(player.statDefense * DefenseChangeValue);
        }

        public override void UpdateNpcStatus(NPC npc, GameModeData data)
        {
            int Level = data.GetEffectiveLevel;
            npc.damage += (int)(npc.damage * Level * 0.15f);
            npc.defense += (int)(npc.defense * Level * 0.15f);
            if(npc.lifeMax > 5)
            {
                npc.lifeMax += (int)(npc.lifeMax * Level * 0.1f);
            }

            if(npc.lifeMax > 5) data.SetExpReward(npc.lifeMax + (npc.damage + npc.defense) * 8);
        }

        public override void OnUnload()
        {
            MaxExpTable = null;
        }

        public void SetupBiomes(){
            //PHM
            AddBiome("Forest", 1, 7, 7, 14, delegate (Player p){ return true;});
            //Caves
            AddBiome("Underground", 9, 15, delegate (Player p){ return p.ZoneDirtLayerHeight;});
            AddBiome("Cavern", 15, 23, delegate (Player p){ return p.ZoneRockLayerHeight;});
            //Snowland
            AddBiome("Tundra", 1, 7, 8, 15, delegate (Player p){ return p.ZoneSnow;});
            AddBiome("Ice Caves", 9, 15, delegate (Player p){ return p.ZoneSnow && p.ZoneDirtLayerHeight && !Main.dayTime;});
            AddBiome("Ice Caves", 16, 22, delegate (Player p){ return p.ZoneSnow && p.ZoneRockLayerHeight && !Main.dayTime;});
            //Desert
            AddBiome("Desert", 1, 7, 8, 15, delegate (Player p){ return p.ZoneDesert;});
            AddBiome("Antlion Hive", 18, 24, delegate (Player p){ return p.ZoneUndergroundDesert;});
            AddBiome("Beach", 27, 32, delegate (Player p){ return p.ZoneBeach;});
            //Graveyard
            AddBiome("Graveyard", 20, 30, delegate (Player p){ return p.ZoneGraveyard;});
            //Corruption
            AddBiome("Corruption", 23, 27, delegate (Player p){ return p.ZoneCorrupt;});
            AddBiome("Crimson", 23, 27, delegate (Player p){ return p.ZoneCrimson;});
            //Hallow (Behaves like Forest)
            AddBiome("Hallow", 1, 7, 7, 14, delegate (Player p){ return p.ZoneHallow;});
            //Jungle
            AddBiome("Jungle", 30, 37, delegate (Player p){ return p.ZoneJungle;});
            AddBiome("Underground Jungle", 34, 40, delegate (Player p){ return p.ZoneJungle && (p.ZoneDirtLayerHeight || p.ZoneRockLayerHeight);});
            //
            AddBiome("Dungeon", 40, 50, delegate (Player p){ return p.ZoneDungeon;});
            AddBiome("Desu~", 9999, 9999, delegate (Player p){ return p.ZoneDungeon && !NPC.downedBoss3;});
            //Underworld
            AddBiome("Underworld", 50, 60, delegate (Player p){ return p.ZoneUnderworldHeight;});

            //HM
            //
            AddBiome("Forest", 1, 7, 56, 64, delegate (Player p){ return Main.hardMode;});
            //Caves
            AddBiome("Underground", 60, 68, delegate (Player p){ return Main.hardMode && p.ZoneDirtLayerHeight;});
            AddBiome("Cavern", 64, 72, delegate (Player p){ return Main.hardMode && p.ZoneRockLayerHeight;});
            //Snowland
            AddBiome("Tundra", 1, 7, 61, 69, delegate (Player p){ return Main.hardMode && p.ZoneSnow;});
            AddBiome("Ice Caves", 59, 65, delegate (Player p){ return Main.hardMode && p.ZoneSnow && p.ZoneDirtLayerHeight && !Main.dayTime;});
            AddBiome("Ice Caves", 66, 76, delegate (Player p){ return Main.hardMode && p.ZoneSnow && p.ZoneRockLayerHeight && !Main.dayTime;});
            //Desert
            AddBiome("Desert", 63, 70, delegate (Player p){ return Main.hardMode && p.ZoneDesert;});
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
            AddBiome("Hallow", 50, 58, 56, 64, delegate (Player p){ return Main.hardMode && p.ZoneHallow;});
            AddBiome("Hallowed Caves", 63, 75, delegate (Player p){ return Main.hardMode && p.ZoneHallow && p.ZoneRockLayerHeight;});
            //Jungle
            AddBiome("Jungle", 80, 87, delegate (Player p){ return Main.hardMode && p.ZoneJungle;});
            AddBiome("Underground Jungle", 88, 100, delegate (Player p){ return Main.hardMode && p.ZoneJungle && p.ZoneRockLayerHeight;});
            AddBiome("Lihzahrd Temple", 100, 115, delegate (Player p){ return Main.hardMode && p.ZoneLihzhardTemple;});
            //
            AddBiome("Dungeon", 115, 130, delegate (Player p){ return Main.hardMode && NPC.downedPlantBoss && p.ZoneDungeon;});
            //Underworld
            AddBiome("Underworld", 80, 90, delegate (Player p){ return Main.hardMode && p.ZoneUnderworldHeight && NPC.downedMechBossAny;});
            //Towers
            AddBiome("Stardust Tower", 130, 140, delegate (Player p){ return p.ZoneTowerStardust;});
            AddBiome("Nebula Tower", 130, 140, delegate (Player p){ return p.ZoneTowerNebula;});
            AddBiome("Solar Tower", 130, 140, delegate (Player p){ return p.ZoneTowerSolar;});
            AddBiome("Vortex Tower", 130, 140, delegate (Player p){ return p.ZoneTowerVortex;});

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
            AddMobLevel(381, 103); //Martian Ranger
            AddMobLevel(382, 103); //Martian Ranger
            AddMobLevel(383, 110); //Martian Officer
            AddMobLevel(385, 100); //Martian Grunty
            AddMobLevel(386, 105); //Martian Engineer
            AddMobLevel(387, 105); //Tesla Turret
            AddMobLevel(388, 106); //Gigazapper
            AddMobLevel(390, 110); //Scutlix Gunner
            AddMobLevel(391, 110); //Scutlix
            AddMobLevel(520, 112); //Martian Walker
            AddMobLevel(395, 115); //Martian Saucer
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
            
            AddMobLevel(370, 140); //Duke Fishron
            AddMobLevel(371, 140); //Duke Fishron
            AddMobLevel(372, 140); //Duke Fishron
            AddMobLevel(373, 140); //Duke Fishron
            
            AddMobLevel(439, 130); //Lunatic Cultist
            AddMobLevel(440, 130);
            AddMobLevel(454, 130);
            AddMobLevel(455, 130);
            AddMobLevel(456, 130);
            AddMobLevel(457, 130);
            AddMobLevel(458, 130);
            AddMobLevel(459, 130);
            AddMobLevel(521, 130);
            AddMobLevel(522, 130);
            AddMobLevel(523, 130);
            
            AddMobLevel(396, 150); //Moon Lord
            AddMobLevel(397, 150);
            AddMobLevel(398, 150);
            AddMobLevel(400, 150);
            AddMobLevel(401, 150);
            
            AddMobLevel(493, 140); //Towers
            AddMobLevel(507, 140);
            AddMobLevel(422, 140);
            AddMobLevel(517, 140);

            AddMobLevel(87, 70); //Wyvern

            //Add Event Mobs
        }
    }
}