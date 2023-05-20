using Terraria;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace nexperience1dot4.ModCompatibility
{
    public class TerraGuardiansMod
    {
        internal static Asset<Texture2D> ExpBarTexture;

        internal static float GroupMemberExpProgressHook(Player player, Vector2 HudPos)
        {
            PlayerMod pm = player.GetModPlayer<PlayerMod>();
            float ExpPercentage = (float)pm.GetCurrentGamemode.GetExp / pm.GetCurrentGamemode.GetMaxExp;
            const int BarSpriteWidth = 124, BarSpriteHeight = 16,
                        DistanceUntilBarStartX = 22, BarWidth = 98;
            Rectangle DrawFrame = new Rectangle(0, 0, BarSpriteWidth, BarSpriteHeight);
            Texture2D BarTexture = ExpBarTexture.Value;
            Main.spriteBatch.Draw(BarTexture, HudPos, DrawFrame, Color.White);
            HudPos.X += DistanceUntilBarStartX;
            DrawFrame.X += DistanceUntilBarStartX;
            DrawFrame.Y += BarSpriteHeight;
            DrawFrame.Width = (int)(BarWidth * ExpPercentage);
            Main.spriteBatch.Draw(BarTexture, HudPos, DrawFrame, Color.White);
            return 18;
        }

        internal static void Load()
        {
            ExpBarTexture = ModContent.Request<Texture2D>(nexperience1dot4.ContentPath + "Interface/TgXpBar");
        }

        internal static void Unload()
        {
            ExpBarTexture = null;
        }
    }
}