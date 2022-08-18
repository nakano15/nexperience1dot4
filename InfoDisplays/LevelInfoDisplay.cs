using Terraria;
using Terraria.ModLoader;
using System;

namespace nexperience1dot4.InfoDisplays
{
    class LevelInfoDisplay : InfoDisplay
    {
        private int LastExp = 0, LastMaxExp = 0;
        private float LastExpPercentage = 0;

        public override void SetStaticDefaults()
        {
            InfoName.SetDefault("Level Display");
        }

        public override bool Active()
        {
            return true;
        }

        public override string DisplayValue()
        {
            GameModeData data = Main.LocalPlayer.GetModPlayer<PlayerMod>().GetCurrentGamemode;
            if(data.GetExp != LastExp || data.GetMaxExp != LastMaxExp)
            {
                LastExpPercentage = (float)Math.Round((float)data.GetExp * 100 / data.GetMaxExp, 2);
                LastExp = data.GetExp;
                LastMaxExp = data.GetMaxExp;
            }
            return "Level [" + data.GetLevel + (data.GetEffectiveLevel != data.GetLevel ? "->" + data.GetEffectiveLevel : "") + "] Exp: [" + LastExpPercentage + "%]";
        }
    }
}
