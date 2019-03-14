#region License
// ====================================================
// AsyncServerClient Copyright(C) 2015-2019 Furkan Türkal
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.Threading;
using ServerFramework;

namespace ServerConsole.PacketProcessing
{
    public sealed class Ping : IPacketReceiver
    {
        private object m_Locker = new object();

        private byte m_MaxRight = 3;
        private byte m_CurrentRight = 0;

        private byte m_SecondToSend = 6;

        private Thread m_ClientPingThread = null;

        public void OnCommandReceived(Packet packet)
        {
            lock (m_Locker)
            {
                if (packet.Opcode == 0x1002)
                {
                    if(packet.ReadAscii() == "Ping")
                    {
                        Packet pingResponse = new Packet(0x1003);
                        pingResponse.WriteAscii("Pong");
                        GatewayServer.m_Client.SendPacket(pingResponse);

                        pingController();
                    }
                }
            }
        }

        private void pingController()
        {
            try
            {
                if (m_ClientPingThread == null)
                {
                    m_ClientPingThread = new Thread(new ThreadStart(handlePing));
                }

                if (m_ClientPingThread.ThreadState == ThreadState.Unstarted)
                {
                    pingSuccess();
                    m_ClientPingThread.IsBackground = true;
                    m_ClientPingThread.Start();
                }
                else
                {
                    pingSuccess();
                }

            }
            catch(Exception e_Ex)
            {
                Logger.ExceptionToLog(e_Ex, "[PacketProcessing[Ping] -> pingController Error.");
            }
        }

        private void pingSuccess()
        {
            m_CurrentRight = 0;
        }


        private void handlePing()
        {
            while (true)
            {
                Thread.Sleep(m_SecondToSend * 1000);

                m_CurrentRight += 1;
                if (m_CurrentRight == m_MaxRight)
                {
                    GatewayServer.m_Client.Disponse("[PacketProcessing[Ping]::handlePing()]");
                    m_CurrentRight = 0;
                    break;
                }
            }
        }

    }
}
