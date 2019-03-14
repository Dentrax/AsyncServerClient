#region License
// ====================================================
// AsyncServerClient Copyright(C) 2015-2019 Furkan Türkal
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.IO;
using System.Threading;
using System.Reflection;

namespace ServerConsole
{
    class Program
    {
        public static Config cfgGateway;

        private static SQLProcess sqlProcessAccount;
        private static SQLProcess sqlProcessShard;
        private static SQLProcess sqlProcessLog;

        private static bool bLoadModule = false;

        static void Main(string[] args)
        {
            if (bLoadModule != true)
            {
                Console.Title = Assembly.GetExecutingAssembly().GetName().Name;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Load Module Deactivated !!!");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine();
                Console.WriteLine("====================================");
                Console.WriteLine("Console initialized successfully.");
                Console.WriteLine("====================================");
                Console.WriteLine();
                Console.ResetColor();

                if (SetupConfig() == false) {
                    Logger.DebugToLog("Config file load error !!!");
                    Console.ReadKey();
                }

                if (cfgGateway.boolDebugMode)
                {
                    Logger.DebugToLog("Debug Mode Activated !!!");
                }
                else
                {
                    Logger.DebugToLog("Debug Mode Deactivated !!!");
                }
                Console.ForegroundColor = ConsoleColor.Green;

                Start();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Load Module Activated !!!");
                Console.WriteLine();
                if (cfgGateway.boolDebugMode)
                {
                    Logger.DebugToLog("Debug Mode Activated !!!");
                }
                else
                {
                    Logger.DebugToLog("Debug Mode Deactivated !!!");
                }
                Console.WriteLine();
                Console.ResetColor();
                AppDomain currentDomain = AppDomain.CurrentDomain;
                LoadAssembly(currentDomain);
            }

        }

        private static void LoadAssembly(AppDomain domain)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("====================================");
            Console.WriteLine("LOADED ASSEMBLIES:");
            Console.WriteLine("====================================");
            Console.WriteLine();

            foreach (Assembly a in domain.GetAssemblies())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Assembly Loaded: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(a.ManifestModule.Name);
                Console.WriteLine();
                Thread.Sleep(100);
                if (a.ManifestModule.Name.Contains(Assembly.GetExecutingAssembly().GetName().Name))
                {
                    SetupConsole();
                }
            }
            Console.ResetColor();
        }

        private static void SetupConsole()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Title = Assembly.GetExecutingAssembly().GetName().Name;
            Console.WriteLine();
            Console.WriteLine("====================================");
            Console.WriteLine("Console initialized successfully.");
            Console.WriteLine("====================================");
            Console.WriteLine();
            Thread.Sleep(100);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Assembly Location     : ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine();
            Thread.Sleep(100);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Assembly Name         : ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Assembly.GetExecutingAssembly().GetName().Name);
            Console.WriteLine();
            Thread.Sleep(100);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("HashAlgorithm         : ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Assembly.GetExecutingAssembly().GetName().HashAlgorithm);
            Console.WriteLine();
            Thread.Sleep(100);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("ProcessorArchitecture : ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Assembly.GetExecutingAssembly().GetName().ProcessorArchitecture);
            Console.WriteLine();
            Thread.Sleep(100);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Version               : ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine();
            Thread.Sleep(100);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("VersionCompatibility  : ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Assembly.GetExecutingAssembly().GetName().VersionCompatibility);
            Console.WriteLine();
            Thread.Sleep(100);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Buid Date             : ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(BuildDate().ToString());
            Console.WriteLine();
            Thread.Sleep(100);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("====================================");
            Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Name + " started successfully.");
            Console.WriteLine("====================================");
            Console.WriteLine();
            Thread.Sleep(100);

            Console.ResetColor();

            Awake();
        }

        private static DateTime BuildDate()
        {
            string filePath = Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];
           Stream s = null;

            try
            {
                s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            int i = BitConverter.ToInt32(b, c_PeHeaderOffset);
            int secondsSince1970 = BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.ToLocalTime();
            return dt;
        }

        private static bool SetupConfig()
        {
            cfgGateway = new Config();

            if (cfgGateway.LoadConfig())
            {
                 return true;
            }
           return false;
        }

        private static bool SetupDatabase()
        {
            sqlProcessAccount = new SQLProcess(cfgGateway.strSQLConnectionStringAccount);
            sqlProcessShard = new SQLProcess(cfgGateway.strSQLConnectionStringShard);
            sqlProcessLog = new SQLProcess(cfgGateway.strSQLConnectionStringLog);

            int intIndex = 0;
            if (sqlProcessAccount.isWorking())
            {
                intIndex += 1;
                Logger.LogToStatus("Database connected successfully: Account", Logger.enLogLevel.INFO);
            }
            if (sqlProcessShard.isWorking())
            {
                intIndex += 1;
                Logger.LogToStatus("Database connected successfully: Shard", Logger.enLogLevel.INFO);
            }
            if (sqlProcessLog.isWorking())
            {
                intIndex += 1;
                Logger.LogToStatus("Database connected successfully: Log", Logger.enLogLevel.INFO);
            }
            if (intIndex == 3)
            {
                return true;
            }
            return false;
        }

        private static void Awake()
        {
            try
            {
                if (SetupConfig())    
                {                
                    Logger.LogToStatus("Config has been loaded successfully.", Logger.enLogLevel.INFO);
                    Logger.LogToStatus("Settings loaded. Checking database connection...", Logger.enLogLevel.INFO);
                }
                else
                {
                    Logger.WriteToConsole("INI file was an error on loading. Please try again.", Logger.enLogLevel.ERROR);
                    return;
                }

                Thread.Sleep(1000);

                if (SetupDatabase())
                {
                    Thread.Sleep(1000);
                    Logger.LogToStatus("Database connection functions working.", Logger.enLogLevel.INFO);
                    Thread.Sleep(1000);
                }
                else
                {
                    Logger.LogToStatus("Database connection error. Please try again.", Logger.enLogLevel.ERROR);
                    return;
                }

                Start();
            }
            catch(Exception ex)
            {
                Logger.ExceptionToLog(ex, "[Program::Awake()] Config loading error.");
                
            }
        }

        private static void Start()
        {
            try
            {
                //GatewayServer s_Tcp = new GatewayServer(cfgGateway.strIP, cfgGateway.strPort);
                GatewayServer s_Tcp = new GatewayServer("192.168.1.2", "15550");
                ConsoleUpdate();
                ReadUpdate();
            }
            catch (Exception ex)
            {
                Logger.ExceptionToLog(ex, "[Program::Start()] -> TcpServer error.");
            }
        }

        public static void CollectGarbage()
        {
            try
            {
                long n_BeforeGC = GC.GetTotalMemory(false);
                double mb_BeforeGC = ConvertBytesToMegabytes(n_BeforeGC);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                long n_AfterGC = GC.GetTotalMemory(true);
                double mb_AfterGC = ConvertBytesToMegabytes(n_AfterGC);
                double mb_CleanedGC = mb_BeforeGC - mb_AfterGC;
                Logger.LogToStatus("All garbage were collected. [Cleaned : " + mb_CleanedGC + " MB]", Logger.enLogLevel.INFO);
                n_BeforeGC = 0;
                n_AfterGC = 0;
            }
            catch
            {

            }
        }

        static double ConvertBytesToMegabytes(long bytes)
        {
            return Math.Round((bytes / 1024f) / 1024f,3);
        }

        public static void ConsoleUpdate()
        {
            while (true)
            {
                Console.Title = Assembly.GetExecutingAssembly().GetName().Name + "[Clients:" + GatewayServer.getClientCount() + "]";
                Thread.Sleep(10);
                //Thread.Sleep(cfgGateway.intGarbageCollectTime);
            }

        }

        public static void ReadUpdate()
        {
            while (true)
            {
                Thread.Sleep(5000);
                if (GatewayServer.getClientCount() == 0)
                {
                    CollectGarbage();
                    break;
                }
                //Thread.Sleep(cfgGateway.intGarbageCollectTime);
            }

        }
    }
}
