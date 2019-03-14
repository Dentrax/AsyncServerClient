#region License
// ====================================================
// AsyncServerClient Copyright(C) 2015-2019 Furkan Türkal
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServerFramework;

namespace ClientConsole
{
    sealed public class PacketProtocol
    {

        private Socket m_socket;

        private Security m_localSecurity = null;
        private TransferBuffer m_localTransferBuffer = null;
        private List<Packet> m_recvPackets = null;
        private List<KeyValuePair<TransferBuffer, Packet>> m_sendBuffer = null;
        public Thread m_Updater;

        private object m_Locker = null;

        public bool m_shouldExit = false;

        private int m_Buffer;

        private string m_ClientName = null;

        public delegate void OnPacketRecivedEventHandler(Packet packet);
        public event OnPacketRecivedEventHandler OnCommandReceived = null;


        public Socket GetSocket() { return this.m_socket; }

        public PacketProtocol()
        {
            m_Locker = new object();

            m_Buffer = 8192;

            m_ClientName = "AsyncServerClient";

            m_localSecurity = new Security();
            m_localSecurity.ChangeIdentity(m_ClientName, 0);

            m_localTransferBuffer = new TransferBuffer(m_Buffer);
        }

        public void Connect(IPEndPoint endPoint) {
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try {
                m_socket.Connect(endPoint);

                Console.WriteLine("[PacketProtocol::Connect()]:  Connected to {0}", m_socket.RemoteEndPoint.ToString());

                if (m_socket.Connected) {
                    m_socket.BeginReceive(m_localTransferBuffer.Buffer, m_localTransferBuffer.Offset, m_Buffer, SocketFlags.None, new AsyncCallback(WaitForData), m_socket);
                }

            } catch (Exception ex) {
                m_shouldExit = true;
                Console.WriteLine("[PacketProtocol::Connect()]: {0}", ex.Message);
            }
        }

        public void SendPacket(Packet pck)
        {
            try
            {
                m_localSecurity.Send(pck);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[PacketProtocol::Send()]: Exception: {0}", ex.Message);
            }
        }

        public void Update()
        {
            while (!m_shouldExit)
            {
                try
                {
                    ProcessIncoming();
                    ProcessOutgoing();
                }
                catch
                {
                    Disponse();
                }
                Thread.Sleep(1);
            }
        }

        private void DoRecvFromClient()
        {
            try
            {
                m_socket.BeginReceive(m_localTransferBuffer.Buffer, m_localTransferBuffer.Offset, m_Buffer, SocketFlags.None, new AsyncCallback(WaitForData), m_socket);
            }
            catch
            {
                Disponse();
            }
        }

        private void WaitForData(IAsyncResult i_AR)
        {
            lock (m_Locker)
            {
                if (m_shouldExit)
                {
                    return;
                }

            }
            try
            {
                var socket = i_AR.AsyncState as Socket;
                m_localTransferBuffer.Size = socket.EndReceive(i_AR);

                if (m_localTransferBuffer.Size > 0)
                {
                    m_localSecurity.Recv(m_localTransferBuffer);
                    socket.BeginReceive(m_localTransferBuffer.Buffer, m_localTransferBuffer.Offset, m_Buffer, SocketFlags.None, new AsyncCallback(WaitForData), socket);
                }
                else
                {
                    Console.WriteLine("[PacketProtocol::WaitForData()]: Warning: 0 Bytes recived!");
                    Thread.Sleep(1);
                    Disponse();
                }
            }
            catch (SocketException sex)
            {
                if (sex.SocketErrorCode == SocketError.ConnectionReset)
                {
                    Disponse();
                }
                else
                {
                    Console.WriteLine("[PacketProtocol::WaitForData()]: SocketException: {0}", sex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[PacketProtocol::WaitForData()]: Exception: {0}", ex.Message);
            }
        }

        private void ProcessIncoming()
        {
            //if(!disposed)
            if (m_socket == null)
            {
                Console.WriteLine("m_ClientSocket == null");
                return;
            }
            m_recvPackets = m_localSecurity.TransferIncoming();
            if (m_recvPackets != null)
            {
                foreach (var packet in m_recvPackets)
                {
                    PacketToConsole(packet, Process.Incoming);

                    if (packet.Opcode == 0x5000 || packet.Opcode == 0x9000 || packet.Opcode == 0x2002)
                    {
                        continue;
                    }

                    OnCommandReceived?.Invoke(packet);
                }
            }
        }

        private void ProcessOutgoing()
        {
            if (m_socket == null)
            {
                Disponse();
                return;
            }

            m_sendBuffer = m_localSecurity.TransferOutgoing();
            if (m_sendBuffer != null)
            {
                foreach (var kvp in m_sendBuffer)
                {
                    Packet packet = kvp.Value;
                    PacketToConsole(packet, Process.Outgoing);
                    m_socket.Send(kvp.Key.Buffer);
                }
            }
        }

        private void Disponse()
        {
            try
            {
                m_shouldExit = true;

                if (m_socket != null)
                {
                    m_socket.Close();
                }

                m_Updater.Abort();

                m_socket = null;
            }
            catch (Exception e_Ex)
            {
                Console.WriteLine("[PacketProtocol::Disponse()] -> Disponse() error. : " + e_Ex.Message);
            }

        }

        private enum Process {
            Incoming,
            Outgoing
        };

        private void PacketToConsole(Packet packet, Process process) {
            var buffer = packet.GetBytes();

            if (process == Process.Incoming) {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[C->S]");
            } else {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("[S->C]");
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("[{0:X4}]", packet.Opcode);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("[{0} bytes]", buffer.Length);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(packet.Encrypted ? "[Encrypted]" : "");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(packet.Massive ? "[Massive]" : "");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Utility.HexDump(buffer));
            Console.WriteLine();
            Console.ResetColor();
        }

        public static void Log(string msg, params object[] values)
        {
            msg = string.Format(msg, values);
            Console.WriteLine(msg + "\t");
        }

    }
}
