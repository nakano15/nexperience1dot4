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
using nterrautils;

namespace nexperience1dot4.Interfaces
{
    public class LevelInfosBottomButton : BottomButton
    {
        public override string Text => "Stats";
        public override int InternalWidth => 480;
        public override int InternalHeight => 220;
        public override Color TabColor => new Color (55, 74, 133);
        //Internal Values
        public static float Transparency = 1f;
        private static int LastExp = 0, LastMaxExp = 0;
        private static float LastExpPercentage = 0;
        private byte StatusRows = 6, StatusColumns = 3, PageCount = 1;
        private byte CurrentPage = 0;
        private float StatusDistance = 1;
        private int[] PointsSpent = new int[0];
        bool MouseOverButton = false;

        void GetPlayerGameModeInfos(out PlayerMod pm, out GameModeData data)
        {
            pm = Main.LocalPlayer.GetModPlayer<PlayerMod>();
            data = pm.GetCurrentGamemode;
        }

        public override void OnClickAction(bool OpeningTab)
        {
            if (OpeningTab)
            {
                GetPlayerGameModeInfos(out PlayerMod player, out GameModeData data);
                StatusColumns = (byte)(Math.Min(4, MathF.Ceiling((float)data.GetBase.GameModeStatus.Length / 6)));
                StatusRows = (byte)(MathF.Ceiling((float)data.GetBase.GameModeStatus.Length / StatusColumns));
                StatusDistance = 1f / (StatusColumns + 1);
                PointsSpent = new int[data.GetBase.GameModeStatus.Length];
                PageCount = (byte)(MathF.Ceiling((float)data.GetBase.GameModeStatus.Length / (StatusColumns * StatusRows)));
            }
        }

        public override void DrawInternal(Vector2 DrawPosition)
        {
            DrawPosition.X += InternalWidth * .5f;
            int Width = InternalWidth;
            int Height = InternalHeight;
            GetPlayerGameModeInfos(out PlayerMod player, out GameModeData data);
            string MouseOverText = "";
            string Text = data.GetBase.GetLevelInfo(data, true);
            Vector2 AcquiredScale = Utils.DrawBorderString(Main.spriteBatch, Text, DrawPosition + Vector2.UnitY * 4f, Color.White, 0.9f, 0.5f);
            Text = "Exp [" + data.GetExp + "/"  + data.GetMaxExp + "] (" + LastExpPercentage + "%)";
            AcquiredScale = Utils.DrawBorderString(Main.spriteBatch, Text, DrawPosition + Vector2.UnitY * 24f, Color.White, 0.9f, 0.5f);
            const float StatusScale = 0.8f;
            float DrawStartY = DrawPosition.Y + 50 + 8;
            GameModeStatusInfo[] status = data.GetBase.GameModeStatus;
            int StatusPointsLeft = data.GetStatusPoints;
            bool HasPointsSpent = false;
            for(int i = 0; i < PointsSpent.Length; i++){
                if(PointsSpent[i] > 0)
                    HasPointsSpent = true;
                StatusPointsLeft -= PointsSpent[i];
            }
            for(byte x = 0; x < StatusColumns; x++)
            {
                for(byte y = 0; y < StatusRows; y++)
                {
                    byte Index = (byte)(y + x * StatusRows + StatusColumns * StatusRows * CurrentPage);
                    if(Index >= status.Length)
                        break;
                    int EffectiveStatus = 0, StatusValue = data.GetStatusValue(Index, out EffectiveStatus);
                    Text = status[Index].GetShortName + " [" + StatusValue + "+" + PointsSpent[Index] + (EffectiveStatus != StatusValue ? "->" + EffectiveStatus : "") + "]";
                    Vector2 Position = new Vector2(DrawPosition.X + (Width * (-0.5f + StatusDistance * (1 + x))), DrawStartY + 20 * y);
                    AcquiredScale = Utils.DrawBorderString(Main.spriteBatch, Text, Position, Color.White, StatusScale, 0.5f);
                    if (Main.mouseX >= Position.X - AcquiredScale.X * 0.5f && Main.mouseX < Position.X + AcquiredScale.X * 0.5f && 
                        Main.mouseY >= Position.Y + 4 && Main.mouseY < Position.Y + 24)
                    {
                        MouseOverText = status[Index].GetName + "\n\"" +status[Index].GetDescription + '\"';
                    }
                    bool HasPlus = false;
                    if(StatusPointsLeft > 0){
                        HasPlus = true;
                        Position.X += AcquiredScale.X * 0.5f + 2f;
                        if(DrawButton(Position, "+", 0, 0, Scale: StatusScale))
                        {
                            PointsSpent[Index]++;
                            StatusPointsLeft --;
                        }
                    }
                    if(PointsSpent[Index] > 0){
                        if(!HasPlus)
                            Position.X += AcquiredScale.X * 0.5f + 2f;
                        else Position.X += 12;
                        if(DrawButton(Position, "-", 0, 0, Scale: StatusScale))
                        {
                            PointsSpent[Index]--;
                            StatusPointsLeft++;
                        }
                    }
                }
            }
            Utils.DrawBorderString(Main.spriteBatch, "Status Points: " + StatusPointsLeft, new Vector2(DrawPosition.X - Width * 0.25f, Main.screenHeight - 30), Color.White, 0.9f, 0.5f);
            {
                Color color = Color.White;
                Vector2 ButtonPosition = new Vector2(DrawPosition.X + Width * 0.25f, Main.screenHeight - 30);
                if (Main.mouseX >= ButtonPosition.X - 60 && Main.mouseX < ButtonPosition.X + 60 && 
                    Main.mouseY >= ButtonPosition.Y && Main.mouseY < ButtonPosition.Y + 20)
                {
                    color = Color.Yellow;
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        GameModeChangeInterface.Open();
                        Main.playerInventory = false;
                        return;
                    }
                }
                Utils.DrawBorderString(Main.spriteBatch, "Change Game Mode", ButtonPosition, color, 0.9f, 0.5f);
            }
            if(PageCount > 1)
            {
                Vector2 PagesPosition = new Vector2(DrawPosition.X + Width * 0.25f, Main.screenHeight - 30);
                AcquiredScale = Utils.DrawBorderString(Main.spriteBatch, "Page [" + (CurrentPage + 1) + "/" + (PageCount) + "]", PagesPosition, Color.White, 0.9f, 0.5f);
                if(CurrentPage > 0)
                {
                    if(DrawButton(PagesPosition - Vector2.UnitX * AcquiredScale.X * 0.5f, "<", 1, 0, 0.9f))
                    {
                        CurrentPage --;
                    }
                }
                if(CurrentPage < PageCount - 1)
                {
                    if(DrawButton(PagesPosition + Vector2.UnitX * AcquiredScale.X * 0.5f, ">", 0, 0, 0.9f))
                    {
                        CurrentPage++;
                    }
                }
            }
            if(HasPointsSpent)
            {
                if(DrawButton(new Vector2(DrawPosition.X, Main.screenHeight - 30), "Spend Points"))
                {
                    for(byte s = 0; s < PointsSpent.Length; s++)
                    {
                        int Point = PointsSpent[s];
                        data.AddStatusPoint(s, Point);
                        PointsSpent[s] = 0;
                    }
                    data.UpdateEffectiveStatus();
                    data.UpdateStatusPoints();
                    if(Main.netMode == 1)
                    {
                        NetplayMod.SendPlayerStatus(Main.myPlayer, -1, Main.myPlayer);
                    }
                }
            }
            else if(DrawButton(new Vector2(DrawPosition.X, Main.screenHeight - 30), "Respec Points"))
            {
                for(byte s = 0; s < PointsSpent.Length; s++)
                {
                    PointsSpent[s] = 0;
                }
                data.ResetStatusPoints();
                if(Main.netMode == 1)
                {
                    NetplayMod.SendPlayerStatus(Main.myPlayer, -1, Main.myPlayer);
                }
            }
            if(MouseOverText != ""){
                Utils.DrawBorderString(Main.spriteBatch, MouseOverText, new Vector2(Main.mouseX + 12, Main.mouseY + 12), Color.White, 0.9f);
            }
        }

        private static bool DrawButton(Vector2 Position, string Text, float AnchorX = 0.5f, float AnchorY = 0.5f, float Scale = 1)
        {
            Vector2 TextDim = Utils.DrawBorderString(Main.spriteBatch, Text, Position, Color.White, Scale, AnchorX, AnchorY);
            if(Main.mouseX >= Position.X - AnchorX * TextDim.X && Main.mouseX < Position.X + (1f - AnchorX) * TextDim.X && 
                Main.mouseY >= Position.Y - AnchorY * TextDim.Y && Main.mouseY < Position.Y + (1f - AnchorY) * TextDim.Y)
                {
                    Utils.DrawBorderString(Main.spriteBatch, Text, Position, Color.Yellow, Scale, AnchorX, AnchorY);
                    if(Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        return true;
                    }
                }
            return false;
        }
    }
}