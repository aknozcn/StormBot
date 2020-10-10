using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StormBot.Functions;

namespace StormBot
{
    public class MSSQL : IDisposable
    {
        #region Private Properties and Fields

        private SqlConnection m_Connection;

        #endregion

        #region Constructors & Destructors

        public MSSQL(string connectionStr, string dbname)
        {
            m_Connection = new SqlConnection(connectionStr);
            try
            {
                m_Connection.Open();
            }
            catch (Exception ex)
            {
                Logger.LogIt("Mssql connection: " + ex.ToString(), LogType.Hata);
            }
        }

        public MSSQL(string Server, string DBName, string UserID, string Password, bool MultipleActiveResultSets)
            : this(String.Format("Server={0};Database={1};User Id={2};Password={3};MultipleActiveResultSets={4}", Server, DBName, UserID, Password, MultipleActiveResultSets), DBName)
        {

        }

        public bool isConnected()
        {
            if (m_Connection.State == ConnectionState.Open)
                return true;
            else
                return false;
        }

        public void Dispose()
        {
            m_Connection.Close();
            m_Connection.Dispose();
        }

        #endregion

        #region Public Methods

        public int ExecuteCommand(string SQLCommand, params object[] args)
        {
            int returnValue = 0;
            try
            {
                SQLCommand = ReplaceArgs(SQLCommand, args);
                using (var cmd = new SqlCommand(SQLCommand, m_Connection))
                    returnValue = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.LogIt(ex.ToString(), LogType.Hata);
            }

            return returnValue;
        }

        public Task<int> ExecuteCommandAsync(string SQLCommand, params object[] args)
        {
            SQLCommand = ReplaceArgs(SQLCommand, args);
            using (var cmd = new SqlCommand(SQLCommand, m_Connection))
                return cmd.ExecuteNonQueryAsync();
        }


        public SqlDataReader ExecuteReader(string SQLCommand, params object[] args)
        {
            SqlDataReader returnValue = null;
            try
            {
                SQLCommand = ReplaceArgs(SQLCommand, args);
                using (var cmd = new SqlCommand(SQLCommand, m_Connection))
                    returnValue = cmd.ExecuteReader();
            }
            catch (Exception ex )
            {
                Logger.LogIt("ExecuteReader: " + ex.ToString(), LogType.Hata);
            }

            return returnValue;
        }

        public Task<SqlDataReader> ExecuteReaderAsync(string SQLCommand, params object[] args)
        {
            SQLCommand = ReplaceArgs(SQLCommand, args);
            using (var cmd = new SqlCommand(SQLCommand, m_Connection))
                return cmd.ExecuteReaderAsync();
        }

        public T Result<T>(string SQLCommand, params object[] args)
        {
            SQLCommand = ReplaceArgs(SQLCommand, args);
            using (var cmd = new SqlCommand(SQLCommand, m_Connection))
                return (T)cmd.ExecuteScalar();
        }

        public Task<object> ResultAsync(string SQLCommand, params object[] args)
        {
            SQLCommand = ReplaceArgs(SQLCommand, args);
            using (var cmd = new SqlCommand(SQLCommand, m_Connection))
                return cmd.ExecuteScalarAsync();
        }

        public int Count(string SQLCommand, params object[] args)
        {
            try
            {
                var tmp = ExecuteReader(SQLCommand, args);
                int cnt = 0;
                while (tmp.Read())
                    cnt++;
                return cnt;
            }
            catch { return 0; }
        }

        #endregion

        #region SingleArray
        public string[] getSingleArray(string SQLCommand, params object[] args)
        {
            try
            {
                SQLCommand = ReplaceArgs(SQLCommand, args);
                using (SqlDataAdapter SqlAD = new SqlDataAdapter())
                using (SqlAD.SelectCommand = new SqlCommand(SQLCommand, m_Connection))
                {
                    DataSet ds = new DataSet();
                    SqlAD.Fill(ds);
                    DataTable dt = ds.Tables[0];
                    if (dt.Rows.Count != 0)
                    {
                        string[] arr = new string[dt.Rows[0].ItemArray.Length];

                        arr = InitStringArray(arr);


                        DataRow row = dt.Rows[0]; //first array

                        for (int i = 0; i < dt.Rows[0].ItemArray.Length; i++)
                        {
                            arr[i] = row[i].ToString();

                        }
                        return arr;
                    }
                }
            }
            catch (SqlException ex)
            {
                Logger.LogIt("getSingleArray: " + ex.ToString(), LogType.Hata);
            }
            return null;
        }

        public string[] InitStringArray(string[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = "";
            }
            return arr;
        }

        public int[] InitIntArray(int[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = 0;
            }
            return arr;
        }

        public bool[] InitBoolArray(bool[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = false;
            }
            return arr;
        }
        #endregion

        #region Private Methods

        private string ReplaceArgs(string SQLCommand, params object[] args)
        {
            for (int i = 0; i < args.Length; i++) args[i] = args[i].ToString().Replace("'", "''");
#if DEBUG
            Logger.LogIt(String.Format(SQLCommand, args), LogType.Query);
#endif
            return String.Format(SQLCommand, args);
        }

        #endregion
    }
}