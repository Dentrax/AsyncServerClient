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
using System.Text;
using System.Data.SqlClient;

namespace ServerConsole
{
    public class Logger
    {
        public static string strPath = null;

        public Logger(string path)
        {
            strPath = path;
        }

        public enum enLogLevel
        {
            DEBUG,
            INFO,
            ERROR,
            WARNING
        }

        public static void LogMessage(string strPath, string strMessage, enLogLevel enLevel)
        {
            lock (strPath)
            {
                try
                {
                    if(strPath == null || strMessage == null) return;
                    string strLog = string.Empty;
                    StringBuilder sbLog = new StringBuilder();
                    StreamWriter swLog = new StreamWriter(strPath,true);
                    DateTime dtNow = DateTime.Now;
                    string strDate = string.Format("{0:d.M.yyyy}", dtNow);
                    switch (enLevel)
                    {
                        case enLogLevel.DEBUG:
                            sbLog.Append("[DEBUG]");
                            sbLog.Append("  ");
                            break;
                        case enLogLevel.INFO:
                            sbLog.Append("[INFO]");
                            sbLog.Append("   ");
                            break;
                        case enLogLevel.ERROR:
                            sbLog.Append("[ERROR]");
                            sbLog.Append("  ");
                            break;
                        case enLogLevel.WARNING:
                            sbLog.Append("[WARNING]");
                            sbLog.Append(" ");
                            break;
                        default:
                            sbLog.Append("[UNKNOWN]");
                            break;
                    }
                    sbLog.Append(" -> [" + strDate + "] :: ");
                    sbLog.Append(strMessage);
                    strLog = sbLog.ToString();
                    swLog.WriteLine(strLog);
                    swLog.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[Logger::LogMessage()] -> Logging error : " + ex.Message);
                }
            }
        }

        public static void WriteToConsole(string strText)
        {
            if (strText == null) return;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(strText);
            Console.ResetColor();
        }

        public static void WriteToConsole(string strText, ConsoleColor cColor)
        {
            if (strText == null) return;
            Console.ForegroundColor = cColor;
            Console.WriteLine(strText);
            Console.ResetColor();
        }

        public static void WriteToConsole(string strText, enLogLevel enLevel, ConsoleColor cColor)
        {
            if (strText == null) return;
            switch (enLevel)
            {
                case enLogLevel.DEBUG:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[DEBUG]   : ");
                    break;
                case enLogLevel.INFO:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[INFO]    : ");
                    break;
                case enLogLevel.ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[ERROR]   : ");
                    break;
                case enLogLevel.WARNING:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write("[WARNING] : ");
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("[UNKNOWN] : ");
                    break;
            }
            Console.ForegroundColor = cColor;
            Console.Write(strText);
            Console.WriteLine();
            Console.ResetColor();
        }

        public static void WriteToConsole(string strText, enLogLevel enLevel)
        {
            if (strText == null) return;
            switch (enLevel)
            {
                case enLogLevel.DEBUG:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[DEBUG]   : ");
                    break;
                case enLogLevel.INFO:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[INFO]    : ");
                    break;
                case enLogLevel.ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[ERROR]   : ");
                    break;
                case enLogLevel.WARNING:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write("[WARNING] : ");
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("[UNKNOWN] : ");
                    break;
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(strText);
            Console.WriteLine();
            Console.ResetColor();
        }

        public static void DebugToLog(string strText)
        {
            if (strText == null) return;
            if (Program.cfgGateway.boolDebugMode == false) return;
            WriteToConsole(strText, enLogLevel.DEBUG);
            LogMessage(strPath, strText, enLogLevel.DEBUG);
        }

        public static void ExceptionToLog(Exception strEx, string strVoid)
        {
            if (strEx == null || strVoid == null) return;
            LogToStatus("[Exception Void] : " + strVoid, enLogLevel.ERROR);
            LogToStatus("[Exception Type] : " + strEx.GetType(), enLogLevel.ERROR);
            LogToStatus("[Exception Data] : "  + strEx.Data, enLogLevel.ERROR);
            LogToStatus("[Exception Message] : " + Environment.NewLine + strEx.Message, enLogLevel.ERROR);
            if (Program.cfgGateway.boolStackTree)
            {
                LogToStatus("[Exception Data] : " + strEx.Data, enLogLevel.ERROR);
                LogToStatus("[Exception StackTrace] : " + Environment.NewLine + strEx.StackTrace, enLogLevel.ERROR);
            }

        }

        public static void SQLExceptionToLog(SqlError strEx, string strVoid)
        {
            if (strEx == null || strVoid == null) return;
            LogToStatus("[SQLException Void] : " + strVoid, enLogLevel.ERROR);
            LogToStatus("[SQLException Type] : " + strEx.GetType(), enLogLevel.ERROR);
            LogToStatus("[SQLException Class] : " + strEx.Class, enLogLevel.ERROR);
            LogToStatus("[SQLException LineNumber] : " + strEx.LineNumber.ToString(), enLogLevel.ERROR);
            LogToStatus("[SQLException Message] : " + strEx.Message, enLogLevel.ERROR);
            LogToStatus("[SQLException Number] : " + strEx.Number, enLogLevel.ERROR);
            LogToStatus("[SQLException Procedure] : " + strEx.Procedure, enLogLevel.ERROR);
            LogToStatus("[SQLException Server] : " + strEx.Server, enLogLevel.ERROR);
            LogToStatus("[SQLException State] : " + strEx.State.ToString(), enLogLevel.ERROR);
            LogToStatus("[SQLException Message] : " + strEx.Message, enLogLevel.ERROR);
            LogToStatus("[SQLException Source] : " + Environment.NewLine + strEx.Source, enLogLevel.ERROR);
        }

        public static void LogToStatus(string strText, enLogLevel enLevel)
        {
            if (strText == null) return;
            WriteToConsole(strText, enLevel);
            LogMessage(strPath, strText, enLevel);
        }

        public static void LogToStatus(string strText, enLogLevel enLevel, ConsoleColor cColor)
        {
            if (strText == null) return;
            WriteToConsole(strText, enLevel, cColor);
            LogMessage(strPath, strText, enLevel);
        }
    }
}
