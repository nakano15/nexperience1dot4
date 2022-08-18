using Terraria;

namespace nexperience1dot4
{
    public class BiomeLevelStruct
    {
        private string BiomeName;
        private int MinLevel, MaxLevel;
        private IsBiomeActiveDel IsBiomeActive;
        public delegate bool IsBiomeActiveDel(Terraria.Player player);

        public string GetBiomeName { get { return BiomeName; } }

        public BiomeLevelStruct(string Name, int MinLv, int MaxLv, IsBiomeActiveDel ActivateReq)
        {
            BiomeName = Name;
            MinLevel = MinLv;
            MaxLevel = MaxLv;
            IsBiomeActive = ActivateReq;
        }

        public bool CheckIfIsActive(Terraria.Player refPlayer)
        {
            return IsBiomeActive(refPlayer);
        }

        public int GetMinLevel { get { return MinLevel; } }

        public int GetMaxLevel { get { return MaxLevel; } }

        public int GetRandomLevel { get { return Main.rand.Next(MinLevel, MaxLevel + 1); } }
    }
}
