#region License
// ====================================================
// AsyncServerClient Copyright(C) 2015-2019 Furkan Türkal
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using ServerFramework;
using System;

namespace ServerConsole.PacketProcessing
{
    public sealed class Message : IPacketReceiver
    {
        public object m_Locker = new object();

        public void OnCommandReceived(Packet packet)
        {
            lock (m_Locker)
            {
                if (packet.Opcode == 0x1235)
                {
                    string message = packet.ReadAscii();

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Message: {0}", message);
                    Console.ResetColor();
                    Packet samplePacket = new Packet(0x1234);
                    samplePacket.WriteAscii("Message CLASS : We are get this message : " + message);
                    GatewayServer.m_Client.SendPacket(samplePacket);
                }
            }
        }

    }
}
