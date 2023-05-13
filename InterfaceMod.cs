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
        private GameModeChangeInterface ci;

        public override void Load()
        {
            li = new LevelInfos("N Experience: Level Infos", null, InterfaceScaleType.UI);
            ci = new GameModeChangeInterface("", null, InterfaceScaleType.UI);
        }

        public override void Unload()
        {
            li = null;
            ci = null;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int InventoryPosition = -1;
            for (int i = 0; i < layers.Count; i++)
            {
                switch(layers[i].Name)
                {
                    case "Vanilla: Inventory":
                        InventoryPosition = i;
                        break;
                }
            }
            layers.Insert(InventoryPosition, li);
            if (GameModeChangeInterface.GetActive) layers.Insert(InventoryPosition, ci);
        }
    }
}
