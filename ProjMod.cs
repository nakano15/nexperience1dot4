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
        }

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            NPC npc = NpcMod.GetOriginNpc;
            if(npc != null && projectile.damage != npc.damage)
            {
                projectile.damage = (int)(projectile.damage * NpcMod.GetNpcProjectileDamage(npc));
            }
        }

        public override void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers)
        {
            switch(projectile.type){
                case ProjectileID.TorchGod:
                    modifiers.FinalDamage = modifiers.FinalDamage * target.GetModPlayer<PlayerMod>().GetHealthPercentageChange;
                    break;
            }
        }
    }
}