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

        public override void ModifyBuffText(int type, ref string buffName, ref string tip, ref int rare)
        {
            switch (type)
            {
                case BuffID.WellFed:
                    tip += "\nIncreases exp acquired by 5%.";
                    break;
                case BuffID.WellFed2:
                    tip += "\nIncreases exp acquired by 10%.";
                    break;
                case BuffID.WellFed3:
                    tip += "\nIncreases exp acquired by 20%.";
                    break;
                case BuffID.MonsterBanner:
                    tip += "\nIncreases exp acquired from monsters in the banner by 10%.";
                    break;
            }
        }
    }
}
