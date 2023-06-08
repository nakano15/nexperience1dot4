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
            GameModeData data = Main.LocalPlayer.GetModPlayer<PlayerMod>().GetCurrentGamemode;
            return data.GetBase.GetZoneInformation(data, Main.LocalPlayer);
        }
    }
}