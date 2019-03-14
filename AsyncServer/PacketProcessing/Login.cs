#region License
// ====================================================
// AsyncServerClient Copyright(C) 2015-2019 Furkan Türkal
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using ServerFramework;

namespace ServerConsole.PacketProcessing
{
    public sealed class Login : IPacketReceiver
    {
        private object m_Locker = new object();

        public void OnCommandReceived(Packet packet)
        {
            lock (m_Locker)
            {
                if (packet.Opcode == 0x2001)
                {
                    string serviceName = packet.ReadAscii();
                    if (serviceName == "AsyncServerClient")
                    {
                        Packet samplePacket = new Packet(0x2100);
                        samplePacket.WriteInt32(0707);
                        samplePacket.WriteAscii("Hello, Client! This is simple Login packet. Your ID = 0x0707");
                        GatewayServer.m_Client.SendPacket(samplePacket);
                    }
                } else if (packet.Opcode == 0x2101) {
                    Packet samplePacket = new Packet(0x2102);
                    samplePacket.WriteAscii("You can send your private identitiy as encrpyted. I can understand your encrypted packet because we did a three-way TCP handshake using Blowfish.");
                    GatewayServer.m_Client.SendPacket(samplePacket);
                }
            }
        }

    }
}
