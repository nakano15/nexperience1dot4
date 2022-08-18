using Terraria;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace nexperience1dot4
{
    public class ServerConfigMod : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("Gameplay Settings")]

        [Label("Death Exp Penalty Percentage")]
        [Tooltip("Allows you to change the exp penalty upon death.")]
        [DefaultValue(5)]
        [Range(0, 100)]
        public byte DeathExpPenalty;

        [Label("Enable Biome Level Capper?")]
        [Tooltip("When enabled, your character level will be capped to\nbiome maximum level, if overleveled to it.\nHaving a higher leveled monster around\nwill increase the level capper.\nYou can still gain exp and level\nup while under effect of the level capper.")]
        [DefaultValue(true)]
        public bool EnableBiomeLevelCapper;

        [Label("Infinite Leveling?")]
        [Tooltip("Enabling this, will make so you can level up past level cap.\nThe mod will try to make up the exp progression past max level.")]
        [DefaultValue(false)]
        public bool InfiniteLeveling;

        [Label("Exp Rate")]
        [Tooltip("Allows you to change the rate at which your character gains exp.\nValue is in percentage.")]
        [DefaultValue(100)]
        [Range(0, 1600)]
        public int ExpRate;

        [Label("Status Window Transparency")]
        [Tooltip("This will change the opacity of the status window.")]
        [DefaultValue(1)]
        [Range(0, 1f)]
        public float StatusWindowOpacity;

        public override void OnLoaded()
        {
            /*DeathExpPenalty = nexperience1dot4.DeathExpPenalty;
            EnableBiomeLevelCapper = nexperience1dot4.EnableBiomeLevelCapper;
            InfiniteLeveling = nexperience1dot4.InfiniteLeveling;
            ExpRate = (int)(nexperience1dot4.ExpRate * 100);*/
        }

        public override void OnChanged()
        {
            nexperience1dot4.DeathExpPenalty = DeathExpPenalty;
            nexperience1dot4.EnableBiomeLevelCapper = EnableBiomeLevelCapper;
            nexperience1dot4.InfiniteLeveling = InfiniteLeveling;
            nexperience1dot4.ExpRate = ExpRate * 0.01f;
            Interfaces.LevelInfos.Transparency = StatusWindowOpacity;
        }
    }
}
