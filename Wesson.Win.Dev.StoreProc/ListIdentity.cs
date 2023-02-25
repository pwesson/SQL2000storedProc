using System;
using System.Collections;  //for the ArrayList
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.IO;  //for file input-output

namespace Wesson.Win.Dev.GenStoredProcedures
{
    /// <summary>
    /// Summary description for TableIndices.
    /// </summary>
    public class ListIdentity
    {
        ///<summary>
        ///dsData
        ///</summary>
        public DataSet dsData;

        /// <summary>
        /// 
        /// </summary>
        public ListIdentity()
        {
             dsData = new DataSet();
        }

        ///<summary>
        ///aNumArray
        ///</summary>
        public long aNumArray
        {
            get
            {
                return dsData.Tables[0].Rows.Count;
            }
        }

        ///<summary>
        /// sDatastream
        ///</summary>
        public string asKeys(int index)
        {
            if (index >= 0 && index < aNumArray)
            {
                return System.Convert.ToString(this.dsData.Tables[0].Rows[index]["name"]);
            }
            else
            {
                return "";
            }
        }


        /// <summary>
        /// ListKeys
        /// </summary>
        /// <param name="sIndexName"></param>
        /// <param name="DatabaseName"></param>
        public void ListKeys(string sIndexName, string DatabaseName)
        {
            try
            {
                //Declare memory used

                SqlConnection dbConnection = null;
                SqlCommand dbCommand;
                //SqlParameter dbParam1;
                string sSQL;

                //Instantiate and open the connection

                dbConnection = GetConnection(DatabaseName);

                //Instantiate and initialise command

                sSQL = "select name from sysindexkeys inner join syscolumns on sysindexkeys.colid = syscolumns.colid where ";
                sSQL += "sysindexkeys.indid=(select indid from sysindexes where name='" + sIndexName + "') ";
                sSQL += "and sysindexkeys.id=(select id from sysindexes where name='" + sIndexName + "') ";
                sSQL += "and syscolumns.id=(select id from sysindexes where name='" + sIndexName + "') and colstat=1";

                dbCommand = new SqlCommand(sSQL, dbConnection);
                dbCommand.CommandType = CommandType.Text;

                //Instantiate, initialize and add parameter to command

                SqlDataAdapter sAdapter = new SqlDataAdapter(dbCommand);

                dsData.Clear();
                sAdapter.Fill(dsData);

                //Close connection to database

                dbConnection.Close();
            }
            catch (Exception e)
            {
                LogException(e);
            }
        }



        //----------------------------------------
        ///<summary>
        /// GetConnection
        ///</summary>
        private SqlConnection GetConnection(string DatabaseName)
        {
            try
            {
                SqlConnection connection = new SqlConnection("uid='TestOneUser';pwd='password'; DATABASE='" + DatabaseName + "'; SERVER='(local)'; ");
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                LogException(e);
                throw (e);
            }
        }

        //----------------------------------------
        ///<summary>
        /// LogException
        ///</summary>
        private void LogException(Exception ex)
        {
            string errMessage = "";
            errMessage = ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine;
            EventLog.WriteEntry("DAL", errMessage, EventLogEntryType.Error);
        }

    }
}
