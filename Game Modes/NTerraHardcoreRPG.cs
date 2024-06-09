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
            const float Increase = .2f; //.45f
            //data.NpcDamageMult += data.NpcDamageMult * Increase;
            data.NpcDamageMult += data.GetEffectiveLevel * Increase;
            data.NpcDefense += npc.defense * Increase;
            data.ProjectileNpcDamagePercentage += data.GetEffectiveLevel * Increase;
            if(npc.lifeMax > 5)
            {
                data.NpcHealthMult += data.GetEffectiveLevel * 0.3f;
                //npc.lifeMax += (int)(npc.lifeMax * Level * 0.1f);
            }
        }
    }
}