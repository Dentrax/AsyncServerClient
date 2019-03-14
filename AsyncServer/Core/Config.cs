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
using System.Reflection;

namespace ServerConsole
{
    public class Config
    {

        //ProgramSettings
        public readonly string strProgramNameHeader = "AsyncServerClient";
        public readonly string strConfigPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\server.ini";
        public bool boolDebugMode { get; private set; }

        //ServerSettings
        public bool boolBlowfish { get; private set; }
        public bool boolSecBytes { get; private set; }
        public bool boolHandshake { get; private set; }
        public bool boolEncrypted { get; private set; }
        public int intOpcodeLimit { get; private set; }
        public int intQueuePacketLimit { get; private set; }
        public int intBackLog { get; private set; }
        public int intIPLimit { get; private set; }
        public int intClientCountLimit { get; private set; }
        public int intMaxBuffer { get; private set; }
        public double doubleMaxBytesPerSecLimit { get; private set; }

        //NetworkSettings
        public string strIP { get; private set; }
        public string strPort { get; private set; }

        //GeneralSettings
        public int intConsoleUpdateTime { get; private set; }
        public int intGarbageCollectTime { get; private set; }

        //LoggerSettings
        public string strLoggerPath { get; private set; }
        public string strLoggerName { get; private set; }
        public string strLoggerType { get; private set; }
        public bool boolStackTree { get; private set; }

        //SQLSERVERSettings
        public string strSQLUserID { get; private set; }
        public string strSQLUserPW { get; private set; }
        public string strSQLDataSource { get; private set; }
        public string strSQLConnectionStringAccount { get; private set; }
        public string strSQLConnectionStringShard { get; private set; }
        public string strSQLConnectionStringLog { get; private set; }
        public string strSQLAccountDB { get; private set; }
        public string strSQLShardDB { get; private set; }
        public string strSQLLogDB { get; private set; }


        //GatewayReadonlySettings
        private readonly string strIPKey = "IP";
        private readonly string strPortKey = "Port";
        private readonly string strDebugModeKey = "DebugMode";

        //ServerReadonlySettings
        private readonly string strServerSettingsHeader = "Server";
        private readonly string strBlowfishKey = "Blowfish";
        private readonly string strSecBytesKey = "SecurityBytes";
        private readonly string strHandshakeKey = "Handshake";
        private readonly string strEncryptedKey = "Encrypted";
        private readonly string strMaxBufferKey = "MaxBuffer";
        private readonly string strBackLogKey = "BackLog";
        private readonly string strIPLimitKey = "IPLimit";
        private readonly string strOpcodeLimitKey = "OpcodeLimit";
        private readonly string strQueuePacketLimitKey = "QueuePacketLimit";
        private readonly string strMaxBytesPerSecLimitKey = "MaxBytesPerSecLimit";
        private readonly string strClientCountLimitKey = "ClientCountLimit";

        //LoggerReadonlySettings
        private readonly string strLoggerSettingsHeader = "Logger";
        private readonly string strLoggerPathKey = "LoggerPath";
        private readonly string strLoggerNameKey = "LoggerName";
        private readonly string strLoggerTypeKey = "LoggerType";
        private readonly string strEnableStackTreeKey = "StackTree";

        //GeneralReadonlySettings
        private readonly string strGeneralSettingsHeader = "General";
        private readonly string strConsoleUpdateTimeKey = "ConsoleUpdateTime";
        private readonly string strGarbageCollectTimeKey = "GarbageCollectTime";

        //SQLSERVERReadonlySettings
        private readonly string strSQLConnectionString = "Data Source=%SERVER%;Initial Catalog=%DATABASE%;User ID=%UID%;Password=%UPW%;";

        private readonly string strSQLSettingsHeader = "Database";
        private readonly string strSQLUserIDKey = "UID";
        private readonly string strSQLUserPWKey = "UPW";
        private readonly string strSQLDataSourceKey = "DataSource";
        private readonly string strSQLAccountDBKey = "ACCOUNT_DB";
        private readonly string strSQLShardDBKey = "SHARD_DB";
        private readonly string strSQLLogDBKey = "LOG_DB";

        public bool LoadConfig()
        {
            try
            {
                if (File.Exists(strConfigPath) == true)
                {
                    INIStream serverINI = new INIStream(strConfigPath);

                    //Program
                    #region Program

                    strIP = serverINI.Read(strProgramNameHeader, strIPKey);
                    strPort = serverINI.Read(strProgramNameHeader, strPortKey);
                
                    boolDebugMode = Functions.ToBoolean(serverINI.Read(strProgramNameHeader, strDebugModeKey));

                    #endregion

                    //General
                    #region General

                    intConsoleUpdateTime = Convert.ToInt32(serverINI.Read(strGeneralSettingsHeader, strConsoleUpdateTimeKey));
                    intGarbageCollectTime = Convert.ToInt32(serverINI.Read(strGeneralSettingsHeader, strGarbageCollectTimeKey));

                    #endregion

                    //Logger
                    #region Logger
                    DateTime dtNow = DateTime.Now;
                    string strDate = string.Format("{0:d.M.yyyy}", dtNow);

                    strLoggerPath = serverINI.Read(strLoggerSettingsHeader, strLoggerPathKey);
                    strLoggerPath = strLoggerPath.Replace("%StartupPath%", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                    strLoggerPath = strLoggerPath.Replace("%ProgramName%", strProgramNameHeader);

                    strLoggerName = serverINI.Read(strLoggerSettingsHeader, strLoggerNameKey);
                    strLoggerName = strLoggerName.Replace("%ProgramName%", strProgramNameHeader);
                    strLoggerName = strLoggerName.Replace("%Date%", strDate);

                    strLoggerType = serverINI.Read(strLoggerSettingsHeader, strLoggerTypeKey);

                    boolStackTree = Functions.ToBoolean(serverINI.Read(strLoggerSettingsHeader, strEnableStackTreeKey));

                    #endregion

                    //Server
                    #region Server

                    boolBlowfish = Functions.ToBoolean(serverINI.Read(strServerSettingsHeader, strBlowfishKey));
                    boolSecBytes = Functions.ToBoolean(serverINI.Read(strServerSettingsHeader, strSecBytesKey));
                    boolHandshake = Functions.ToBoolean(serverINI.Read(strServerSettingsHeader, strHandshakeKey));
                    boolEncrypted = Functions.ToBoolean(serverINI.Read(strServerSettingsHeader, strEncryptedKey));

                    intMaxBuffer = Convert.ToInt32(serverINI.Read(strServerSettingsHeader, strMaxBufferKey));
                    intOpcodeLimit = Convert.ToInt32(serverINI.Read(strServerSettingsHeader, strOpcodeLimitKey));
                    intQueuePacketLimit = Convert.ToInt32(serverINI.Read(strServerSettingsHeader, strQueuePacketLimitKey));
                    intBackLog = Convert.ToInt32(serverINI.Read(strServerSettingsHeader, strBackLogKey));
                    intIPLimit = Convert.ToInt32(serverINI.Read(strServerSettingsHeader, strIPLimitKey));
                    intClientCountLimit = Convert.ToInt32(serverINI.Read(strServerSettingsHeader, strClientCountLimitKey));

                    doubleMaxBytesPerSecLimit = Convert.ToDouble(serverINI.Read(strServerSettingsHeader, strMaxBytesPerSecLimitKey));

                    

                    #endregion

                    //SQLSERVER
                    #region SQLSERVER
                    strSQLUserID = serverINI.Read(strSQLSettingsHeader, strSQLUserIDKey);
                    strSQLUserPW = serverINI.Read(strSQLSettingsHeader, strSQLUserPWKey);
                    strSQLDataSource = serverINI.Read(strSQLSettingsHeader, strSQLDataSourceKey);
                    strSQLAccountDB = serverINI.Read(strSQLSettingsHeader, strSQLAccountDBKey);
                    strSQLShardDB = serverINI.Read(strSQLSettingsHeader, strSQLShardDBKey);
                    strSQLLogDB = serverINI.Read(strSQLSettingsHeader, strSQLLogDBKey);

                    strSQLConnectionStringAccount = strSQLConnectionString;
                    strSQLConnectionStringShard = strSQLConnectionString;
                    strSQLConnectionStringLog = strSQLConnectionString;

                    strSQLConnectionStringAccount = strSQLConnectionStringAccount.Replace("%SERVER%", strSQLDataSource);
                    strSQLConnectionStringAccount = strSQLConnectionStringAccount.Replace("%DATABASE%", strSQLAccountDB);
                    strSQLConnectionStringAccount = strSQLConnectionStringAccount.Replace("%UID%", strSQLUserID);
                    strSQLConnectionStringAccount = strSQLConnectionStringAccount.Replace("%UPW%", strSQLUserPW);

                    strSQLConnectionStringShard = strSQLConnectionStringShard.Replace("%SERVER%", strSQLDataSource);
                    strSQLConnectionStringShard = strSQLConnectionStringShard.Replace("%DATABASE%", strSQLShardDB);
                    strSQLConnectionStringShard = strSQLConnectionStringShard.Replace("%UID%", strSQLUserID);
                    strSQLConnectionStringShard = strSQLConnectionStringShard.Replace("%UPW%", strSQLUserPW);

                    strSQLConnectionStringLog = strSQLConnectionStringLog.Replace("%SERVER%", strSQLDataSource);
                    strSQLConnectionStringLog = strSQLConnectionStringLog.Replace("%DATABASE%", strSQLLogDB);
                    strSQLConnectionStringLog = strSQLConnectionStringLog.Replace("%UID%", strSQLUserID);
                    strSQLConnectionStringLog = strSQLConnectionStringLog.Replace("%UPW%", strSQLUserPW);
                    #endregion

                    //END

                    #region Controller

                    if (!strLoggerPath.EndsWith("\\"))
                    {
                        strLoggerPath = strLoggerPath + "\\";
                    }

                    if (Directory.Exists(strLoggerPath) != true)
                    {
                        DirectoryInfo di = Directory.CreateDirectory(strLoggerPath);
                    }

                    strLoggerPath = strLoggerPath + strLoggerName + strLoggerType;
                    Logger.strPath = strLoggerPath;

                    if (File.Exists(strLoggerPath) != true)
                    {
                        Logger.LogMessage(strLoggerPath, "Logger file has been created.", Logger.enLogLevel.INFO);

                    }
                    #endregion

                    return true;
                }
                
            }
            catch(Exception ex)
            {
                Console.WriteLine("Please check .ini file to run. Exception : " + ex.Message);
            }
            return false;
        }


    }
}
