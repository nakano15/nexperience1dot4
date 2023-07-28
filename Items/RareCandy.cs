using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace nexperience1dot4.Items
{
    public class RareCandy : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Tooltip.SetDefault("You probably know what this does.");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 10;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
			Item.height = 26;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useAnimation = 15;
			Item.useTime = 15;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item3;
			Item.maxStack = 99;
			Item.consumable = true;
			Item.rare = ItemRarityID.Lime;
			Item.value = Item.buyPrice(silver: 1);
        }

        public override bool? UseItem(Player player)
        {
            PlayerMod pm = player.GetModPlayer<PlayerMod>();
            if(pm.GetCurrentGamemode.GetLevel < pm.GetCurrentGamemode.GetBase.GetMaxLevel || nexperience1dot4.InfiniteLeveling)
            {
                pm.AddExp(pm.GetCurrentGamemode.GetMaxExp);
            }
            return true;
        }

        public override bool ConsumeItem(Player player)
        {
            PlayerMod pm = player.GetModPlayer<PlayerMod>();
            return pm.GetCurrentGamemode.GetLevel < pm.GetCurrentGamemode.GetBase.GetMaxLevel|| nexperience1dot4.InfiniteLeveling;
        }
    }
}