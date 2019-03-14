#region License
// ====================================================
// AsyncServerClient Copyright(C) 2015-2019 Furkan Türkal
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using ServerConsole.PacketProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace ServerConsole
{
    sealed partial class GatewayServer
    {
        private GatewayListener m_ClientListener;

        public static PacketHandler m_Client;

        private Thread m_thSessionUpdater;

        //Thread-Safe
        private Object m_Locker = null;

        static int m_ClientCountList = 0;

        //Events
        public delegate void delClientDisconnect(ref Socket ClientSocket);

        //Initialize
        public GatewayServer(string ip, string port)
        {
            m_Locker = new object();

            m_ClientListener = new GatewayListener();
            m_ClientListener.OnClientConnected += onClientConnected;
            m_ClientListener.Start(ip, port);

            m_thSessionUpdater = new Thread(ClientUpdate);
            m_thSessionUpdater.IsBackground = true;
            m_thSessionUpdater.Start();
        }

        //Packet updater
        private void ClientUpdate()
        {
            while (true)
            {
                if (m_Client != null)
                {
                    m_Client.Update();
                }
                Thread.Sleep(1);
            }
        }
        
        //onClinetConnected
        //onCommandReceived
        //PacketProcessing
        private void onClientConnected(Socket clientSocket)
        {
            lock (m_Locker)
            {
                try
                {
                    if (clientSocket == null)
                    {
                        return;
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("=================================[Client---Connected]=================================");
                    Console.WriteLine("=================================[" + clientSocket.RemoteEndPoint + "]=================================");
                    Console.ResetColor();

                    m_Client = new PacketHandler();

                    //PacketProcessing
                    IEnumerable<Type> packetReceivers = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetInterfaces().Count(y => y.Name == "IPacketReceiver") > 0);
                    foreach (var receiver in packetReceivers)
                    {
                        //Read all class file in PacketProcessing folder. Using interface.
                        m_Client.OnCommandReceived +=  (Activator.CreateInstance(receiver) as IPacketReceiver).OnCommandReceived;
                    }

                    m_Client.Create(clientSocket, onClientDisconnect);
                    m_ClientCountList++;
                }
                catch (SocketException s_Ex)
                {
                    Logger.ExceptionToLog(s_Ex, "[TcpServer::onClientConnected()] -> Error opening socket.");
                    disponseSocket(clientSocket);
                }
                catch (Exception e_Ex)
                {
                    Logger.ExceptionToLog(e_Ex, "[TcpServer::onClientConnected()] -> Error.");
                    disponseSocket(clientSocket);
                }
            }
        }

        private void onClientDisconnect(ref Socket clientSocket)
        {
            lock (m_Locker)
            {
                try
                {
                    if (clientSocket == null)
                    {
                        return;
                    }

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("=================================[Client--Disconnect]=================================");
                    Console.WriteLine("=================================[" + clientSocket.RemoteEndPoint + "]=================================");
                    Console.ResetColor();

                    disponseSocket(clientSocket);
                    m_ClientCountList--;
                }
                catch (SocketException s_Ex)
                {
                    Logger.ExceptionToLog(s_Ex, "[TcpServer::onClientDisconnect()] -> Error closing socket.");
                }
                catch (ObjectDisposedException od_Ex)
                {
                    Logger.ExceptionToLog(od_Ex, "[TcpServer::onClientDisconnect()] -> Error closing socket (socket already disposed?).");
                }
                catch (Exception e_Ex)
                {
                    Logger.ExceptionToLog(e_Ex, "[TcpServer::onClientDisconnect()] -> Error.");
                }
            }
        }

        private void disponseSocket(Socket clientSocket)
        {
            lock (m_Locker)
            {
                try
                {
                    if (clientSocket != null)
                    {
                        clientSocket.Close();
                    }
                }
                catch (Exception e_Ex)
                {
                    Logger.ExceptionToLog(e_Ex, "[TcpServer::disponseSocket()] -> Error.");
                }
            }
           
        }

        public static int getClientCount()
        {
          return m_ClientCountList;
        }

    }
}
