using Terraria;
using System.Collections.Generic;

namespace nexperience1dot4
{
    public class GameModeBase
    {
        public virtual string Name { get { return "Unknown"; } }
        public virtual string Description { get { return "No description."; } }
        public virtual GameModeStatusInfo[] GameModeStatus { get { return new GameModeStatusInfo[0]; } }
        public virtual float StatusPointsPerLevel { get { return 1; } }
        public virtual float InitialStatusPoints { get { return 0; } }
        public virtual float InitialStatusPointsDistribution { get { return 0; } }
        private Dictionary<int, MobLevelStruct> GameModeMobLevels = new Dictionary<int, MobLevelStruct>();
        private int MaxLevel = 1;
        private bool MaxLevelForced = false;
        public int GetMaxLevel { get { return MaxLevel; } }
        private List<BiomeLevelStruct> BiomeLevels = new List<BiomeLevelStruct>();

        protected void SetMaxLevel(int Level)
        {
            MaxLevel = Level;
            MaxLevelForced = true;
        }

        private void UpdateLevelCap(int NewHighestLevelEntry)
        {
            if (!MaxLevelForced && NewHighestLevelEntry > MaxLevel)
                MaxLevel = NewHighestLevelEntry;
        }

        public void AddBiome(string Name, int MinLevel, int MaxLevel, BiomeLevelStruct.IsBiomeActiveDel BiomeActiveReq, bool CountsTowardsLevelCap = true)
        {
            AddBiome(Name, MinLevel, MaxLevel, -1, -1, BiomeActiveReq, CountsTowardsLevelCap);
        }

        public void AddBiome(string Name, int MinLevel, int MaxLevel, int NightMinLevel, int NightMaxLevel, BiomeLevelStruct.IsBiomeActiveDel BiomeActiveReq, bool CountsTowardsLevelCap = true)
        {
            BiomeLevelStruct biome = new BiomeLevelStruct(Name, MinLevel, MaxLevel, BiomeActiveReq, NightMinLevel, NightMaxLevel);
            int Position = -1;
            for(int i = 0; i < BiomeLevels.Count; i++)
            {
                if (MinLevel < BiomeLevels[i].GetMinLevel)
                    Position = i;
            }
            if (Position == -1)
                BiomeLevels.Add(biome);
            else
                BiomeLevels.Insert(Position, biome);
            if (CountsTowardsLevelCap) UpdateLevelCap(MaxLevel);
        }

        public BiomeLevelStruct GetBiomeActiveForPlayer(Player player)
        {
            BiomeLevelStruct b = null;
            foreach(BiomeLevelStruct biome in BiomeLevels)
            {
                if (biome.CheckIfIsActive(player))
                    b = biome;
            }
            return b;
        }

        public void AddMobLevel(int MobID, int Level, bool CountsTowardsLevelCap = true)
        {
            MobLevelStruct moblvlinfo = new MobLevelStruct() { Level = Level };
            if (GameModeMobLevels.ContainsKey(MobID))
                GameModeMobLevels.Remove(MobID);
            GameModeMobLevels.Add(MobID, moblvlinfo);
            if (CountsTowardsLevelCap)
                UpdateLevelCap(Level);
        }

        public int GetMobLevel(NPC npc)
        {
            if (GameModeMobLevels.ContainsKey(npc.type))
                return GameModeMobLevels[npc.type].Level;
            return 0;
        }

        public virtual void UpdatePlayerStatus(Player player, GameModeData data)
        {

        }

        public virtual void UpdateNpcStatus(NPC npc, GameModeData data)
        {

        }

        public virtual void OnUnload()
        {

        }

        public virtual int GetLevelExp(int Level)
        {
            return 100 * Level * Level;
        }

        protected int CalculateOverflowedExpStack(int Level, int[] ExpTable){
            int MaxLevel = ExpTable.Length;
            int CurLevel = Level;
            int Result = 0;
            while(CurLevel >= MaxLevel)
            {
                CurLevel -= MaxLevel;
                try{
                    checked{
                        Result += ExpTable[MaxLevel];
                    }
                }
                catch
                {
                    return int.MaxValue;
                }
            }
            try{
                checked{
                    Result += ExpTable[CurLevel];
                }
            }
            catch
            {
                return int.MaxValue;
            }
            return Result;
        }
    }
}
