using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;
using System;
using Terraria.DataStructures;

namespace nexperience1dot4
{
    public class ProjMod : GlobalProjectile
    {
        public override void SetDefaults(Projectile projectile)
        {
            NPC npc = NpcMod.GetOriginNpc;
            if(npc != null && projectile.npcProj && projectile.damage != npc.damage)
            {
                projectile.damage = (int)(projectile.damage * NpcMod.GetNpcProjectileDamage(npc));
            }
        }
    }
}