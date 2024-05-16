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
        public virtual bool EnableLevelCapping { get { return true; } }
        public virtual int DefenseToHealthConversionRate { get { return 2; }}
        public virtual float LevelChangeFactor { get { return 1f; } }
        private Dictionary<int, MobLevelStruct> GameModeMobLevels = new Dictionary<int, MobLevelStruct>();
        private int MaxLevel = 1;
        private bool MaxLevelForced = false;
        public int GetMaxLevel { get { return MaxLevel; } }
        private List<BiomeLevelStruct> BiomeLevels = new List<BiomeLevelStruct>();
        public virtual int GetExpReward(NPC npc, GameModeData data)
        {
            return 1;
        }

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

        public void AddBiome(string Name, int MinLevel, int MaxLevel, System.Func<Player, bool> BiomeActiveReq, bool CountsTowardsLevelCap = true)
        {
            AddBiome(Name, MinLevel, MaxLevel, -1, -1, BiomeActiveReq, CountsTowardsLevelCap);
        }

        public void AddBiome(string Name, int MinLevel, int MaxLevel, int NightMinLevel, int NightMaxLevel, System.Func<Player, bool> BiomeActiveReq, bool CountsTowardsLevelCap = true)
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

        public void AddMobLevel(int MobID, int Level, bool CountsTowardsLevelCap = true, System.Func<bool> Requirement = null)
        {
            if (GameModeMobLevels.ContainsKey(MobID)){
                GameModeMobLevels[MobID].AddMobLevel(Level, Requirement);
                //GameModeMobLevels.Remove(MobID);
            }
            else
            {
                MobLevelStruct moblvlinfo = new MobLevelStruct();
                moblvlinfo.AddMobLevel(Level, Requirement);
                GameModeMobLevels.Add(MobID, moblvlinfo);
            }
            if (CountsTowardsLevelCap)
                UpdateLevelCap(Level);
        }
        
        public void AddMobLevel(int[] MobIDs, int Level, bool CountsTowardsLevelCap = true, System.Func<bool> Requirement = null)
        {
            foreach(int ID in MobIDs)
                AddMobLevel(ID, Level, CountsTowardsLevelCap, Requirement);
        }

        public int GetMobLevel(NPC npc)
        {
            if (GameModeMobLevels.ContainsKey(npc.type))
                return GameModeMobLevels[npc.type].TakeLevel();
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
            return 100 * Level;
        }

        public virtual int GetExpReward(float RewardLevel, float Percentage)
        {
            return (int)(GetLevelExp((int)(GetMaxLevel * (RewardLevel * 0.01f))) * Percentage);
        }

        public virtual int GetNpcLevelProcedural(NPC npc)
        {
            return 0;
        }

        public virtual string GetLevelInfo(GameModeData data, bool Inventory)
        {
            if(Inventory)
            {
                string Text = "Level [" + data.GetLevel;
                if(data.GetLevel != data.GetEffectiveLevel){
                    Text += " -> " + data.GetEffectiveLevel + "]";
                }
                else Text += "]";
                return Text;
            }
            else
            {
                return "Level " + data.GetLevel + (data.GetLevel != data.GetEffectiveLevel ? "->" + data.GetEffectiveLevel : "");;
            }
        }

        protected int CalculateOverflowedExpStack(int Level, int[] ExpTable){
            int MaxLevel = ExpTable.Length - 1;
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

        public virtual string GetZoneInformation(GameModeData data, Player player)
        {
            BiomeLevelStruct biome = data.GetMyBiome;
            if(biome == null)
                return "No Information";
            return biome.GetBiomeName + " Lv[" + biome.GetMinLevel + "~" + biome.GetMaxLevel + "]";
        }

        public virtual int GetTileBreakingExp(int Tile)
        {
            return 0;
        }
    }
}
