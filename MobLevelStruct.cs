using Terraria;
using System;
using System.Collections.Generic;

namespace nexperience1dot4
{
    public class MobLevelStruct
    {
        private List<LevelSetup> Levels = new List<LevelSetup>();
        public struct LevelSetup{
            public int Level;
            public Func<bool> CanTakeLevel;

            public LevelSetup(int NewLevel, Func<bool> NewRequirement)
            {
                Level = NewLevel;
                CanTakeLevel = NewRequirement;
            }
        }

        public int TakeLevel()
        {
            int HighestLevelTaken = 0;
            foreach(LevelSetup level in Levels)
            {
                if(level.Level > HighestLevelTaken && level.CanTakeLevel())
                {
                    HighestLevelTaken = level.Level;
                }
            }
            return HighestLevelTaken;
        }

        public void AddMobLevel(int Level, Func<bool> Requirement = null)
        {
            Levels.Add(new LevelSetup(Level, Requirement == null ? DefaultTrue : Requirement));
        }

        private static bool DefaultTrue(){ return true;}
    }
}
