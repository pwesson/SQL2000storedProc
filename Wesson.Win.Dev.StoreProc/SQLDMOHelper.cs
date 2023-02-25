using System;
using System.Collections;

namespace Wesson.Win.Dev.GenStoredProcedures
{
	/// <summary>
	/// Provides helper functions for SQLDMO
	/// </summary>
	public class SQLDMOHelper
	{
		public string ServerName;
		public string UserName;
		public string Password;
		public string Database;
		public string Table;

		/// <summary>
		/// SQLDMO.SQLServer Connection
		/// </summary>
		private SQLDMO.SQLServer Connection = new SQLDMO.SQLServerClass();

		/// <summary>
		/// Connects this.Connection
		/// </summary>
		public void Connect()
		{
            try
            {
                Connection.Connect("(local)", "TestOneUser", "Password10");
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
			
		}

		/// <summary>
		/// DisConnects this.Connection
		/// </summary>
		public void DisConnect()
		{
			Connection.DisConnect();
		}

		/// <summary>
		/// An array of Registered SQL Servers
		/// </summary>
		public Array RegisteredServers
		{
			get
			{
				ArrayList aServers = new ArrayList();
				SQLDMO.ApplicationClass acServers = new SQLDMO.ApplicationClass();

				for (int iServerGroupCount = 1; iServerGroupCount <= acServers.ServerGroups.Count; iServerGroupCount++)
					for (int iServerCount = 1; iServerCount <= acServers.ServerGroups.Item(iServerGroupCount).RegisteredServers.Count; iServerCount++)
						aServers.Add(acServers.ServerGroups.Item(iServerGroupCount).RegisteredServers.Item(iServerCount).Name);

				return aServers.ToArray();
			}
		}

		/// <summary>
		/// An array of Databases in a SQL Server
		/// </summary>
		public Array Databases
		{
			get
			{
				ArrayList aDatabases = new ArrayList();

				foreach(SQLDMO.Database dbCurrent in Connection.Databases)
					aDatabases.Add(dbCurrent.Name);

				return aDatabases.ToArray();
			}
		}

		/// <summary>
		/// Array of Tables in a Database
		/// </summary>
		public Array Tables
		{
			get
			{
				ArrayList aTables = new ArrayList();
				SQLDMO.Database dbCurrent = (SQLDMO.Database)Connection.Databases.Item(this.Database, Connection);

				foreach(SQLDMO.Table tblCurrent in dbCurrent.Tables)
					aTables.Add(tblCurrent.Name);
				
				return aTables.ToArray();
			}
		}

		/// <summary>
		/// A SQLDMO.Columns collection of Fields (Columns) in a Table
		/// </summary>
		public SQLDMO.Columns Fields
		{
			get
			{
				SQLDMO.Database dbCurrent = (SQLDMO.Database)Connection.Databases.Item(this.Database, Connection);
				SQLDMO.Table tblCurrent = (SQLDMO.Table)dbCurrent.Tables.Item(this.Table, Connection);

				return tblCurrent.Columns;
			}
		}

		public SQLDMO.Keys Keys
		{
			get
			{
				SQLDMO.Database dbCurrent = (SQLDMO.Database)Connection.Databases.Item(this.Database, Connection);
				SQLDMO.Table tblCurrent = (SQLDMO.Table)dbCurrent.Tables.Item(this.Table, Connection);

				return tblCurrent.Keys;
			}
		}

	}
}