using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace nexperience1dot4
{
    public class BuffMod : GlobalBuff
    {
        public override void Update(int type, Player player, ref int buffIndex)
        {
            switch (type)
            {
                case BuffID.WellFed:
                    player.GetModPlayer<PlayerMod>().ExpPercentage += 0.05f;
                    break;
                case BuffID.WellFed2:
                    player.GetModPlayer<PlayerMod>().ExpPercentage += 0.1f;
                    break;
                case BuffID.WellFed3:
                    player.GetModPlayer<PlayerMod>().ExpPercentage += 0.2f;
                    break;
            }
        }
    }
}
