#region License
// ====================================================
// AsyncServerClient Copyright(C) 2015-2019 Furkan Türkal
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServerFramework;

namespace ClientConsole
{
    class Program
    {
        private static PacketProtocol client;
        private static Thread thSessionUpdater;

        private static object m_Lock = new object();
        public static bool m_shouldExit = true;

        static void Main(string[] args)
        {
            Connect("192.168.1.2", 15550);
        }

        private static void Connect (string ip, int port)
        {
            Console.Title = "ClientConsole";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Console initialized.");
            Console.ResetColor();
            Thread.Sleep(1000);
            int timeToTry = 1;

        Retry:
            try
            {
                IPAddress strIPAddress = IPAddress.Parse(ip);
                IPEndPoint conAddress = new IPEndPoint(strIPAddress, port);
                //localSocket.Blocking = true;
                //localSocket.NoDelay = true;
                client = new PacketProtocol();
                client.OnCommandReceived += Client_OnCommandReceived;
                client.Connect(conAddress);
                
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine("Connection failed. Reconnecting... (" + timeToTry + ")");
                Console.ResetColor();

                if (timeToTry >= 3)
                {
                    Thread.Sleep(2000);
                    Environment.Exit(0);
                }
                Thread.Sleep(1000);
                timeToTry += 1;
                goto Retry;
            }
       
            #region Log

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=================================");
            Console.ResetColor();
            Console.WriteLine("Socket Available : " + client.GetSocket().Available);
            Console.WriteLine("Socket Blocking : " + client.GetSocket().Blocking);
            Console.WriteLine("Socket Connected : " + client.GetSocket().Connected);
            Console.WriteLine("Socket IsBound : " + client.GetSocket().IsBound);
            Console.WriteLine("Socket Poll : " + client.GetSocket().Poll(1000, SelectMode.SelectRead));
            Console.WriteLine("Socket LocalEndPoint : " + client.GetSocket().LocalEndPoint);
            Console.WriteLine("Socket RemoteEndPoint : " + client.GetSocket().RemoteEndPoint);
            Console.WriteLine("Socket NoDelay : " + client.GetSocket().NoDelay);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=================================");
            Console.ResetColor();

            Thread.Sleep(100);

            #endregion

            #region Status

            if (client.GetSocket().Connected)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Connection Success : " + client.GetSocket().RemoteEndPoint);
                Console.ResetColor();
                Thread.Sleep(1000);
                FirstEngine();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Connection Failed!");
                Console.ResetColor();
                Thread.Sleep(1000);
                Environment.Exit(0);
            }

            #endregion

            ReadME();

        }

        private static void FirstEngine()
        {
            thSessionUpdater = new Thread(ThreadedSessionUpdating);
            thSessionUpdater.Start();

            ReadME();
        }

        private static void ThreadedSessionUpdating()
        {
            while (true)
            {
                if (client != null)
                {
                    client.Update();
                }
                Thread.Sleep(1);
            }
        }

        private static void Client_OnCommandReceived(Packet packet)
        {
            if (packet.Opcode == 0x2100)
            {
                int id = packet.ReadInt32();
                string message = packet.ReadAscii();
                
                Packet sampleResponse = new Packet(0x2101);
                sampleResponse.WriteAscii("Hello, Server! Thank you for log me in! Can I send my private identity as encrypted?");
                client.SendPacket(sampleResponse);
            }

            if (packet.Opcode == 0x2102) {
                string message = packet.ReadAscii();

                Packet sampleResponse = new Packet(0x2103, true);
                sampleResponse.WriteAscii("I am sending my private identity as encrypted.");
                sampleResponse.WriteInt32(77);
                sampleResponse.WriteInt32(11);
                sampleResponse.WriteDouble(6.7);
                sampleResponse.WriteUInt64(UInt64.MaxValue - 1);
                client.SendPacket(sampleResponse);
            }

            if (packet.Opcode == 0x2008)
            {
                string message = packet.ReadAscii();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Message: {0}", message);
                Console.ResetColor();
            }
        }

        private void Lazım()
        {
            /*

             byte[] bytes = _pck.GetBytes();
                                Log("[SERVER->CLIENT][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", new object[]
                                {
                                    _pck.Opcode,
                                    bytes.Length,
                                    _pck.Encrypted ? "[Encrypted]" : "",
                                    _pck.Massive ? "[Massive]" : "",
                                    Environment.NewLine,
                                    Utility.HexDump(bytes),
                                    Environment.NewLine
                                });

                                if (_pck.Opcode == 0x2002)
                                {
                                    if (_pck.ReadInt8() == 1)
                                    {
                                        if (_pck.ReadInt8() == 3)
                                        {
                                            if (_pck.ReadAscii() == "OK")
                                            {
                                                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!");
                                                Console.WriteLine("SERIAL ACCEPTED.");
                                                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!");
                                                continue;
                                            }
                                        }
                                    }
                                }

            */
        }

        private static void ReadME()
        {
            Again:
            string r = Console.ReadLine();
            if (r == "ben")
            {
                if (client.GetSocket().IsBound)
                {

                    Packet packet = new Packet(0x2003);
                    packet.WriteInt8(2);
                    packet.WriteInt8(4);
                    packet.WriteAscii("BEN");
                    client.SendPacket(packet);
                    Console.WriteLine("Packet send ?");
                    goto Again;
                }
            }
            else
            {
                goto Again;
            }

        }

        private static void CleanClient(int Where)
        {
            try
            {
                //remote = false;
                Console.WriteLine("Disconnected Agent. (" + Where.ToString() + ")");
                client.GetSocket().Close();
            }
            catch
            {
            }
        }

        public static void Log(string msg, params object[] values)
        {
            msg = string.Format(msg, values);
            Console.WriteLine(msg + "\t");
        }

    }
}
