using Terraria;
using Terraria.ModLoader;
using System;

namespace nexperience1dot4.InfoDisplays
{
    class BiomeInfoDisplay : InfoDisplay
    {
        public override void SetStaticDefaults()
        {
            InfoName.SetDefault("Area Info");
        }

        public override bool Active()
        {
            return true;
        }

        public override string DisplayValue()
        {
            BiomeLevelStruct biome = Main.LocalPlayer.GetModPlayer<PlayerMod>().GetCurrentGamemode.GetMyBiome;
            if(biome == null)
                return "No Information";
            return biome.GetBiomeName + " Lv[" + biome.GetMinLevel + "~" + biome.GetMaxLevel + "]";
        }
    }
}