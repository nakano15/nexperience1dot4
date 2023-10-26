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