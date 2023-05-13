using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace nexperience1dot4.Interfaces
{
    public class GameModeChangeInterface : LegacyGameInterfaceLayer
    {
        public static bool GetActive { get { return IsActive; } }
        private static bool IsActive = false;
        private static Vector2 Position = Vector2.Zero;
        private static Point Dimension = Point.Zero;
        private static int Selected = -1, MenuScroll = 0;
        private static GameModeBase SelectedGameMode = null;
        private static string[] GameModeIDs = null;
        private static string[] GameModeNames = null;
        private static string[] Description = null;
        private static int MaxLines = 0;

        public GameModeChangeInterface(string name, GameInterfaceDrawMethod drawMethod, InterfaceScaleType scaleType = InterfaceScaleType.UI) : base(name, DrawInterface, scaleType)
        {
            name = "NExperience : Game Mode Change UI";
        }

        public static void Open()
        {
            Dimension = new Point((int)(Main.screenWidth * 0.5f), (int)(Main.screenHeight * 0.5f));
            Position.X = Main.screenWidth * 0.5f - Dimension.X * 0.5f;
            Position.Y = Main.screenHeight * 0.5f - Dimension.Y * 0.5f;
            IsActive = true;
            Selected = -1;
            MenuScroll = 0;
            GameModeIDs = nexperience1dot4.GetGameModeIDs;
            GameModeNames = new string[GameModeIDs.Length];
            for(int i = 0; i < GameModeIDs.Length; i++)
            {
                GameModeNames[i] = nexperience1dot4.GetGameMode(GameModeIDs[i]).Name;
            }
        }

        public static void Close()
        {
            SelectedGameMode = null;
            GameModeIDs = null;
            IsActive = false;
            Description = null;
        }
        
        public static bool DrawInterface()
        {
            if (Main.playerInventory)
            {
                Close();
                return true;
            }
            Vector2 DrawPosition = Position;
            if (Main.mouseX >= DrawPosition.X && Main.mouseX < DrawPosition.X + Dimension.X && 
                Main.mouseY >= DrawPosition.Y && Main.mouseY < DrawPosition.Y + Dimension.Y)
            {
                Main.LocalPlayer.mouseInterface = true;
            }
            Main.spriteBatch.Draw(nexperience1dot4.HandyTexture.Value, new Rectangle((int)DrawPosition.X - 2, (int)DrawPosition.Y - 2, Dimension.X + 4, Dimension.Y + 4), Color.White);
            Main.spriteBatch.Draw(nexperience1dot4.HandyTexture.Value, new Rectangle((int)DrawPosition.X, (int)DrawPosition.Y, Dimension.X, Dimension.Y), Color.Gray);
            DrawPosition.X += 4;
            DrawPosition.Y += 4;
            int MaxItems = (int)((Dimension.Y - 8) / 30);
            float ListWidth = 258f;
            for(int i = 0; i < MaxItems; i++)
            {
                int Index = i + MenuScroll;
                byte OptionType = 0;
                if (Index >= GameModeIDs.Length) break;
                if (i == 0 && Index > 0)
                {
                    OptionType = 1;
                }
                else if (i == MaxItems - 1 && Index + MaxItems < GameModeIDs.Length)
                {
                    OptionType = 2;
                }
                string Text = "";
                string GameModeID = GameModeIDs[Index];
                switch(OptionType)
                {
                    case 1:
                        Text = "= Up =";
                        break;
                    case 2:
                        Text = "= Down =";
                        break;
                    default:
                        Text = GameModeNames[Index];
                        break;
                }
                Vector2 ItemPosition = DrawPosition;
                ItemPosition.Y += i * 30;
                Color color = Selected == Index ? Color.Cyan : Color.White;
                if (Main.mouseX >= ItemPosition.X && Main.mouseX < ItemPosition.X + ListWidth && 
                    Main.mouseY >= ItemPosition.Y && Main.mouseY < ItemPosition.Y + 30)
                {
                    color = Color.Yellow;
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        switch(OptionType)
                        {
                            default:
                                Selected = Index;
                                SelectedGameMode = nexperience1dot4.GetGameMode(GameModeIDs[Index]);
                                Description = Utils.WordwrapString(SelectedGameMode.Description, Terraria.GameContent.FontAssets.MouseText.Value, Dimension.X - (int)ListWidth - 8, 10, out MaxLines);
                                break;
                            case 1:
                                MenuScroll--;
                                break;
                            case 2:
                                MenuScroll++;
                                break;
                        }
                    }
                }
                Utils.DrawBorderString(Main.spriteBatch, Text, ItemPosition, color);
            }
            DrawPosition.X += ListWidth + 4;
            if(Selected > -1)
            {
                DrawPosition.Y += Utils.DrawBorderString(Main.spriteBatch, SelectedGameMode.Name, DrawPosition, Color.White, 1.2f).Y;
                for(int i = 0; i <= MaxLines; i++)
                {
                    Utils.DrawBorderString(Main.spriteBatch, Description[i], DrawPosition, Color.White);
                    DrawPosition.Y += 30;
                }
                Utils.DrawBorderString(Main.spriteBatch, "Max Level: " + SelectedGameMode.GetMaxLevel, DrawPosition, Color.White);
                DrawPosition.Y += 30;
                Utils.DrawBorderString(Main.spriteBatch, "Allows Level Capping? " + SelectedGameMode.EnableLevelCapping, DrawPosition, Color.White);
                DrawPosition.Y += 30;
                if (Main.GameMode != Selected && Main.netMode == 0)
                {
                    DrawPosition.X = Position.X + ListWidth + (Dimension.X - ListWidth + 8) * 0.5f;
                    DrawPosition.Y = Position.Y + Dimension.Y - 40;
                    Color color = Color.White;
                    if (Main.mouseX >= DrawPosition.X - 80 && Main.mouseX < DrawPosition.X + 80 && 
                        Main.mouseY >= DrawPosition.Y - 15 && Main.mouseY < DrawPosition.Y + 10)
                    {
                        color = Color.Yellow;
                        if (Main.mouseLeft && Main.mouseLeftRelease)
                        {
                            bool CanChangeGameMode = true;
                            for(int i = 0; i < 200; i++)
                            {
                                if (Main.npc[i].active && Terraria.ID.NPCID.Sets.ShouldBeCountedAsBoss[i])
                                {
                                    CanChangeGameMode = false;
                                    break;
                                }
                            }
                            if (!CanChangeGameMode)
                            {
                                Main.NewText("Finish your fight before changing game mode!");
                                Close();
                                return true;
                            }
                            nexperience1dot4.ChangeActiveGameMode(GameModeIDs[Selected]);
                            Close();
                            return true;
                        }
                    }
                    Utils.DrawBorderString(Main.spriteBatch, "Change Game Mode", DrawPosition, color, 1f, 0.5f, 0.5f);
                }

            }
            DrawPosition.X = Position.X + Dimension.X - 64;
            DrawPosition.Y = Position.Y + 4;
            Color closecolor = Color.Red;
            if (Main.mouseX >= DrawPosition.X && Main.mouseX < DrawPosition.X + 60 && 
                Main.mouseY >= DrawPosition.Y && Main.mouseY < DrawPosition.Y + 26f)
            {
                closecolor = Color.Yellow;
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Close();
                    return true;
                }
            }
            Utils.DrawBorderString(Main.spriteBatch, "Close", DrawPosition, closecolor, 0.9f);
            return true;
        }
    }
}