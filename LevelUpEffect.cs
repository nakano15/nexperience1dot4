using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace nexperience1dot4
{
    public class LevelUpEffect : PlayerDrawLayer
    {
        private readonly char[] LevelUpLetters = "Level Up".ToCharArray();

        public override Position GetDefaultPosition()
        {
            return new BeforeParent(PlayerDrawLayers.Head);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            PlayerMod pm = drawInfo.drawPlayer.GetModPlayer<PlayerMod>();
            int Time = pm.LevelUpEffectTime;
            if (Time < 0)
                return;
            Vector2 TextPosition = drawInfo.Center - Main.screenPosition;
            const int LettersDistanceX = 12, LettersDistanceY = 48;
            float MovementPercentage = 1f;
            float Opacity = 1f;
            if(Time < 30)
            {
                Opacity = Time * (1f / 30);
            }
            if(Time < 60)
            {
                MovementPercentage = Time * (1f / 60);
            }
            if(Time >= 120)
            {
                Opacity = 1f - (Time - 120) * (1f / 60);
            }
            for(byte l = 0; l < LevelUpLetters.Length; l++)
            {
                Vector2 LetterPosition = new Vector2(TextPosition.X + (-LevelUpLetters.Length * LettersDistanceX * 0.5f + LettersDistanceX * l) * MovementPercentage, 
                    TextPosition.Y - LettersDistanceY * MovementPercentage);
                Utils.DrawBorderStringBig(Main.spriteBatch, LevelUpLetters[l].ToString(), LetterPosition, Color.White * Opacity, 1f, 0.5f, 0.5f);
            }
        }
    }
}
