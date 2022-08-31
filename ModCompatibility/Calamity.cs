using Terraria;
using Terraria.ModLoader;

namespace nexperience1dot4.ModCompatibility
{
    public class Calamity
    {
        private static Mod CalamityMod;
        private const string crags = "crags",
        astral = "astral",
        sunkensea = "sunkensea",
        sulphursea = "sulphursea",
        abyss = "abyss",
        layer1 = "layer1",
        layer2 = "layer2",
        layer3 = "layer3",
        layer4 = "layer4";

        public static void Load(){
            if(ModLoader.HasMod("CalamityMod"))
            {
                CalamityMod = ModLoader.GetMod("CalamityMod");

                BiomesSetup();
                MobLevelsSetup();
            }
        }

        private static void BiomesSetup(){
            //Regular RPG
            GameModeBase RegularRPG = nexperience1dot4.GetGameMode(nexperience1dot4.RegularRPGModeID);
            RegularRPG.AddBiome("Sulphur Sea", 40, 50, delegate(Player player){
                return GetInBiome(player, sulphursea);
            });
        }

        private static void MobLevelsSetup(){

        }

        public static void Unload(){
            CalamityMod = null;
        }

        private static bool GetInBiome(Player player, string BiomeName)
        {
            return (bool)CalamityMod.Call("GetInZone", player, BiomeName);
        }
    }
}