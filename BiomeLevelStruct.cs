using Terraria;

namespace nexperience1dot4
{
    public class BiomeLevelStruct
    {
        private string BiomeName;
        private int MinLevel, MaxLevel;
        private int NightMinLevel, NightMaxLevel;
        private System.Func<Player, bool> IsBiomeActive;
        
        public string GetBiomeName { get { return BiomeName; } }

        public BiomeLevelStruct(string Name, int MinLv, int MaxLv, System.Func<Player, bool> ActivateReq, int NightMinLv = -1, int NightMaxLv = -1)
        {
            BiomeName = Name;
            MinLevel = MinLv;
            MaxLevel = MaxLv;
            NightMinLevel = NightMinLv;
            NightMaxLevel = NightMaxLv;
            IsBiomeActive = ActivateReq;
        }

        public bool CheckIfIsActive(Terraria.Player refPlayer)
        {
            return IsBiomeActive(refPlayer);
        }

        public int GetMinLevel { get { if(!Main.dayTime && NightMinLevel > -1) return NightMinLevel; return MinLevel; } }

        public int GetMaxLevel { get { if(!Main.dayTime && NightMaxLevel > -1) return NightMaxLevel; return MaxLevel; } }

        public int GetRandomLevel { get { return Main.rand.Next(GetMinLevel, GetMaxLevel + 1); } }
    }
}
