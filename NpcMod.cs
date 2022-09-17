using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
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

        private int OriginalHP = 100;
        public int GetOriginalHP{get{return OriginalHP;}}
        private BitsByte UpdateInfos = new BitsByte();
        private bool FirstUpdate { get{ return UpdateInfos[0]; } set{ UpdateInfos[0] = value; }}
        private bool UpdatedStatus { get{ return UpdateInfos[1]; } set{ UpdateInfos[1] = value; }}

        public override bool InstancePerEntity => true;
        public override bool IsCloneable => false;
        //public override bool CloneNewInstances => false;
        private GameModeData MobStatus;

        public GameModeData GetData { get { return MobStatus; } }
        public static int LastLoggedMonsterLevel = 0;

        public override void ModifyGlobalLoot(GlobalLoot globalLoot)
        {
            globalLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.RareCandy>(), 10000));
        }

        public static float GetNpcProjectileDamage(NPC npc)
        {
            return npc.GetGlobalNPC<NpcMod>().MobStatus.ProjectileNpcDamagePercentage;
        }

        internal static void UpdateNpcStatus(NPC n)
        {
            if(!n.active) return;
            NpcMod npc = n.GetGlobalNPC<NpcMod>();
            npc.MobStatus.ChangeGameMode(nexperience1dot4.GetActiveGameModeID);
            npc.MobStatus.SpawnNpcLevel(n);
            npc.MobStatus.UpdateNPC(n);
        }

        public override void SetDefaults(NPC npc)
        {
            if(MobStatus == null)
                MobStatus = new GameModeData(nexperience1dot4.GetActiveGameModeID);
            //else MobStatus.ChangeGameMode(nexperience1dot4.GetActiveGameModeID);
            MobStatus.SpawnNpcLevel(npc);
            UpdatedStatus = false;
            FirstUpdate = true;
            OriginalHP = npc.lifeMax;
        }
        
        public override void OnSpawn(NPC npc, Terraria.DataStructures.IEntitySource source)
        {
        }

        public override void ScaleExpertStats(NPC npc, int numPlayers, float bossLifeScale)
        {
            //MobStatus.UpdateNPC(npc);
            //OriginalHP = npc.lifeMax;
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

        public override bool PreAI(NPC npc)
        {
            if(!UpdatedStatus)
            {
                OriginalHP = npc.lifeMax;
                UpdatedStatus = true;
                float Percentage = npc.life >= npc.lifeMax ? 1f : (float)npc.life / npc.lifeMax;
                MobStatus.UpdateNPC(npc);
                npc.life = (int)(npc.lifeMax * Percentage);
            }
            if(FirstUpdate)
            {
                FirstUpdate = false;
                NetplayMod.SendNpcLevel(npc.whoAmI, -1, Main.myPlayer);
            }
            OriginNpc = npc;
            LastLoggedMonsterLevel = MobStatus.GetLevel;
            return base.PreAI(npc);
        }

        public override void PostAI(NPC npc)
        {
            LastLoggedMonsterLevel = 0;
            OriginNpc = null;
            MobStatus.UpdateNPC(npc);
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            LastSpawnInfo = spawnInfo;
        }

        public override void OnKill(NPC npc)
        {
            if(nexperience1dot4.ZombiesDroppingTombstones)
                TombstoneGenerator(npc, Main.LocalPlayer.whoAmI);
            DistributeExp(npc);
        }

        private void DistributeExp(NPC killedNPC)
        {
            List<Player> Players = new List<Player>();
            for(byte p = 0; p < 255; p++)
            {
                if (killedNPC.playerInteraction[p])
                {
                    Player player = Main.player[p];
                    foreach(Player other in PlayerMod.GetPlayerTeammates(player))
                    {
                        if(!Players.Contains(other))
                            Players.Add(other);
                    }
                }
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
                float ThisExpValue = Exp * ExpDistribution;
                float ExpIncrease = 0;
                if(p.HasNPCBannerBuff(Item.NPCtoBanner(killedNPC.BannerID())))
                {
                    ExpIncrease += 0.1f;
                }
                PlayerMod.AddPlayerExp(p, ThisExpValue, killedNPC.getRect(), ExpIncrease);
            }
        }

        public override void Unload()
        {
            LastSpawnInfo = null;
            OriginNpc = null;
        }

        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if(nexperience1dot4.NTerrariaGraveyard && player.ZoneGraveyard && !Main.dayTime)
            {
                spawnRate = (int)(spawnRate * 0.5f);
                maxSpawns *= 2;
            }
        }

        public void TombstoneGenerator(NPC npc, int plr)
        {
            bool CreateTombstone = false;
            switch (npc.type)
            {
                case 3:
                case 132:
                case 161:
                case 186:
                case 187:
                case 188:
                case 189:
                case 200:
                case 223:
                case 254:
                case 255:
                case 319:
                case 320:
                case 321:
                    CreateTombstone = true;
                    break;
            }
            if (Main.netMode != 1 && CreateTombstone && plr != -1 && !Main.player[plr].ZoneGraveyard && Main.rand.Next(5) == 0)
            {
                float num = (float)Main.rand.Next(-35, 35 + 1) * 0.1f;
                int num2 = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.position.X + npc.width * 0.5f, npc.position.Y, (float)Main.rand.Next(10, 30) * 0.1f * (float)npc.direction + num, (float)Main.rand.Next(-40, -20) * 0.1f, 43, 0, 0f, Main.myPlayer);
                switch (Main.rand.Next(75))
                {
                    case 0:
                        Main.projectile[num2].miscText = "A zombie perished here, better remove this tombstone soon.";
                        break;
                    case 1:
                        Main.projectile[num2].miscText = "The following text has been received by fax and instantly written here.";
                        break;
                    case 2:
                        Main.projectile[num2].miscText = "Still dead.";
                        break;
                    case 3:
                        Main.projectile[num2].miscText = "\"This can't get worse....\", he were wrong.";
                        break;
                    case 4:
                        Main.projectile[num2].miscText = "\"Look,the wall of flesh!!!\" was his last words.";
                        break;
                    case 5:
                        Main.projectile[num2].miscText = "\"Don't push pin of the grenade, or we will...\" were said before the boom.";
                        break;
                    case 6:
                        Main.projectile[num2].miscText = "She learned that no one can use a slime as a cushion.";
                        break;
                    case 7:
                        Main.projectile[num2].miscText = "\"A button! I'll press it.\" made him turn into more than 150 pieces.";
                        break;
                    case 8:
                        Main.projectile[num2].miscText = "\"You will die today Zombie!!!\" he said before getting his own tombstone.";
                        break;
                    case 9:
                        Main.projectile[num2].miscText = "\"Eye of Cthulhu is easy.\" she said. \"AAAAAAHHHHHH!!!\" she also said.";
                        break;
                    case 10:
                        Main.projectile[num2].miscText = "His charity act made Eater of Worlds lose " + Main.rand.Next(10, 95 + 1) + "% of hunger.";
                        break;
                    case 11:
                        Main.projectile[num2].miscText = "She tried to pet a unicorn.";
                        break;
                    case 12:
                        Main.projectile[num2].miscText = "\"I will not listen what a Crazy Old Man have to say\", you should have listened.";
                        break;
                    case 13:
                        Main.projectile[num2].miscText = "We found " + Main.rand.Next(10) + "/10 pieces of "+(Main.rand.Next(2) == 0 ? "him" : "her")+". If you find the other pieces, please call me.";
                        break;
                    case 14:
                        Main.projectile[num2].miscText = "He learned that can't fight against the Wall of Flesh using a Tyrfing.";
                        break;
                    case 15:
                        Main.projectile[num2].miscText = "CoolShadow should have used his coins on insurance...";
                        break;
                    case 16:
                        Main.projectile[num2].miscText = "robflop couldn't slap faster...";
                        break;
                    case 17:
                        Main.projectile[num2].miscText = "Nakano found the chest, but not that pressure plate.";
                        break;
                    case 18:
                        Main.projectile[num2].miscText = "She thought it was a Spider super hero going to save her from the Crimson.";
                        break;
                    case 19:
                        Main.projectile[num2].miscText = "He needed a hug, a Zombie provided that.";
                        break;
                    case 20:
                        Main.projectile[num2].miscText = "This adventurer discovered Face Monster's breath.";
                        break;
                    case 21:
                        Main.projectile[num2].miscText = "He asked for brains, and got bullets instead.";
                        break;
                    case 22:
                        Main.projectile[num2].miscText = "He didn't counted on Brain of Cthulhu's cleverness.";
                        break;
                    case 23:
                        Main.projectile[num2].miscText = "She turned into Plantera's gum.";
                        break;
                    case 24:
                        Main.projectile[num2].miscText = "Everybody knows the cake is a lie, why he tried to eat it?";
                        break;
                    case 25:
                        Main.projectile[num2].miscText = "Do not do the Victory Dance when you beat Eye of Cthulhu's first part.";
                        break;
                    case 26:
                        Main.projectile[num2].miscText = "This one got stinged by Queen Bee. Guess where.";
                        break;
                    case 27:
                        Main.projectile[num2].miscText = "Feeling fat? Do like her! Use Golem's stomp as solution to that problem.";
                        break;
                    case 28:
                        Main.projectile[num2].miscText = "The more, the better.... For THEM.";
                        break;
                    case 29:
                        Main.projectile[num2].miscText = "He thought they were dead. But they were not.";
                        break;
                    case 30:
                        Main.projectile[num2].miscText = "\"I don't need potions!\" he said before fighting Plantera.";
                        break;
                    case 31:
                        Main.projectile[num2].miscText = "\"Let's move Plantera to the surface, It will be easier to kill.\" she said before knowing that things changed.";
                        break;
                    case 32:
                        Main.projectile[num2].miscText = "\"I have enough time to kill skeletron prime.\" he said before dawn.";
                        break;
                    case 33:
                        Main.projectile[num2].miscText = "They though he could defeat the Dungeon Guardian with a Copper Shortsword.";
                        break;
                    case 34:
                        Main.projectile[num2].miscText = "He tried to be like Yrimir.";
                        break;
                    case 35:
                        Main.projectile[num2].miscText = "Poor guy. He forgot the laws of physics.";
                        break;
                    case 36:
                        Main.projectile[num2].miscText = "The Groom now got a Bride.";
                        break;
                    case 37:
                        Main.projectile[num2].miscText = "Self confident Terrarian VS Crimera, who won?";
                        break;
                    case 38:
                        Main.projectile[num2].miscText = "Too bad she will not be able to reveal Murasame's secret...";
                        break;
                    case 39:
                        Main.projectile[num2].miscText = "The fight against the Dragon ended up on 1x0... To the Dragon.";
                        break;
                    case 40:
                        Main.projectile[num2].miscText = "He tried to explore the castle... They tried to tell him to not do that...";
                        break;
                    case 41:
                        Main.projectile[num2].miscText = "Is this tombstone really necessary?";
                        break;
                    case 42:
                        Main.projectile[num2].miscText = "The Red Beast avenged his friend's death.";
                        break;
                    case 43:
                        Main.projectile[num2].miscText = "This one died because tried to ride a Unicorn.";
                        break;
                    case 44:
                        Main.projectile[num2].miscText = "She thought that there were one, on truth, there were two.";
                        break;
                    case 45:
                        Main.projectile[num2].miscText = "Someone caused this.";
                        break;
                    case 46:
                        Main.projectile[num2].miscText = "Yeah... He had to say \"Dead Branch is useless, i'll toss this away.\" when were near the water.";
                        break;
                    case 47:
                        Main.projectile[num2].miscText = "A Solar Eclipse made this party more interesting.";
                        break;
                    case 48:
                        Main.projectile[num2].miscText = "Someone once told me to not leave the character afk while equipping a Ocean Shield. I should have heard her.";
                        break;
                    case 49:
                        Main.projectile[num2].miscText = "\"Fishing is not dangerous.\", he said before discovering the Truffle Worm's usefullness.";
                        break;
                    case 50:
                        Main.projectile[num2].miscText = "She discovered that the cool realm is not so cool at all.";
                        break;
                    case 51:
                        {
                            string s = "there is a Zombie wanting your brain";
                            switch (Main.rand.Next(11))
                            {
                                case 0:
                                    s = "there are " + (WorldGen.crimson ? "Crimera" : "Eater of Souls") + " wanting your " + (WorldGen.crimson ? "blood" : "soul");
                                    break;
                                case 1:
                                    s = "a Terrarian digging a hellevator";
                                    break;
                                case 2:
                                    s = "the Wall of Flesh is doing a hell ride";
                                    break;
                                case 3:
                                    s = "a Slime devouring a Bunny";
                                    break;
                                case 4:
                                    s = "someone is falling off a cliff";
                                    break;
                                case 5:
                                    s = "a Dungeon Guardian is chasing a Terrarian";
                                    break;
                                case 6:
                                    s = "the Groom is on a marriage";
                                    break;
                                case 7:
                                    s = "the Guide is derping around";
                                    break;
                                case 8:
                                    s = "the Goblins are playing poker";
                                    break;
                                case 9:
                                    s = "the Terraria Developers finished working on Terraria version 1.4, and that's about right";
                                    break;
                                case 10:
                                    s = "Nakano15 is probably working on some cool things";
                                    break;
                            }
                            Main.projectile[num2].miscText = "While you are reading this, " + s + ".";
                        }
                        break;
                    case 52:
                        Main.projectile[num2].miscText = "Well... At least she got a free ride on Wyvern Airlines...";
                        break;
                    case 53:
                        Main.projectile[num2].miscText = "Keep thinking that landmines does not works.";
                        break;
                    case 54:
                        Main.projectile[num2].miscText = "And the luck number of this night is.... " + Main.rand.Next(100, 1000) + "!\nGood luck redeeming the prize.";
                        break;
                    case 55:
                        Main.projectile[num2].miscText = "This is what MashiroSora got because her summon upgrade were on the Underworld.";
                        break;
                    case 56:
                        Main.projectile[num2].miscText = "SonicPivot fell on the trolling that his internet were going to fall, and tossed his hardcore character inside the pre-skeletron Dungeon.";
                        break;
                    case 57:
                        Main.projectile[num2].miscText = "This one got tired of playing with Skulliton.";
                        break;
                    case 58:
                        Main.projectile[num2].miscText = "Here will lie " + Main.player[plr].name + ", ";
                        switch(Main.rand.Next(4))
                        {
                            case 0:
                                Main.projectile[num2].miscText += "if does not watch out for traps.";
                                break;
                            case 1:
                                Main.projectile[num2].miscText += "if wont stop being hugged by zombies.";
                                break;
                            case 2:
                                Main.projectile[num2].miscText += "if get touched by slimes.";
                                break;
                            case 3:
                                Main.projectile[num2].miscText += "if afk grind.";
                                break;
                        }
                        break;
                    case 59:
                        Main.projectile[num2].miscText = "This one should not spam potions during boss fights. This isn't Terraria 1.0 anymore.";
                        break;
                    case 60:
                        Main.projectile[num2].miscText = "I thought it would be funny playing soccer with a rolling cacti.";
                        break;
                    case 61:
                        Main.projectile[num2].miscText = "It's no good having a Bunny Avenger for Easter.";
                        break;
                    case 62:
                        Main.projectile[num2].miscText = "She were too close to level up...";
                        break;
                    case 63:
                        Main.projectile[num2].miscText = "His obsidian generation machine broke.";
                        break;
                    case 64:
                        Main.projectile[num2].miscText = "I does not know what to say about this person, I only remember of yelling at him before running wild in a cavern.";
                        break;
                    case 65:
                        Main.projectile[num2].miscText = (Main.rand.Next(2) == 0 ? "He" : "She") + " did not survive N Terraria mod bugs...";
                        break;
                    case 66:
                        Main.projectile[num2].miscText = "The last thing I told to him, was \"Do not recklessly charge on the boss.\".";
                        break;
                    case 67:
                        Main.projectile[num2].miscText = "She were playing with the Dungeon Shackles, but didn't expected it to lock itself.";
                        break;
                    case 68:
                        Main.projectile[num2].miscText = "His Lavender Tower structure got way too realist.";
                        break;
                    case 69:
                        Main.projectile[num2].miscText = "Nakano15 should not have tried to organize the inventory while under water..";
                        break;
                    case 70:
                        Main.projectile[num2].miscText = "\"We can escape the Wall of Flesh by using the Magic Mirror!\", now you need to find his friends tombstones.";
                        break;
                    case 71:
                        Main.projectile[num2].miscText = "\"We'll be alright if it doesn't use the laser.\", then the Moon Lord used the laser.";
                        break;
                    case 72:
                        Main.projectile[num2].miscText = "She said \"I thought it was just a stone.\" before being smashed by a boulder.";
                        break;
                    case 73:
                        Main.projectile[num2].miscText = "\"I'm using Cloud in a Bottle. I can break my fall before reach the floor.\", but he didn't counted on his bad timing.";
                        break;
                    default:
                        Main.projectile[num2].miscText = "A dead person lies here.";
                        break;
                }
            }
        }
    }
}
