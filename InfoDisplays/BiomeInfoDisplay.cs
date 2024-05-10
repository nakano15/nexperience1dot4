using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;

namespace nexperience1dot4.InfoDisplays
{
    class BiomeInfoDisplay : InfoDisplay
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Area Info");
        }

        public override bool Active()
        {
            return true;
        }

        public override string DisplayValue(ref Color displayColor, ref Color displayShadowColor)/* tModPorter Suggestion: Set displayColor to InactiveInfoTextColor if your display value is "zero"/shows no valuable information */
        {
            GameModeData data = Main.LocalPlayer.GetModPlayer<PlayerMod>().GetCurrentGamemode;
            return data.GetBase.GetZoneInformation(data, Main.LocalPlayer);
        }
    }
}