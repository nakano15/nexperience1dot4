using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;
using System;

namespace nexperience1dot4
{
    public class NpcMod : GlobalNPC
    {
        private static NPCSpawnInfo? LastSpawnInfo = null;
        private static NPC OriginNpc = null;
        public static NPC GetOriginNpc { get { return OriginNpc; } }
        public static NPCSpawnInfo? GetLastSpawnInfo { get { return LastSpawnInfo; } }

        private static int LastHealthBackup = 0, LastTypeBackup = 0;

        public override bool InstancePerEntity => true;
        public override bool IsCloneable => false;
        //public override bool CloneNewInstances => false;
        private GameModeData MobStatus;

        public GameModeData GetData { get { return MobStatus; } }

        public override void SetDefaults(NPC npc)
        {
            MobStatus = new GameModeData(nexperience1dot4.GetActiveGameModeID);
            MobStatus.SpawnNpcLevel(npc);
            MobStatus.UpdateNPC(npc);
            npc.life = npc.lifeMax;
        }

        public static int GetNpcLevel(NPC npc)
        {
            NpcMod nmod = npc.GetGlobalNPC<NpcMod>();
            if(nmod != null)
            {
                return nmod.MobStatus.GetLevel;
            }
            return 0;
        }

        public override void AI(NPC npc)
        {
            OriginNpc = npc;
            LastTypeBackup = npc.type;
            LastHealthBackup = npc.life;
        }

        public override void PostAI(NPC npc)
        {
            OriginNpc = null;
            if(npc.type != LastTypeBackup){
                npc.life = LastHealthBackup;
            }
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            LastSpawnInfo = spawnInfo;
        }

        public override void ScaleExpertStats(NPC npc, int numPlayers, float bossLifeScale)
        {
            MobStatus.UpdateNPC(npc);
        }

        public override void OnKill(NPC npc)
        {
            DistributeExp(npc);
        }

        private void DistributeExp(NPC killedNPC)
        {
            List<Player> Players = new List<Player>();
            for(byte p = 0; p < 255; p++)
            {
                if (killedNPC.playerInteraction[p])
                    Players.Add(Main.player[p]);
            }
            if (Players.Count == 0) return;
            float ExpDistribution = 1f / Players.Count + (Players.Count - 1) * 0.1f;
            float Exp = MobStatus.GetExp;
            if(killedNPC.type >= NPCID.EaterofWorldsHead && killedNPC.type <= NPCID.EaterofWorldsTail)
            {
                Exp = Microsoft.Xna.Framework.MathHelper.Max(Exp * 0.1f, 1);
            }
            foreach(Player p in Players)
            {
                PlayerMod.AddPlayerExp(p, Exp * ExpDistribution, killedNPC.getRect());
            }
        }

        public override void Unload()
        {
            LastSpawnInfo = null;
            OriginNpc = null;
        }
    }
}
