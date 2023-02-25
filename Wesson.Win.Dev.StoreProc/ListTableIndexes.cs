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
	public class TableIndices
	{
		///<summary>
		///dsData
		///</summary>
		public DataSet dsData;

		/// <summary>
		/// 
		/// </summary>
		public TableIndices()
		{
			dsData =  new DataSet();
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
		public string asIndexes(int index)
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
		/// 
		/// </summary>
		/// <param name="sTableName"></param>
		/// <returns></returns>
		public string[] ListAllKeys( string sTableName, string DatabaseName)
		{
			TableIndices obj = new TableIndices();
			obj.ListIndices(sTableName, DatabaseName);

			//Have a list of the Keys & Indexes on the table
			//Loop over those

			ArrayList Keys = new ArrayList();

			for (int i=0; i<obj.aNumArray; i++)
			{
				ListIndexKeys obj2 = new ListIndexKeys();
				string sIndexName = obj.dsData.Tables[0].Rows[i]["name"].ToString();
				obj2.ListKeys( sIndexName, DatabaseName);
			
				//Loop over the keys
				for (int j=0; j<obj2.aNumArray; j++)
				{
					//Have seen before?
					int nFound = 0;
					for (int k=0; k<Keys.Count; k++)
					{
						if (Keys[k].ToString() == obj2.asKeys(j))
						{
							nFound = 1;
						}
					}
					if (nFound == 0)
					{
						Keys.Add(obj2.asKeys(j));
					}
				}

			}

			//Allocate memory

			string[] List = new string [Keys.Count];
			
			for (int i=0; i<Keys.Count; i++)
			{
				List[i] = Keys[i].ToString();
			}
			return List;
		}

        //List of Identity columns within Table
        public string[] ListIdentity(string sTableName, string DatabaseName)
        {
            TableIndices obj = new TableIndices();
            obj.ListIndices(sTableName, DatabaseName);

            //Have a list of the Keys & Indexes on the table
            //Loop over those

            ArrayList Keys = new ArrayList();

            for (int i = 0; i < obj.aNumArray; i++)
            {
                ListIdentity obj2 = new ListIdentity();
                string sIndexName = obj.dsData.Tables[0].Rows[i]["name"].ToString();
                obj2.ListKeys(sIndexName, DatabaseName);

                //Loop over the keys
                for (int j = 0; j < obj2.aNumArray; j++)
                {
                    //Have seen before?
                    int nFound = 0;
                    for (int k = 0; k < Keys.Count; k++)
                    {
                        if (Keys[k].ToString() == obj2.asKeys(j))
                        {
                            nFound = 1;
                        }
                    }
                    if (nFound == 0)
                    {
                        Keys.Add(obj2.asKeys(j));
                    }
                }

            }

            //Allocate memory

            string[] List = new string[Keys.Count];

            for (int i = 0; i < Keys.Count; i++)
            {
                List[i] = Keys[i].ToString();
            }
            return List;
        }


		/// <summary>
		/// IsIndex
		/// </summary>
		/// <param name="sKeyName"></param>
		/// <param name="List"></param>
		/// <returns></returns>
		public static bool IsIndex(string sKeyName, string[] List)
		{
			bool Found = false;

			for (int i=0; i<List.Length; i++)
			{
				if (sKeyName == List[i])
				{
					Found = true;
				}
			}
			return Found;
		}


		/// <summary>
		/// ListIndices
		/// </summary>
		/// <param name="sTableName"></param>
		/// <param name="DatabaseName"></param>
		public void ListIndices( string sTableName, string DatabaseName)
		{
			try
			{
				//Declare memory used

				SqlConnection dbConnection = null;
				SqlCommand dbCommand;
				string sSQL;

				//Instantiate and open the connection

				dbConnection = GetConnection(DatabaseName);

				sSQL  = "select name from sysindexes where ";
				sSQL += "id=(select id from sysobjects where xtype='u' and name='" + sTableName + "') ";
				sSQL += "and (name like 'PK%' or name like 'IX%')";

				//Instantiate and initialise command

				dbCommand = new SqlCommand(sSQL, dbConnection);
				dbCommand.CommandType = CommandType.Text;

				SqlDataAdapter sAdapter = new SqlDataAdapter(dbCommand);

				dsData.Clear();
				sAdapter.Fill(dsData); 
				
				//Close connection to database

				dbConnection.Close();
			}
			catch (Exception e)
			{
				LogException (e);
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
				LogException (e);
				throw (e);
			}
		}

		//----------------------------------------
		///<summary>
		/// LogException
		///</summary>
		private void LogException (Exception ex)
		{
			string errMessage = "";
			errMessage = ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine;
			EventLog.WriteEntry("DAL", errMessage, EventLogEntryType.Error);
		}

	}
}
