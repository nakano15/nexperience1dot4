using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using System;
using Terraria.Localization;
using System.IO;
using Terraria.UI;

namespace nexperience1dot4
{
    public class SystemMod : ModSystem
    {
        public override void NetSend(BinaryWriter writer)
        {
            
        }

        public override void NetReceive(BinaryReader reader)
        {
            
        }

        public override void PreUpdateEntities()
        {
            UpdateWeekendExpCheck();
        }

        internal static void UpdateWeekendExpCheck()
        {
            DateTime date = DateTime.Now;
            bool LastActive = nexperience1dot4.WeekendExp;
            nexperience1dot4.WeekendExp = date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
            if (LastActive != nexperience1dot4.WeekendExp)
            {
                if (nexperience1dot4.WeekendExp)
                {
                    Main.NewText("Weekend Experience Boost active! Enjoy your weekend.");
                }
                else
                {
                    Main.NewText("Weekend Experience Boost disabled. See you next week.");
                }
            }
        }

        public override void PostUpdateNPCs()
        {
            NpcMod.TransformTrap = 255;
            NpcMod.GetOriginNpc = null;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            
        }
    }
}