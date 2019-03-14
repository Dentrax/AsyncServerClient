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

namespace ServerConsole
{
    sealed partial class PacketHandler
    {
        public Socket m_PacketSocket;

        private Security m_localSecurity;
        private TransferBuffer m_localTransferBuffer;
        private List<Packet> m_recvPackets = null;
        private List<KeyValuePair<TransferBuffer, Packet>> m_sendBuffer = null;

        public List<string> m_opcodeList = new List<string>();
        public List<int> m_opcodeCount = new List<int>();

        private Queue<Packet> m_LastPackets;

        private object m_Locker = null;
        private object n_Locker = null;

        private bool m_shouldExit = false;
        private bool m_bOpcodeLimit = true;

        private bool m_bBlowfish;
        private bool m_bSecurityBytes;
        private bool m_bHandshake;
        private bool m_bEncrypted;

        private int m_Buffer;

        public int m_opcodeData = 0;
        public int m_opcodeLimit = 0;

        private string m_ServerName = null;

        public string IP = null;

        private DateTime m_StartTime = DateTime.Now;
        private ulong m_BytesRecvFromClient = 0;

        public double m_MaxBytesPerSecLimit;

        private GatewayServer.delClientDisconnect m_delDisconnect;

        public delegate void OnPacketRecivedEventHandler(Packet packet);
        public event OnPacketRecivedEventHandler OnCommandReceived = null;

        //Settings initialize
        public PacketHandler()
        {
            m_Locker = new object();
            n_Locker = new object();

            m_bBlowfish = Program.cfgGateway.boolBlowfish;
            m_bSecurityBytes = Program.cfgGateway.boolSecBytes;
            m_bHandshake = Program.cfgGateway.boolHandshake;
            m_bEncrypted = Program.cfgGateway.boolEncrypted;

            m_Buffer = Program.cfgGateway.intMaxBuffer;

            m_ServerName = Program.cfgGateway.strProgramNameHeader;

            m_MaxBytesPerSecLimit = Program.cfgGateway.doubleMaxBytesPerSecLimit;

            m_opcodeLimit = Program.cfgGateway.intOpcodeLimit;
            m_LastPackets = new Queue<Packet>(Program.cfgGateway.intQueuePacketLimit);

            m_localSecurity = new Security();
            m_localSecurity.ChangeIdentity(m_ServerName, 0);
            m_localTransferBuffer = new TransferBuffer(m_Buffer);
        }

        public void Create(Socket _Socket, GatewayServer.delClientDisconnect _delDisconnect)
        {
            m_PacketSocket = _Socket;
            m_delDisconnect = _delDisconnect;
            m_localSecurity.ChangeIdentity(m_ServerName, 0);
            m_localSecurity.GenerateSecurity(m_bBlowfish, m_bSecurityBytes, m_bHandshake);

            IP = IPAddress.Parse(((IPEndPoint)m_PacketSocket.RemoteEndPoint).Address.ToString()).ToString();

            if (isSocketConnected(m_PacketSocket))
            {
                //Receive packets from client
                DoRecvFromClient();
            }
           
        }

        //Packet updater
        public void Update()
        {
            if (!m_shouldExit)
            {
                try
                {
                    ProcessIncoming();
                    ProcessOutgoing();
                }
                catch
                {
                    Disponse("[PacketHandler::Update()]");
                }
            }
        }

        private void DoRecvFromClient()
        {
            try
            {
                m_PacketSocket.BeginReceive(m_localTransferBuffer.Buffer, m_localTransferBuffer.Offset, m_Buffer, SocketFlags.None, new AsyncCallback(WaitForData), m_PacketSocket);
            }
            catch
            {
                Disponse("[PacketHandler::DoRecvFromClient()]");
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
                    Logger.LogToStatus("[Client::WaitForData()] -> 0 Bytes recived!", Logger.enLogLevel.WARNING);
                    Thread.Sleep(1);
                    Disponse("[PacketHandler::DoRecvFromClient(0 Bytes recived!)]");
                }
            }
            catch (SocketException s_Ex)
            {
                if (s_Ex.SocketErrorCode == SocketError.ConnectionReset)
                {
                    Disponse("[PacketHandler::WaitForData(SocketException)]");
                }
                else
                {
                    Logger.ExceptionToLog(s_Ex, "[Client::WaitForData()] -> SocketError.");
                }
            }
            catch (Exception e_Ex)
            {
                Logger.ExceptionToLog(e_Ex, "[Client::WaitForData()] -> WaitForData error.");
            }
        }

        //Download packets
        private void ProcessIncoming()
        {
            //if(!disposed)
            if (m_PacketSocket == null)
            {
                Disponse("[PacketHandler::ProcessIncoming()]");
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

        //Upload packets
        private void ProcessOutgoing()
        {
            if (m_PacketSocket == null)
            {
                Disponse("[PacketHandler::ProcessOutgoing()]");
                return;
            }

            m_sendBuffer = m_localSecurity.TransferOutgoing();
            if (m_sendBuffer != null)
            {
                foreach (var kvp in m_sendBuffer)
                {
                    Packet packet = kvp.Value;
                    PacketToConsole(packet, Process.Outgoing);
                    m_PacketSocket.Send(kvp.Key.Buffer);
                }
            }
        }

        private static Random _random = new Random();
        private static ConsoleColor GetRandomConsoleColor()
        {
            var consoleColors = Enum.GetValues(typeof(ConsoleColor));
            return (ConsoleColor)consoleColors.GetValue(_random.Next(consoleColors.Length));
        }

        private void PacketToConsole(Packet packet, Process process)
        {
            var buffer = packet.GetBytes();

            if (process == Process.Incoming)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[C->S]");
            }
            else
            {
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

        private enum Process
        {
            Incoming,
            Outgoing
        };

        //See last packets on screen
        public void DumpLastPackets(int lastPackets)
        {
            if (m_LastPackets.Count == 0) return;
            try
            {
                for (int i = 0; i < lastPackets; i++)
                {
                    Packet p = m_LastPackets.Dequeue();
                    Log("PACKET ID [{0}]: Opcode: 0x{1:X}", i + 1, p.Opcode);
                    byte[] packet_bytes = p.GetBytes();
                    Log("[DUMP][{0} bytes]{1}{2}{3}{4}{5}", packet_bytes.Length, p.Encrypted ? "[Encrypted]" : "", p.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet_bytes), Environment.NewLine);
                }
            }
            catch (Exception e_Ex)
            {
                Logger.ExceptionToLog(e_Ex, "[PacketProtocol::DumpLastPackets()] -> DumpLastPackets() error.");
            }
        }

        public void SendPacket(Packet packet)
        {
            if (m_PacketSocket == null)
            {
                Disponse("[PacketHandler::SendPacket()]");
                return;
            }
            try
            {
                m_localSecurity.Send(packet);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[PacketHandler::SendPacket()]: Send->Exception: {0}", ex.Message);
            }
            
        }

        public double getBytesPerSecondFromClient()
        {
            double res = 0.0;
            TimeSpan diff = (DateTime.Now - m_StartTime);
            if (m_BytesRecvFromClient > int.MaxValue)
            {
                m_BytesRecvFromClient = 0;
            }
            if (m_BytesRecvFromClient > 0)
            {
                try
                {
                    unchecked
                    {
                        double div = diff.TotalSeconds;
                        if (diff.TotalSeconds < 1.0)
                        {
                            div = 1.0;
                        }
                        res = Math.Round((m_BytesRecvFromClient / div), 2);
                    }
                }
                catch
                {
                }
            }

            return res;
        }

        private bool isSocketConnected(Socket s)
        {
            return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) && (!s.IsBound) || !s.Connected);
        }

        private int getIPLimit()
        {
            try
            {
                return Convert.ToInt32(GatewayListener.m_IPLimit);
            }
            catch(Exception e_Ex)
            {
                Console.WriteLine("getIPLimit Error : " + e_Ex.Message);
                Disponse("[PacketHandler::getIPLimit()]");
            }
            return 0;
        }

        public void Disponse(string strVoid)
        {
            try
            {
                lock (m_Locker)
                {
                    if (m_PacketSocket != null)
                    {
                        m_shouldExit = true;
                        if (m_delDisconnect != null)
                        {
                            m_delDisconnect.Invoke(ref m_PacketSocket);
                            Logger.DebugToLog("[Client::Disponse()] -> Disponse(" + strVoid + ")");
                        }
                    }
                }
            }
            catch (Exception e_Ex)
            {
               Logger.ExceptionToLog(e_Ex, "[Client::Disponse()] -> Disponse() error.");
               GC.SuppressFinalize(this);
            }
        }

        private void Log(string msg, params object[] values)
        {
            msg = string.Format(msg, values);
            Logger.DebugToLog(msg + "\t");
        }

    }
}
