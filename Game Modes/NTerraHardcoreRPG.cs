using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace nexperience1dot4.Game_Modes
{
    public class NTerraHardcoreRPG : NTerraRegularRPG
    {
        public override string Name => "Hardcore RPG Mode";
        public override string Description => "A game mode based on Regular RPG mode, but with buffed monsters stats.";

        public override void UpdateNpcStatus(NPC npc, GameModeData data)
        {
            base.UpdateNpcStatus(npc, data);
            const float Increase = .45f;
            data.NpcDamageMult += data.NpcDamageMult * Increase;
            data.NpcDefense += data.NpcDefense * Increase;
            data.ProjectileNpcDamagePercentage += data.ProjectileNpcDamagePercentage * Increase;
        }
    }
}