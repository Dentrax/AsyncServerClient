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
using System.Text;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace ServerConsole
{
    sealed class GatewayListener
    {
        //Master socket
        private TcpListener s_master = null;

        //Address settings
        private string m_strIP = null;
        private string m_strPort = null;
        private int m_backLog;

        //Thread-Safe
        private object m_Locker = null;
 

        //Lists
        public static List<string> m_IPList = new List<string>();
        public static List<string> m_SpecialIPList = new List<string>();
        public static List<PacketHandler> m_ClientList = new List<PacketHandler>();
        public static List<int> m_CountList = new List<int>();

        //Events
        public delegate void OnClientConnectEventHandler(Socket connectedClient);
        public event OnClientConnectEventHandler OnClientConnected = null;

        private Thread m_AcceptInitThread = null;

        public static int m_IPLimit;

        public void Start(string ip, string port)
        {
            m_Locker = new object();

            m_strIP = ip;
            m_strPort = port;

            m_IPLimit = Program.cfgGateway.intIPLimit;
            m_backLog = Program.cfgGateway.intBackLog;

            #region IP-Port Control

            IPAddress strIPAddress;
            int intPortAddress;

            if (String.IsNullOrEmpty(ip) || String.IsNullOrEmpty(port))
            {
                Logger.LogToStatus("IP address or port number cannot be null.", Logger.enLogLevel.WARNING);
                return;
            }


            if (!IPAddress.TryParse(m_strIP, out strIPAddress))
            {
                Logger.LogToStatus("Invalid IP address entered. Defaulting to local ip address : " + getLocalIPAddress().ToString(), Logger.enLogLevel.WARNING);
                strIPAddress = IPAddress.Parse(getLocalIPAddress().ToString());
                m_strIP = strIPAddress.ToString();
            }
            else
            {
                strIPAddress = IPAddress.Parse(m_strIP);
                m_strIP = strIPAddress.ToString();
            }

            if (!Int32.TryParse(m_strPort, out intPortAddress))
            {
                Logger.LogToStatus("Invalid port entered. Defaulting to 27707", Logger.enLogLevel.WARNING);
                intPortAddress = 27707;
                m_strPort = intPortAddress.ToString();
            }
            else
            {
                intPortAddress = Convert.ToInt32(m_strPort);
                m_strPort = intPortAddress.ToString();
            }

            if (Convert.ToInt32(intPortAddress) < 0 || Convert.ToInt32(intPortAddress) > 65535)
            {
                Logger.LogToStatus("Port value out of range. Should be between [0,65535]", Logger.enLogLevel.WARNING);
                return;
            }
            

            Logger.LogToStatus("IP and Port controlled successfully. => " + "(" + strIPAddress + ";" + intPortAddress + ")", Logger.enLogLevel.INFO);
            
            IPEndPoint conAddress = new IPEndPoint(strIPAddress, intPortAddress);

            #endregion

            s_master = new TcpListener(conAddress);

            #region Connection

            int timeToTry = 1;

            Retry:

            try
            {
                if (s_master != null)
                {
                    if (!GetState())
                    {
                        s_master.Start();
                    }
                }
            }
            catch
            {
                if (timeToTry >= 5 && GetState() == false)
                {
                    Logger.LogToStatus("Connection timed out. Now it's closing...", Logger.enLogLevel.WARNING);
                    Thread.Sleep(3 * 1000);
                    Environment.Exit(0);
                }
                else
                {
                    Logger.LogToStatus("(" + timeToTry + ")" + " Error creating server listener on " + "(" + getAddress() + ")", Logger.enLogLevel.WARNING);
                    timeToTry += 1;
                    Thread.Sleep(1000);
                    goto Retry;
                }
            }
            
            Logger.LogToStatus("Please allow port " + intPortAddress + " through firewall.", Logger.enLogLevel.INFO);
            Logger.LogToStatus("Server starting in 3 seconds...", Logger.enLogLevel.INFO);

            Thread.Sleep(3 * 1000);

            #endregion

            if (GetState())
            {
                Logger.LogToStatus("Server started on "+getAddress()+" successfully.", Logger.enLogLevel.INFO);
                GetLog();
                startListener();
            }
            else
            {
                Logger.LogToStatus("Server is not available.", Logger.enLogLevel.ERROR);
            }
        }

        public void startListener()
        {
            m_AcceptInitThread = new Thread(new ThreadStart(handleConnections));
            m_AcceptInitThread.IsBackground = true;
            m_AcceptInitThread.Start();
        }

        //Client Accept
        private void handleConnections()
        {
            while (s_master != null)
            {
                lock (m_Locker)
                {
                    try
                    {
                        Socket client = s_master.AcceptSocket();
                        //client.NoDelay = true;
                        //client.Blocking = false;

                        if (client.Connected == false)
                        {
                            continue;
                        }
                        OnClientConnected?.Invoke(client);
                    }
                    catch (Exception s_Ex)
                    {
                        Logger.ExceptionToLog(s_Ex, "[TcpServer::handleConnections()] -> Error while starting AcceptSocket() context.");
                    }
                }
                Thread.Sleep(1);
            }
        }

        private int findSocketIndex(Socket socket)
        {
            for (int i = 0; i < m_ClientList.Count; i++)
            {
                if (socket == m_ClientList[i].m_PacketSocket)
                {
                    return i;
                }
            }
            return -1;
        }
        
        public string getLocalIPAddress()
        {
            return (Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault().ToString());
        }

        public string getAddress()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Clear();
                sb.Append(m_strIP);
                sb.Append(":");
                sb.Append(m_strPort);
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Logger.ExceptionToLog(ex,"[TcpServer::getAddress()] StringBuilder error.");
            }
            return 0.ToString(); ;
        }

        public bool GetState()
        {
            try
            {
                if (s_master != null)
                {
                    if (s_master.Server.IsBound)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.ExceptionToLog(ex, "[TcpServer::isRunning()] Server checking error.");
            }
            return false;
        }

        private void GetLog()
        {
            if (GetState())
            {
                Logger.LogToStatus("---------------------------------------------------------------", Logger.enLogLevel.INFO);
                Logger.LogToStatus("Server TTL : " + s_master.Server.Ttl.ToString(), Logger.enLogLevel.INFO);
                Logger.LogToStatus("Server Type : " + s_master.Server.SocketType.ToString(), Logger.enLogLevel.INFO);
                Logger.LogToStatus("Server LocalEndpoint : " + s_master.Server.LocalEndPoint, Logger.enLogLevel.INFO);
                Logger.LogToStatus("Server Handle : " + s_master.Server.Handle, Logger.enLogLevel.INFO);
                Logger.LogToStatus("Server ReceiveBufferSize : " + s_master.Server.ReceiveBufferSize, Logger.enLogLevel.INFO);
                Logger.LogToStatus("Server SendBufferSize : " + s_master.Server.SendBufferSize, Logger.enLogLevel.INFO);
                Logger.LogToStatus("Server AddressFamily : " + s_master.Server.AddressFamily, Logger.enLogLevel.INFO);
                Logger.LogToStatus("Server AddressUse : " + s_master.Server.ExclusiveAddressUse, Logger.enLogLevel.INFO);
                Logger.LogToStatus("Server Available : " + s_master.Server.Available, Logger.enLogLevel.INFO);
                Logger.LogToStatus("Server NoDelay : " + s_master.Server.NoDelay, Logger.enLogLevel.INFO);
                Logger.LogToStatus("Server Blocking : " + s_master.Server.Blocking, Logger.enLogLevel.INFO);
                Logger.LogToStatus("---------------------------------------------------------------", Logger.enLogLevel.INFO);
            }
            else
            {
                Logger.LogToStatus("Server not started.", Logger.enLogLevel.WARNING);
            }
        }

    }
}
