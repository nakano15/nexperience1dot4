using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;

namespace nexperience1dot4
{
    public class NetplayMod
    {
        public static void ReceivedMessages(System.IO.BinaryReader reader, int Me)
        {
            MessageType msgType = (MessageType)reader.ReadByte();
            switch(msgType)
            {
                case MessageType.SendPlayerLevel:
                    {
                        byte PlayerID = reader.ReadByte();
                        string GameModeID = reader.ReadString();
                        int Level = reader.ReadInt32();
                        if(PlayerID == Main.myPlayer)
                            return;
                        GameModeData gamemode = Main.player[PlayerID].GetModPlayer<PlayerMod>().GetPlayerGamemode(GameModeID);
                        if(gamemode == null) return;
                        gamemode.SetLevel(Level);
                        gamemode.ResetEffectiveLevel();
                    }
                    return;
                case MessageType.SendPlayerStatus:
                    {
                        byte PlayerID = reader.ReadByte();
                        string GameModeID = reader.ReadString();
                        byte StatusLength = reader.ReadByte();
                        int[] StatusValues = new int[StatusLength];
                        for(byte i = 0; i < StatusLength; i++)
                        {
                            StatusValues[i] = reader.ReadInt32();
                        }
                        if(PlayerID == Main.myPlayer) return;
                        GameModeData gamemode = Main.player[PlayerID].GetModPlayer<PlayerMod>().GetPlayerGamemode(GameModeID);
                        if(GameModeID == null) return;
                        for(byte i = 0; i < StatusLength; i++)
                            gamemode.ChangeStatusValue(i, StatusValues[i]);
                        gamemode.ResetEffectiveLevel();
                    }
                    return;
                    case MessageType.AskForGameMode:
                    {
                        int Player = reader.ReadByte();
                        if(Main.netMode == 2){
                            SendGameMode(Player);
                        }
                    }
                    return;
                    case MessageType.SendGameMode:
                    {
                        string GameModeID = reader.ReadString();
                        if(nexperience1dot4.ChangeActiveGameMode(GameModeID))
                        {
                            Main.NewText("Current Game Mode is '" + nexperience1dot4.GetGameMode(GameModeID).Name + "'.");
                            if(Main.netMode == 1)
                            {
                                SendPlayerLevel(Main.myPlayer, -1, Main.myPlayer);
                                SendPlayerStatus(Main.myPlayer, -1, Main.myPlayer);
                            }
                        }
                        else
                        {
                            Main.NewText("Game Mode '" + GameModeID+ "' doesn't seems to exist.", Color.Red);
                        }
                    }
                    return;

                    case MessageType.SendExpToPlayer:
                    {
                        int PlayerID = reader.ReadByte();
                        int Exp = reader.ReadInt32();
                        if(Main.netMode == 2)
                            SendExpToPlayer(PlayerID, Exp, Me);
                        else if(Main.netMode == 1)
                            PlayerMod.AddPlayerExp(Main.player[PlayerID], Exp);
                    }
                    return;

                    case MessageType.SendNpcLevel:
                    {
                        byte NpcPos = reader.ReadByte();
                        int Level = reader.ReadInt32();
                        int Health = reader.ReadInt32();
                        if(Main.npc[NpcPos].active)
                        {
                            NpcMod npcMod = Main.npc[NpcPos].GetGlobalNPC<NpcMod>();
                            npcMod.GetData.SetLevel(Level);
                            npcMod.UpdatedStatus = false;
                            //npcMod.GetData.UpdateNPC(Main.npc[NpcPos]);
                            //NpcMod.UpdateNpcStatus(Main.npc[NpcPos]);
                            Main.npc[NpcPos].life = Health;
                        }
                    }
                    return;
            }
        }

        public static void SendPlayerLevel(int PlayerID, int To = -1, int From = -1)
        {
            ModPacket packet = CreatePacket(MessageType.SendPlayerLevel);
            if(packet == null) return;
            PlayerMod pm = Main.player[PlayerID].GetModPlayer<PlayerMod>();
            packet.Write((byte)PlayerID);
            packet.Write(pm.GetCurrentGamemode.GetGameModeID);
            packet.Write(pm.GetCurrentGamemode.GetLevel);
            packet.Send(To, From);
        }

        public static void SendPlayerStatus(int PlayerID, int To = -1, int From = -1)
        {
            ModPacket packet = CreatePacket(MessageType.SendPlayerStatus);
            if(packet == null) return;
            PlayerMod pm = Main.player[PlayerID].GetModPlayer<PlayerMod>();
            packet.Write((byte)PlayerID);
            GameModeData gamemode = pm.GetCurrentGamemode;
            packet.Write(gamemode.GetGameModeID);
            GameModeStatusInfo[] si = gamemode.GetBase.GameModeStatus;
            packet.Write((byte)si.Length);
            for(byte i = 0; i < si.Length; i++)
            {
                packet.Write(gamemode.GetStatusValue(i));
            }
            packet.Send(To, From);
        }

        public static void AskForGameMode(int SendTo = -1)
        {
            ModPacket packet = CreatePacket(MessageType.AskForGameMode);
            if(packet == null) return;
            packet.Write((byte)SendTo);
            packet.Send(-1, -1);
        }

        public static void SendGameMode(int To = -1, int From = -1)
        {
            ModPacket packet = CreatePacket(MessageType.SendGameMode);
            if(packet == null) return;
            packet.Write(nexperience1dot4.GetActiveGameModeID);
            packet.Send(To, From);
        }

        public static void SendExpToPlayer(int PlayerID, int Exp, int From = -1)
        {
            ModPacket packet = CreatePacket(MessageType.SendExpToPlayer);
            if(packet == null) return;
            packet.Write((byte)PlayerID);
            packet.Write(Exp);
            packet.Send(PlayerID, From);
        }

        public static void SendNpcLevel(int Npc, int To = -1, int From = -1)
        {
            if(Npc < 0 || Npc >= 200) return;
            ModPacket packet = CreatePacket(MessageType.SendNpcLevel);
            if(packet == null) return;
            packet.Write((byte)Npc);
            packet.Write(Main.npc[Npc].GetGlobalNPC<NpcMod>().GetData.GetLevel);
            packet.Write(Main.npc[Npc].life);
            packet.Send(To, From);
        }

        private static ModPacket CreatePacket(MessageType message)
        {
            //return null; //Disabled for now.
            if(Main.netMode == 0) return null;
            ModPacket packet = nexperience1dot4.packet;
            packet.Write((byte)message);
            return packet;
        }

        public enum MessageType : byte
        {
            SendPlayerLevel,
            SendPlayerStatus,
            AskForGameMode,
            SendGameMode,
            SendExpToPlayer,
            SendNpcLevel
        }
    }
}