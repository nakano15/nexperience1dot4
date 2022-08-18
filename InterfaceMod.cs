using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using nexperience1dot4.Interfaces;

namespace nexperience1dot4
{
    public class InterfaceMod : ModSystem
    {
        private LevelInfos li;

        public override void Load()
        {
            li = new LevelInfos("N Experience: Level Infos", null, InterfaceScaleType.UI);
        }

        public override void Unload()
        {
            li = null;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            layers.Insert(0, li);
        }
    }
}
