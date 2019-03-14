#region License
// ====================================================
// AsyncServerClient Copyright(C) 2015-2019 Furkan Türkal
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace ServerConsole
{
    public class SQLProcess
    {
        public static string ConnectionString { get; set; }

        public SQLProcess(string strConnectionString)
        {
            ConnectionString = strConnectionString;
            
        }

        public static bool ExecuteQuery(string strQuery)
        {
            try
            {
                using (SqlCommand comm = new SqlCommand(strQuery, Connect()))
                {
                    if (comm == null) return false;
                    if (comm.ExecuteNonQuery() == 1)
                    {
                        return true;
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.ExceptionToLog(ex, "[SQLProcess::ExecuteQuery()] -> ExecuteQuery error.");
            }
            return false;
        }

        public bool isWorking()
        {
            if (ConnectionString == null) return false;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                if (conn == null) return false;
               
                if (conn.State == ConnectionState.Closed)
                {
                    try
                    {
                        conn.Open();

                        Thread.Sleep(3 * 1000);

                        if (conn.State == ConnectionState.Open)
                        {
                            getSQLInfo(conn);
                            return true;
                        }

                    }
                    catch(Exception ex)
                    {
                        Logger.ExceptionToLog(ex, "[SQLProcess::isWorking()] -> Connection error.");
                    }
                }
            }
            return false;
        }

        public static object ExecuteScalar(string strQuery)
        {
            try
            {
                using (SqlCommand comm = new SqlCommand(strQuery, Connect()))
                {
                    if (comm == null) return false;
                    return comm.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Logger.ExceptionToLog(ex, "[SQLProcess::ExecuteScalar()] -> ExecuteScalar error.");
            }
            return null;
        }

        public static void getSQLInfo (SqlConnection conn)
        {
            Logger.LogToStatus("Connected to SQLSERVER Database successfully.", Logger.enLogLevel.INFO);
            Logger.LogToStatus("SQLSERVER ServerVersion : " + conn.ServerVersion, Logger.enLogLevel.INFO);
            Logger.LogToStatus("SQLSERVER WorkstationId : " + conn.WorkstationId, Logger.enLogLevel.INFO);
            Logger.LogToStatus("SQLSERVER ConnectionString : " + conn.ConnectionString, Logger.enLogLevel.INFO);
            Logger.LogToStatus("SQLSERVER ConnectionTimeout : " + conn.ConnectionTimeout.ToString(), Logger.enLogLevel.INFO);
            Logger.LogToStatus("SQLSERVER PacketSize : " + conn.PacketSize.ToString(), Logger.enLogLevel.INFO);
        }

        private static SqlConnection Connect()
        {
            if (ConnectionString == null) return null;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                if (conn == null) return null;
                   
                if (conn.State != ConnectionState.Open)
                {
                    try
                    {
                        conn.Open();

                        Thread.Sleep(1000);

                        if (conn.State  == ConnectionState.Open)
                        {
                            conn.FireInfoMessageEventOnUserErrors = true;

                            conn.StateChange += OnStateChange;
                            conn.InfoMessage += OnInfoMessage;
                        }

                    }
                    catch (Exception ex)
                    {
                        Logger.ExceptionToLog(ex, "[SQLProcess::Connect()] -> Connection error.");
                    }
                }

            return conn;
            }
        }

        private static void DisplaySqlErrors(SqlException exception)
        {
            try
            {
                foreach (SqlError err in exception.Errors)
                {
                     Logger.SQLExceptionToLog(err, "[SQLProcess::DisplaySqlErrors()] -> SqlException.");
                }
            }
            catch (Exception ex)
            {
                Logger.ExceptionToLog(ex, "[SQLProcess::DisplaySqlErrors()] -> SqlException error.");
            }
        }

        private static void OnInfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            try
            {
                foreach (SqlError info in e.Errors)
                {
                    Logger.SQLExceptionToLog(info, "[SQLProcess::OnInfoMessage()] -> SqlInfoMessageEventArgs.");
                }
            }
            catch (Exception ex)
            {
                Logger.ExceptionToLog(ex, "[SQLProcess::OnInfoMessage()] -> SqlInfoMessageEventArgs error.");
            }
}

        private static void OnStateChange(object sender, StateChangeEventArgs e)
        {
            Logger.LogToStatus("SQLSERVER Connection state changed: "+ e.OriginalState + " => "+ e.CurrentState, Logger.enLogLevel.INFO);
        }
    }
}
