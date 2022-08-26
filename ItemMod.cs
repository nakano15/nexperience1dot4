using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;
using System;

namespace nexperience1dot4
{
    public class ItemMod : GlobalItem
    {
        public override bool OnPickup(Item item, Player player)
        {
            switch(item.type)
            {
                case ItemID.Heart:
                case ItemID.CandyApple:
                case ItemID.CandyCane:
                    int HealthRestored = (int)(player.GetModPlayer<PlayerMod>().GetHealthPercentageChange * 20) - 20;
                    if(HealthRestored > 0)
                    {
                        player.statLife += HealthRestored;
                        if(player.statLife > player.statLifeMax2)
                            player.statLife = player.statLifeMax2;
                    }
                    break;
                case ItemID.Star:
                case ItemID.SoulCake:
                case ItemID.SugarPlum:
                    int ManaRestored = (int)(player.GetModPlayer<PlayerMod>().GetManaPercentageChange * 20) - 20;
                    if(ManaRestored > 0)
                    {
                        player.statMana += ManaRestored;
                        if(player.statMana > player.statManaMax2)
                            player.statMana = player.statManaMax2;
                    }
                    break;
            }
            return base.OnPickup(item, player);
        }
    }
}