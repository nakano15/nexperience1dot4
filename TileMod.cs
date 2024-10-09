using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;
using System;

namespace nexperience1dot4
{
    public class TileMod : GlobalTile
    {
        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (fail || Main.gameMenu || !nexperience1dot4.EnableExpGainFromBreakingTiles) return;
            Player Nearest = nterrautils.MainMod.GetPlayerCharacter();
            if (Main.netMode > 0)
            {
                float NearestDistance = float.MaxValue;
                Player NearestFoundPlayer = null;
                for (int p = 0; p < 255; p++)
                {
                    if (Main.player[p].active && !Main.player[p].dead)
                    {
                        float Distance = Nearest.Distance(Main.player[p].position);
                        if (Distance < NearestDistance)
                        {
                            NearestFoundPlayer = Nearest;
                            NearestDistance = Distance;
                        }
                    }
                }
                if (NearestFoundPlayer != null)
                {
                    Nearest = NearestFoundPlayer;
                }
            }
            foreach (Player p in PlayerMod.GetPlayerTeammates(Nearest))
            {
                PlayerMod pm = p.GetModPlayer<PlayerMod>();
                int Exp = pm.GetCurrentGamemode.GetBase.GetTileBreakingExp(type);
                if (Exp > 0)
                    pm.AddExp(Exp);
            }
        }
    }
}