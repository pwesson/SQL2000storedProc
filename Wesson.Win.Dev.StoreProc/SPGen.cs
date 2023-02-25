using System;
using System.Collections;
using System.Text;
using System.Drawing;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;
using System.Configuration;

namespace Wesson.Win.Dev.GenStoredProcedures
{	
	/// <summary>
	/// Stored Procedure Helper class
	/// </summary>
	public class StoredProcedure
	{
        //==================================
        public bool IsDate(string DataType)
        {
            if (DataType == "datetime" || DataType == "date")
            {
                return true;
            }
            return false;
        }
        //==================================

		/// <summary>
		/// Generates code for an UPDATE or INSERT Stored Procedure
		/// </summary>
		/// <param name="sptypeGenerate">The type of SP to generate, INSERT or UPDATE</param>
		/// <param name="colsFields">A SQLDMO.Columns collection</param>
		/// <returns>The SP code</returns>
		public string GenerateIU(SQLDMO.Columns colsFields, string sTableName, string DatabaseName)
		{
			TableIndices obj = new TableIndices();
			string[] List = obj.ListAllKeys(sTableName, DatabaseName);

            string[] ListIdentity = obj.ListIdentity(sTableName, DatabaseName);

			int iCount;

			StringBuilder sStr = new StringBuilder();
			StringBuilder sStr2 = new StringBuilder();
			StringBuilder sStr3 = new StringBuilder();
			StringBuilder sWhere = new StringBuilder();
			StringBuilder sBody = new StringBuilder();			
			StringBuilder sINSERTValues = new StringBuilder();
			StringBuilder sGeneratedCode = new StringBuilder();
			StringBuilder sParamDeclaration = new StringBuilder();

			// Setup SP code, begining is the same no matter the type
			sGeneratedCode.Append("--============================================================");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("-- {0}",DateTime.Now.ToString());
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("-- INSERT/UPDATE stored procedure for TABLE {0}",sTableName);
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("-- P J Wesson");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("--============================================================");
			sGeneratedCode.Append(Environment.NewLine);

			sGeneratedCode.AppendFormat("CREATE PROCEDURE dbo.sp{0}INSERTUPDATE", new string[]{Trimtbl(sTableName)});			
			sGeneratedCode.Append(Environment.NewLine);

			foreach (SQLDMO.Column colCurrent in colsFields)
			{
				// Param Declaration construction
				sParamDeclaration.AppendFormat("	@var{0} {1}", new string[]{colCurrent.Name, colCurrent.Datatype});				
								
				// Only binary, char, nchar, nvarchar, varbinary and varchar may have their length declared								
				
				if (colCurrent.Datatype == "decimal")
				{
					sParamDeclaration.AppendFormat("({0},{1})", new string[]{colCurrent.NumericPrecision.ToString(),colCurrent.NumericScale.ToString()});
				}

				if (colCurrent.Datatype == "binary" || 
					colCurrent.Datatype == "char" || 
					colCurrent.Datatype == "nchar" || 
					colCurrent.Datatype == "nvarchar" || 
					colCurrent.Datatype == "varbinary" || 
					colCurrent.Datatype == "varchar")
				{
					sParamDeclaration.AppendFormat("({0})", colCurrent.Length);
				}
				if (colCurrent.Datatype == "varchar" ||
					colCurrent.Datatype == "char")
				{
					sParamDeclaration.Append(" = NULL");
				}
				sParamDeclaration.Append(",");
				sParamDeclaration.Append(Environment.NewLine);
			}
			sGeneratedCode.Append(sParamDeclaration.Remove(sParamDeclaration.Length - 3, 3));			
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("AS");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("BEGIN");
			sGeneratedCode.Append(Environment.NewLine);
			
			//-------- set up a newid if a uniqueidentifier is null
			
			foreach (SQLDMO.Column colCurrent in colsFields)
			{
				if (colCurrent.Datatype == "uniqueidentifier")
				{
					sGeneratedCode.AppendFormat("	IF (@var{0} = '00000000-0000-0000-0000-000000000000') SET @var{0} = NEWID()", new string[]{colCurrent.Name});				
					sGeneratedCode.Append(Environment.NewLine);
				}
			}
			sGeneratedCode.Append(Environment.NewLine);
			
			//--------
			
			sGeneratedCode.Append("	DECLARE @varCount AS INT");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("	SET @varCount = (SELECT COUNT(*) FROM {0}",sTableName);
			
			iCount = 0;
			sWhere.Append("	WHERE ");
			foreach (SQLDMO.Column colCurrent in colsFields)
			{
				if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colCurrent.Name, List))
				{
					sWhere.AppendFormat("[{0}] = @var{0} AND ",colCurrent.Name,colCurrent.Name);
					iCount = iCount + 1;
				}			
			}
			sGeneratedCode.Append(Environment.NewLine);
			if (iCount >0)
			{
				sGeneratedCode.Append(sWhere.Remove(sWhere.Length - 4, 4));
			}
			sGeneratedCode.Append(")");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append(Environment.NewLine);
			
			sGeneratedCode.Append("IF (@varCount = 0)");
			sGeneratedCode.Append(Environment.NewLine);			
			sGeneratedCode.Append("BEGIN");
			sGeneratedCode.Append(Environment.NewLine);

			sGeneratedCode.AppendFormat("	INSERT INTO {0}",sTableName);
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("	(");
			sGeneratedCode.Append(Environment.NewLine);

			foreach (SQLDMO.Column colCurrent in colsFields)
			{
               
                    if (!Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colCurrent.Name, ListIdentity))
                    {
                        sStr.AppendFormat("		[{0}],", colCurrent.Name);
                        sStr.Append(Environment.NewLine);
                    }
               
			}
			sGeneratedCode.Append(sStr.Remove(sStr.Length - 3, 3));

			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("	)");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("	VALUES");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("	(");
			sGeneratedCode.Append(Environment.NewLine);

			foreach (SQLDMO.Column colCurrent in colsFields)
			{
                
                    if (!Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colCurrent.Name, ListIdentity))
                    {
                        sStr2.AppendFormat("		@var{0},", colCurrent.Name);
                        sStr2.Append(Environment.NewLine);
                    }
               
			}
			sGeneratedCode.Append(sStr2.Remove(sStr2.Length - 3, 3));
			
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("	)");
			sGeneratedCode.Append(Environment.NewLine);

			sGeneratedCode.Append("END");
			sGeneratedCode.Append(Environment.NewLine);			
			sGeneratedCode.Append("ELSE");
			sGeneratedCode.Append(Environment.NewLine);			
			sGeneratedCode.Append("BEGIN");
			sGeneratedCode.Append(Environment.NewLine);			
			
			sGeneratedCode.AppendFormat("	UPDATE {0}",sTableName);
			sGeneratedCode.Append(Environment.NewLine);			
			
			sStr3.Append("	SET ");
			sStr3.Append(Environment.NewLine);	
			foreach (SQLDMO.Column colCurrent in colsFields)
			{
                if (!Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colCurrent.Name, List))
                {
                    if (!Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colCurrent.Name, ListIdentity))
                    {
                        sStr3.AppendFormat("		[{0}] = @var{1},", colCurrent.Name, colCurrent.Name);
                        sStr3.Append(Environment.NewLine);
                    }
                }
			}

			sGeneratedCode.Append(sStr3.Remove(sStr3.Length - 3, 3));

			sGeneratedCode.Append(Environment.NewLine);			
			sGeneratedCode.AppendFormat("	FROM {0}",sTableName);
			sGeneratedCode.Append(Environment.NewLine);			
			
			if (iCount >0)
			{
				sGeneratedCode.Append(	sWhere);
			}
			sGeneratedCode.Append(Environment.NewLine);	

			sGeneratedCode.Append("END");
			sGeneratedCode.Append(Environment.NewLine);			

			sGeneratedCode.Append("END");
			sGeneratedCode.Append(Environment.NewLine);			

			sGeneratedCode.Append("GO");
					
			return sGeneratedCode.ToString();
		}


		public string GenerateINSERT(SQLDMO.Columns colsFields, string sTableName, string DatabaseName)
		{
			TableIndices obj = new TableIndices();
			string[] List = obj.ListAllKeys(sTableName, DatabaseName);

            string[] ListIdentity = obj.ListIdentity(sTableName, DatabaseName);

			int iCount;

			StringBuilder sStr = new StringBuilder();
			StringBuilder sStr2 = new StringBuilder();
			StringBuilder sStr3 = new StringBuilder();
			StringBuilder sWhere = new StringBuilder();
			StringBuilder sBody = new StringBuilder();			
			StringBuilder sINSERTValues = new StringBuilder();
			StringBuilder sGeneratedCode = new StringBuilder();
			StringBuilder sParamDeclaration = new StringBuilder();

			// Setup SP code, begining is the same no matter the type
			sGeneratedCode.Append("--============================================================");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("-- {0}",DateTime.Now.ToString());
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("-- INSERT stored procedure for TABLE {0}",sTableName);
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("-- P J Wesson");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("--============================================================");
			sGeneratedCode.Append(Environment.NewLine);

			sGeneratedCode.AppendFormat("CREATE PROCEDURE dbo.sp{0}INSERT", new string[]{Trimtbl(sTableName)});			
			sGeneratedCode.Append(Environment.NewLine);


			foreach (SQLDMO.Column colCurrent in colsFields)
			{
				// Param Declaration construction
				sParamDeclaration.AppendFormat("	@var{0} {1}", new string[]{colCurrent.Name, colCurrent.Datatype});				
								
				// Only binary, char, nchar, nvarchar, varbinary and varchar may have their length declared								
				
				if (colCurrent.Datatype == "decimal")
				{
					sParamDeclaration.AppendFormat("({0},{1})", new string[]{colCurrent.NumericPrecision.ToString(),colCurrent.NumericScale.ToString()});
				}

				if (colCurrent.Datatype == "binary" || 
					colCurrent.Datatype == "char" || 
					colCurrent.Datatype == "nchar" || 
					colCurrent.Datatype == "nvarchar" || 
					colCurrent.Datatype == "varbinary" || 
					colCurrent.Datatype == "varchar")
				{
					sParamDeclaration.AppendFormat("({0})", colCurrent.Length);
				}
				if (colCurrent.Datatype == "varchar" ||
					colCurrent.Datatype == "char")
				{
					sParamDeclaration.Append(" = NULL");
				}
				sParamDeclaration.Append(",");
				sParamDeclaration.Append(Environment.NewLine);
			}
			sGeneratedCode.Append(sParamDeclaration.Remove(sParamDeclaration.Length - 3, 3));			
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("AS");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("BEGIN");
			sGeneratedCode.Append(Environment.NewLine);
			
			//-------- set up a newid if a uniqueidentifier is null
			
			foreach (SQLDMO.Column colCurrent in colsFields)
			{
				if (colCurrent.Datatype == "uniqueidentifier")
				{
					sGeneratedCode.AppendFormat("	IF (@var{0} = '00000000-0000-0000-0000-000000000000') SET @var{0} = NEWID()", new string[]{colCurrent.Name});				
					sGeneratedCode.Append(Environment.NewLine);
				}
			}
			sGeneratedCode.Append(Environment.NewLine);
			
			//--------

			iCount = 0;
			sWhere.Append(" WHERE ");
			foreach (SQLDMO.Column colCurrent in colsFields)
			{
                if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colCurrent.Name, List))
				{
					sWhere.AppendFormat("[{0}] = @var{0} AND ",colCurrent.Name,colCurrent.Name);
					iCount = iCount + 1;
				}			
			}
			
			sGeneratedCode.AppendFormat("	INSERT INTO {0}",sTableName);
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("	(");
			sGeneratedCode.Append(Environment.NewLine);

			foreach (SQLDMO.Column colCurrent in colsFields)
			{
                if (!Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colCurrent.Name, ListIdentity))
                {
                    sStr.AppendFormat("		[{0}],", colCurrent.Name);
                    sStr.Append(Environment.NewLine);
                }
			}
			sGeneratedCode.Append(sStr.Remove(sStr.Length - 3, 3));

			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("	)");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("	VALUES");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("	(");
			sGeneratedCode.Append(Environment.NewLine);

			foreach (SQLDMO.Column colCurrent in colsFields)
			{
                if (!Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colCurrent.Name, ListIdentity))
                {
                    sStr2.AppendFormat("		@var{0},", colCurrent.Name);
                    sStr2.Append(Environment.NewLine);
                }
			}
			sGeneratedCode.Append(sStr2.Remove(sStr2.Length - 3, 3));
			
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("	)");
			sGeneratedCode.Append(Environment.NewLine);

			sGeneratedCode.Append("END");
			sGeneratedCode.Append(Environment.NewLine);			

			sGeneratedCode.Append("GO");
					
			return sGeneratedCode.ToString();
		}


		public string GenerateUPDATE(SQLDMO.Columns colsFields, string sTableName, string DatabaseName)
		{
			TableIndices obj = new TableIndices();
			string[] List = obj.ListAllKeys(sTableName, DatabaseName);

            string[] ListIdentity = obj.ListIdentity(sTableName, DatabaseName);

			int iCount;

			StringBuilder sStr = new StringBuilder();
			StringBuilder sStr2 = new StringBuilder();
			StringBuilder sStr3 = new StringBuilder();
			StringBuilder sWhere = new StringBuilder();
			StringBuilder sBody = new StringBuilder();			
			StringBuilder sINSERTValues = new StringBuilder();
			StringBuilder sGeneratedCode = new StringBuilder();
			StringBuilder sParamDeclaration = new StringBuilder();

			// Setup SP code, begining is the same no matter the type
			sGeneratedCode.Append("--============================================================");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("-- {0}",DateTime.Now.ToString());
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("-- UPDATE stored procedure for TABLE {0}",sTableName);
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("-- P J Wesson");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("--============================================================");
			sGeneratedCode.Append(Environment.NewLine);

			sGeneratedCode.AppendFormat("CREATE PROCEDURE dbo.sp{0}UPDATE", new string[]{Trimtbl(sTableName)});			
			sGeneratedCode.Append(Environment.NewLine);


			foreach (SQLDMO.Column colCurrent in colsFields)
			{
				// Param Declaration construction
				sParamDeclaration.AppendFormat("	@var{0} {1}", new string[]{colCurrent.Name, colCurrent.Datatype});				
								
				// Only binary, char, nchar, nvarchar, varbinary and varchar may have their length declared								
				
				if (colCurrent.Datatype == "decimal")
				{
					sParamDeclaration.AppendFormat("({0},{1})", new string[]{colCurrent.NumericPrecision.ToString(),colCurrent.NumericScale.ToString()});
				}

				if (colCurrent.Datatype == "binary" || 
					colCurrent.Datatype == "char" || 
					colCurrent.Datatype == "nchar" || 
					colCurrent.Datatype == "nvarchar" || 
					colCurrent.Datatype == "varbinary" || 
					colCurrent.Datatype == "varchar")
				{
					sParamDeclaration.AppendFormat("({0})", colCurrent.Length);
				}
				if (colCurrent.Datatype == "varchar" ||
					colCurrent.Datatype == "char")
				{
					sParamDeclaration.Append(" = NULL");
				}
				sParamDeclaration.Append(",");
				sParamDeclaration.Append(Environment.NewLine);
			}
			sGeneratedCode.Append(sParamDeclaration.Remove(sParamDeclaration.Length - 3, 3));			
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("AS");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("BEGIN");
			sGeneratedCode.Append(Environment.NewLine);
			
			iCount = 0;
			sWhere.Append("	WHERE ");
			foreach (SQLDMO.Column colCurrent in colsFields)
			{
                if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colCurrent.Name, List))
				{
					sWhere.AppendFormat("[{0}] = @var{0} AND ",colCurrent.Name,colCurrent.Name);
					iCount = iCount + 1;
				}			
			}
					
			sGeneratedCode.AppendFormat("	UPDATE {0}",sTableName);
			sGeneratedCode.Append(Environment.NewLine);			
			
			sStr3.Append("	SET ");
			sStr3.Append(Environment.NewLine);	
			foreach (SQLDMO.Column colCurrent in colsFields)
			{
                if (!Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colCurrent.Name, ListIdentity))
                {
                    sStr3.AppendFormat("		[{0}] = @var{1},", colCurrent.Name, colCurrent.Name);
                    sStr3.Append(Environment.NewLine);
                }
			}

			sGeneratedCode.Append(sStr3.Remove(sStr3.Length - 3, 3));

			sGeneratedCode.Append(Environment.NewLine);			
			sGeneratedCode.AppendFormat("	FROM {0}",sTableName);
			sGeneratedCode.Append(Environment.NewLine);			
			
			if (iCount >0)
			{
				sGeneratedCode.Append(sWhere.Remove(sWhere.Length - 4, 4));
			}
			sGeneratedCode.Append(Environment.NewLine);	

			sGeneratedCode.Append("END");
			sGeneratedCode.Append(Environment.NewLine);			

			sGeneratedCode.Append("GO");
					
			return sGeneratedCode.ToString();
		}


		//-------------------------------------------------------------------------------
		public string GenerateDELETE(SQLDMO.Columns colsFields, string sTableName, string DatabaseName)
		{
			TableIndices obj = new TableIndices();
			string[] List = obj.ListAllKeys(sTableName, DatabaseName);

			int iCount;

			StringBuilder sStr = new StringBuilder();
			StringBuilder sStr2 = new StringBuilder();
			StringBuilder sStr3 = new StringBuilder();
			StringBuilder sWhere = new StringBuilder();
			StringBuilder sBody = new StringBuilder();			
			StringBuilder sINSERTValues = new StringBuilder();
			StringBuilder sGeneratedCode = new StringBuilder();
			StringBuilder sParamDeclaration = new StringBuilder();

			// Setup SP code, begining is the same no matter the type
			sGeneratedCode.Append("--============================================================");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("-- {0}",DateTime.Now.ToString());
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("-- DELETE stored procedure for TABLE {0}",sTableName);
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("-- P J Wesson");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("--============================================================");
			sGeneratedCode.Append(Environment.NewLine);

			sGeneratedCode.AppendFormat("CREATE PROCEDURE dbo.sp{0}DELETE", new string[]{Trimtbl(sTableName)});			
			
			foreach (SQLDMO.Column colCurrent in colsFields)
			{
                if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colCurrent.Name, List))
				{
					sGeneratedCode.AppendFormat("By{0}",colCurrent.Name);	
				}			
			}

			sGeneratedCode.Append(Environment.NewLine);

			iCount = 0;
			foreach (SQLDMO.Column colCurrent in colsFields)
			{
				//if (colCurrent.InPrimaryKey)
                if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colCurrent.Name, List))
				{
					// Param Declaration construction
					sParamDeclaration.AppendFormat("	@var{0} {1}", new string[]{colCurrent.Name, colCurrent.Datatype});				
					iCount = iCount+1;
			
					// Only binary, char, nchar, nvarchar, varbinary and varchar may have their length declared								
				
					if (colCurrent.Datatype == "decimal")
					{
						sParamDeclaration.AppendFormat("({0},{1})", new string[]{colCurrent.NumericPrecision.ToString(),colCurrent.NumericScale.ToString()});
					}

					if (colCurrent.Datatype == "binary" || 
						colCurrent.Datatype == "char" || 
						colCurrent.Datatype == "nchar" || 
						colCurrent.Datatype == "nvarchar" || 
						colCurrent.Datatype == "varbinary" || 
						colCurrent.Datatype == "varchar")
					{
						sParamDeclaration.AppendFormat("({0})", colCurrent.Length);
					}
					if (colCurrent.Datatype == "varchar" ||
						colCurrent.Datatype == "char")
					{
						sParamDeclaration.Append(" = NULL");
					}
					sParamDeclaration.Append(",");
					sParamDeclaration.Append(Environment.NewLine);
			
				}
			}
			if (iCount >0)
			{
				sGeneratedCode.Append(sParamDeclaration.Remove(sParamDeclaration.Length - 3, 3));			
			}
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("AS");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("BEGIN");
			sGeneratedCode.Append(Environment.NewLine);
			
			iCount = 0;
			sWhere.Append("	WHERE ");
			foreach (SQLDMO.Column colCurrent in colsFields)
			{
                if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colCurrent.Name, List))
				{
					sWhere.AppendFormat("[{0}] = @var{0} AND ",colCurrent.Name,colCurrent.Name);
					iCount = iCount + 1;
				}			
			}
					
			sGeneratedCode.AppendFormat("	DELETE FROM {0}",sTableName);
			sGeneratedCode.Append(Environment.NewLine);			
			
			if (iCount >0)
			{
				sGeneratedCode.Append(sWhere.Remove(sWhere.Length - 4, 4));
			}
			sGeneratedCode.Append(Environment.NewLine);	

			sGeneratedCode.Append("END");
			sGeneratedCode.Append(Environment.NewLine);			

			sGeneratedCode.Append("GO");
					
			return sGeneratedCode.ToString();
		}

		//-------------------------------------------------------------------------------
		public string GenerateSELECT(SQLDMO.Columns colsFields, string sTableName, string DatabaseName)
		{
			int iCount;

			StringBuilder sStr = new StringBuilder();
			StringBuilder sStr2 = new StringBuilder();
			StringBuilder sStr3 = new StringBuilder();
			StringBuilder sWhere = new StringBuilder();
			StringBuilder sBody = new StringBuilder();			
			StringBuilder sINSERTValues = new StringBuilder();
			StringBuilder sGeneratedCode = new StringBuilder();
			StringBuilder sParamDeclaration = new StringBuilder();

			//---------
			TableIndices obj = new TableIndices();
			string[] List = obj.ListAllKeys(sTableName, DatabaseName);

			// Setup SP code, begining is the same no matter the type
			sGeneratedCode.Append("--============================================================");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("-- {0}",DateTime.Now.ToString());
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("-- SELECT stored procedure for TABLE {0}",sTableName);
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("-- P J Wesson");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("--============================================================");
			sGeneratedCode.Append(Environment.NewLine);

			sGeneratedCode.AppendFormat("CREATE PROCEDURE dbo.sp{0}SELECT", new string[]{Trimtbl(sTableName)});			
			
			foreach (SQLDMO.Column colCurrent in colsFields)
			{
                if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colCurrent.Name, List))
				{
					sGeneratedCode.AppendFormat("By{0}",colCurrent.Name);	
				}			
			}

			sGeneratedCode.Append(Environment.NewLine);

			iCount = 0;
			foreach (SQLDMO.Column colCurrent in colsFields)
			{	
				//if (colCurrent.InPrimaryKey)
                if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colCurrent.Name, List))
				{
					// Param Declaration construction
					sParamDeclaration.AppendFormat("	@var{0} {1}", new string[]{colCurrent.Name, colCurrent.Datatype});				
					iCount = iCount+1;
					
					// Only binary, char, nchar, nvarchar, varbinary and varchar may have their length declared								
				
					if (colCurrent.Datatype == "decimal")
					{
						sParamDeclaration.AppendFormat("({0},{1})", new string[]{colCurrent.NumericPrecision.ToString(),colCurrent.NumericScale.ToString()});
					}

					if (colCurrent.Datatype == "binary" || 
						colCurrent.Datatype == "char" || 
						colCurrent.Datatype == "nchar" || 
						colCurrent.Datatype == "nvarchar" || 
						colCurrent.Datatype == "varbinary" || 
						colCurrent.Datatype == "varchar")
					{
						sParamDeclaration.AppendFormat("({0})", colCurrent.Length);
					}
					if (colCurrent.Datatype == "varchar" ||
						colCurrent.Datatype == "char")
					{
						sParamDeclaration.Append(" = NULL");
					}
					sParamDeclaration.Append(",");
					sParamDeclaration.Append(Environment.NewLine);
			
				}
			}
			if (iCount >0)
			{
				sGeneratedCode.Append(sParamDeclaration.Remove(sParamDeclaration.Length - 3, 3));			
			}
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("AS");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("BEGIN");
			sGeneratedCode.Append(Environment.NewLine);
			
			iCount = 0;
			sWhere.Append("	WHERE ");
			foreach (SQLDMO.Column colCurrent in colsFields)
			{
                if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colCurrent.Name, List))
				{
					sWhere.AppendFormat("[{0}] = @var{0} AND ",colCurrent.Name,colCurrent.Name);
					iCount = iCount + 1;
				}			
			}
					
			//===============================
            //before
            //sGeneratedCode.AppendFormat("	SELECT * FROM {0}",sTableName);

            //after
            sGeneratedCode.AppendFormat("	SELECT {0} FROM {1}", StarString(colsFields), sTableName);

            //===============================
            
            sGeneratedCode.Append(Environment.NewLine);			
			
			if (iCount >0) 
			{
				sGeneratedCode.Append(sWhere.Remove(sWhere.Length - 4, 4));
			}
			sGeneratedCode.Append(Environment.NewLine);	

			sGeneratedCode.Append("END");
			sGeneratedCode.Append(Environment.NewLine);			

			sGeneratedCode.Append("GO");
					
			return sGeneratedCode.ToString();
		}

		//-------------------------------------------------------------------------------
		public string GenerateSELECTALL(SQLDMO.Columns colsFields, string sTableName, string DatastreamName)
		{
			//int iCount;

			StringBuilder sStr = new StringBuilder();
			StringBuilder sStr2 = new StringBuilder();
			StringBuilder sStr3 = new StringBuilder();
			StringBuilder sWhere = new StringBuilder();
			StringBuilder sBody = new StringBuilder();			
			StringBuilder sINSERTValues = new StringBuilder();
			StringBuilder sGeneratedCode = new StringBuilder();
			StringBuilder sParamDeclaration = new StringBuilder();

			// Setup SP code, begining is the same no matter the type
			sGeneratedCode.Append("--============================================================");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("-- {0}",DateTime.Now.ToString());
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("-- SELECT ALL stored procedure for TABLE {0}",sTableName);
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("-- P J Wesson");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("--============================================================");
			sGeneratedCode.Append(Environment.NewLine);

			sGeneratedCode.AppendFormat("CREATE PROCEDURE dbo.sp{0}SELECTALL", new string[]{Trimtbl(sTableName)});			
			
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("AS");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("BEGIN");
			sGeneratedCode.Append(Environment.NewLine);
			
            //=========================
			//sGeneratedCode.AppendFormat("	SELECT * FROM {0}",sTableName);
            sGeneratedCode.AppendFormat("	SELECT {0} FROM {1}", StarString(colsFields), sTableName);

			sGeneratedCode.Append(Environment.NewLine);			
			

			sGeneratedCode.Append("END");
			sGeneratedCode.Append(Environment.NewLine);			

			sGeneratedCode.Append("GO");
					
			return sGeneratedCode.ToString();
		}

		//-------------------------------------------------------------------------------
		public string GenerateSELECTINDIVIUALS(SQLDMO.Columns colsFields, string sTableName, string DatabaseName)
		{
			TableIndices obj = new TableIndices();
			string[] List = obj.ListAllKeys(sTableName, DatabaseName);

			int iCount = 0;
			StringBuilder sGeneratedCode = new StringBuilder();

			foreach (SQLDMO.Column colKey in colsFields)
			{
                if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colKey.Name, List))
				{
					iCount = iCount + 1;
				}
			}

			//Only worth this is there are at least two keys
			
			if (iCount >= 2)
			{
				foreach (SQLDMO.Column colKey in colsFields)
				{
                    if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colKey.Name, List))
					{
						sGeneratedCode.Append( GenerateSub(colKey, sTableName, colsFields) );
						sGeneratedCode.Append(Environment.NewLine);
					}
				}

				//Three extra stored procedures when DateTime variables are involved
				
				foreach (SQLDMO.Column colKey in colsFields)
				{
                    if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colKey.Name, List)
								&& !IsDate(colKey.Datatype))
					{
						foreach (SQLDMO.Column colKey2 in colsFields)
						{
							if (colKey.Name != colKey2.Name &&
                                Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colKey2.Name, List) &&
								IsDate(colKey2.Datatype))
							{
                                sGeneratedCode.Append(GenerateSubExtra(colKey, colKey2, sTableName, 1, colsFields));
								sGeneratedCode.Append(Environment.NewLine);

                                sGeneratedCode.Append(GenerateSubExtra(colKey, colKey2, sTableName, 2, colsFields));
								sGeneratedCode.Append(Environment.NewLine);

                                sGeneratedCode.Append(GenerateSubExtra(colKey, colKey2, sTableName, 3, colsFields));
								sGeneratedCode.Append(Environment.NewLine);

                                sGeneratedCode.Append(GenerateSubExtra(colKey, colKey2, sTableName, 4, colsFields));
								sGeneratedCode.Append(Environment.NewLine);

                                sGeneratedCode.Append(GenerateSubExtra(colKey, colKey2, sTableName, 5, colsFields));
								sGeneratedCode.Append(Environment.NewLine);
							}
						}
					}
				}				
			}

			return sGeneratedCode.ToString();
		}


		//-------------------------------------------------------------------------------
		public string GenerateSELECTDistinct(SQLDMO.Columns colsFields, string sTableName, string DatabaseName)
		{
			TableIndices obj = new TableIndices();
			string[] List = obj.ListAllKeys(sTableName, DatabaseName);

			StringBuilder sGeneratedCode = new StringBuilder();
			StringBuilder sParamDeclaration = new StringBuilder();

			foreach (SQLDMO.Column colKey in colsFields)
			{
                if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colKey.Name, List))
				{
					// Setup SP code, begining is the same no matter the type
				
					sGeneratedCode.Append("--============================================================");
					sGeneratedCode.Append(Environment.NewLine);
					sGeneratedCode.AppendFormat("-- {0}",DateTime.Now.ToString());
					sGeneratedCode.Append(Environment.NewLine);
					sGeneratedCode.AppendFormat("-- SELECT stored procedure for TABLE {0}",sTableName);
					sGeneratedCode.Append(Environment.NewLine);
					sGeneratedCode.Append("-- P J Wesson");
					sGeneratedCode.Append(Environment.NewLine);
					sGeneratedCode.Append("--============================================================");
					sGeneratedCode.Append(Environment.NewLine);

					sGeneratedCode.AppendFormat("CREATE PROCEDURE dbo.sp{0}SELECTDistinct{1}", new string[]{Trimtbl(sTableName),colKey.Name});			
					sGeneratedCode.Append(Environment.NewLine);

					sGeneratedCode.Append("AS");
					sGeneratedCode.Append(Environment.NewLine);
					sGeneratedCode.Append("BEGIN");
					sGeneratedCode.Append(Environment.NewLine);
			
					sGeneratedCode.AppendFormat("	SELECT DISTINCT [{0}] FROM {1}",new string[]{colKey.Name,sTableName});
					sGeneratedCode.Append(Environment.NewLine);			
			
					sGeneratedCode.AppendFormat("	ORDER BY [{0}] ASC",colKey.Name);
					sGeneratedCode.Append(Environment.NewLine);	
					
					sGeneratedCode.Append("END");
					sGeneratedCode.Append(Environment.NewLine);			

					sGeneratedCode.Append("GO");
					sGeneratedCode.Append(Environment.NewLine);			
				}
			}
			return sGeneratedCode.ToString();
		}


		//-------------------------------------------------------------------------------
        public string GenerateSub(SQLDMO.Column colKey, string sTableName, SQLDMO.Columns colsFields)
		{
			StringBuilder sStr = new StringBuilder();
			StringBuilder sStr2 = new StringBuilder();
			StringBuilder sStr3 = new StringBuilder();
			StringBuilder sWhere = new StringBuilder();
			StringBuilder sBody = new StringBuilder();			
			StringBuilder sINSERTValues = new StringBuilder();
			StringBuilder sGeneratedCode = new StringBuilder();
			StringBuilder sParamDeclaration = new StringBuilder();

			// Setup SP code, begining is the same no matter the type
			sGeneratedCode.Append("--============================================================");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("-- {0}",DateTime.Now.ToString());
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("-- SELECT stored procedure for TABLE {0}",sTableName);
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("-- P J Wesson");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("--============================================================");
			sGeneratedCode.Append(Environment.NewLine);

			sGeneratedCode.AppendFormat("CREATE PROCEDURE dbo.sp{0}SELECTBy{1}", new string[]{Trimtbl(sTableName),colKey.Name});			
				
			sGeneratedCode.Append(Environment.NewLine);

			// Param Declaration construction
			sParamDeclaration.AppendFormat("	@var{0} {1}", new string[]{colKey.Name, colKey.Datatype});				
			
			// Only binary, char, nchar, nvarchar, varbinary and varchar may have their length declared								
			
			if (colKey.Datatype == "decimal")
			{
				sParamDeclaration.AppendFormat("({0},{1})", new string[]{colKey.NumericPrecision.ToString(),colKey.NumericScale.ToString()});
			}

			if (colKey.Datatype == "binary" || 
					colKey.Datatype == "char" || 
					colKey.Datatype == "nchar" || 
					colKey.Datatype == "nvarchar" || 
					colKey.Datatype == "varbinary" || 
					colKey.Datatype == "varchar")
			{
				sParamDeclaration.AppendFormat("({0})", colKey.Length);
			}
					
			if (colKey.Datatype == "varchar" ||
						colKey.Datatype == "char")
			{
				sParamDeclaration.Append(" = NULL");
			}
			sParamDeclaration.Append(",");
			sParamDeclaration.Append(Environment.NewLine);
			
			sGeneratedCode.Append(sParamDeclaration.Remove(sParamDeclaration.Length - 3, 3));			
			
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("AS");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("BEGIN");
			sGeneratedCode.Append(Environment.NewLine);
			
			sWhere.Append("	WHERE ");
			sWhere.AppendFormat("[{0}] = @var{0} AND ",colKey.Name,colKey.Name);
			
		    //==================
			//sGeneratedCode.AppendFormat("	SELECT * FROM {0}",sTableName);
            sGeneratedCode.AppendFormat("	SELECT {0} FROM {1}", StarString(colsFields), sTableName);
            
            
            sGeneratedCode.Append(Environment.NewLine);			
			
			sGeneratedCode.Append(sWhere.Remove(sWhere.Length - 4, 4));
			
			sGeneratedCode.Append(Environment.NewLine);	

			sGeneratedCode.Append("END");
			sGeneratedCode.Append(Environment.NewLine);			

			sGeneratedCode.Append("GO");
			

			return sGeneratedCode.ToString();
		}


		//-------------------------------------------------------------------------------
        public string GenerateSubExtra(SQLDMO.Column colKey, SQLDMO.Column colKey2, string sTableName, int iChoice, SQLDMO.Columns colsFields)
		{
			StringBuilder sStr = new StringBuilder();
			StringBuilder sStr2 = new StringBuilder();
			StringBuilder sStr3 = new StringBuilder();
			StringBuilder sWhere = new StringBuilder();
			StringBuilder sBody = new StringBuilder();			
			StringBuilder sINSERTValues = new StringBuilder();
			StringBuilder sGeneratedCode = new StringBuilder();
			StringBuilder sParamDeclaration = new StringBuilder();

			// Setup SP code, begining is the same no matter the type
			sGeneratedCode.Append("--============================================================");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("-- {0}",DateTime.Now.ToString());
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("-- SELECT stored procedure for TABLE {0}",sTableName);
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("-- P J Wesson");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("--============================================================");
			sGeneratedCode.Append(Environment.NewLine);

			sGeneratedCode.AppendFormat("CREATE PROCEDURE dbo.sp{0}SELECTBy{1}", new string[]{Trimtbl(sTableName),colKey.Name});			
			if (iChoice == 1) sGeneratedCode.AppendFormat("Max{0}",colKey2.Name);
			if (iChoice == 2) sGeneratedCode.AppendFormat("Between{0}ASC",colKey2.Name);
			if (iChoice == 3) sGeneratedCode.AppendFormat("OrderBy{0}ASC",colKey2.Name);
			if (iChoice == 4) sGeneratedCode.AppendFormat("DistinctBy{0}ASC",colKey2.Name);
			if (iChoice == 5) sGeneratedCode.AppendFormat("Min{0}",colKey2.Name);
			if (iChoice == 6) sGeneratedCode.AppendFormat("Between{0}Sum",colKey2.Name);
			
			sGeneratedCode.Append(Environment.NewLine);

			// Param Declaration construction
			sParamDeclaration.AppendFormat("	@var{0} {1}", new string[]{colKey.Name, colKey.Datatype});				

			// Only binary, char, nchar, nvarchar, varbinary and varchar may have their length declared								
			
			if (colKey.Datatype == "decimal")
			{
				sParamDeclaration.AppendFormat("({0},{1})", new string[]{colKey.NumericPrecision.ToString(),colKey.NumericScale.ToString()});
			}

			if (colKey.Datatype == "binary" || 
				colKey.Datatype == "char" || 
				colKey.Datatype == "nchar" || 
				colKey.Datatype == "nvarchar" || 
				colKey.Datatype == "varbinary" || 
				colKey.Datatype == "varchar")
			{
				sParamDeclaration.AppendFormat("({0})", colKey.Length);
			}
					
			if (colKey.Datatype == "varchar" ||
				colKey.Datatype == "char")
			{
				sParamDeclaration.Append(" = NULL");
			}
			sParamDeclaration.Append(",");
			sParamDeclaration.Append(Environment.NewLine);

			if (iChoice == 2)
			{
				sParamDeclaration.AppendFormat("	@var{0}Start {1},", new string[]{colKey2.Name, colKey2.Datatype});				
				sParamDeclaration.Append(Environment.NewLine);
				sParamDeclaration.AppendFormat("	@var{0}End {1},", new string[]{colKey2.Name, colKey2.Datatype});				
				sParamDeclaration.Append(Environment.NewLine);
			}

			sGeneratedCode.Append(sParamDeclaration.Remove(sParamDeclaration.Length - 3, 3));				
			sGeneratedCode.Append(Environment.NewLine);

			sGeneratedCode.Append("AS");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("BEGIN");
			sGeneratedCode.Append(Environment.NewLine);
			
			sWhere.Append("	WHERE ");
			sWhere.AppendFormat("[{0}] = @var{0} AND ",colKey.Name,colKey.Name);
			
			if (iChoice == 1)
			{
				sGeneratedCode.AppendFormat("	SELECT max( [{0}] ) AS {0} FROM {1}",new string[]{colKey2.Name,sTableName});
			}
			if (iChoice == 2)
			{
                //=================
				//sGeneratedCode.AppendFormat("	SELECT * FROM {0}",sTableName);
                sGeneratedCode.AppendFormat("	SELECT {0} FROM {1}", StarString(colsFields), sTableName);

			}
			if (iChoice == 3)
			{
				//sGeneratedCode.AppendFormat("	SELECT * FROM {0}",sTableName);
                sGeneratedCode.AppendFormat("	SELECT {0} FROM {1}", StarString(colsFields), sTableName);
			}
			if (iChoice == 4)
			{
				sGeneratedCode.AppendFormat("	SELECT DISTINCT {0} FROM {1}",new string[]{colKey2.Name,sTableName});
			}
			if (iChoice == 5)
			{
				sGeneratedCode.AppendFormat("	SELECT min( [{0}] ) AS {0} FROM {1}",new string[]{colKey2.Name,sTableName});
			}

			sGeneratedCode.Append(Environment.NewLine);			
			
			sGeneratedCode.Append(sWhere.Remove(sWhere.Length - 4, 4));

			if (iChoice == 2)
			{
				sGeneratedCode.AppendFormat(" AND [{0}] >= @var{0}Start AND [{0}] <= @var{0}End",colKey2.Name);
			}
			sGeneratedCode.Append(Environment.NewLine);	

			if (iChoice == 2 || iChoice == 3)
			{
				sGeneratedCode.AppendFormat("	ORDER BY [{0}] ASC",colKey2.Name);
				sGeneratedCode.Append(Environment.NewLine);	
			}

			if (iChoice ==4)
			{
				sGeneratedCode.AppendFormat("	ORDER BY [{0}] ASC",colKey2.Name);
				sGeneratedCode.Append(Environment.NewLine);	
			}

			sGeneratedCode.Append("END");
			sGeneratedCode.Append(Environment.NewLine);			

			sGeneratedCode.Append("GO");
			

			return sGeneratedCode.ToString();
		}


		//-------------------------------------------------------------------------------
		public string GenerateSELECTPairIndividuals(SQLDMO.Columns colsFields, string sTableName, string DatabaseName)
		{
			TableIndices obj = new TableIndices();
			string[] List = obj.ListAllKeys(sTableName, DatabaseName);

			int iCount = 0;
			int jCount = 0;
			int nCount = 0;
			StringBuilder sGeneratedCode = new StringBuilder();

			foreach (SQLDMO.Column colKey in colsFields)
			{
                if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colKey.Name, List))
				{
					nCount = nCount + 1;
				}
			}

			//Only worth this is there are at least three keys
			if (nCount >= 3)
			{
				foreach (SQLDMO.Column colKey1 in colsFields)
				{
                    if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colKey1.Name, List))
					{
						iCount = iCount +1;
						jCount = 0;
						foreach (SQLDMO.Column colKey2 in colsFields)
						{
                            if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colKey2.Name, List))
							{
								jCount = jCount+1;
								if (jCount > iCount)
								{
                                    sGeneratedCode.Append(GeneratePairSub(colKey1, colKey2, sTableName, colsFields));
									sGeneratedCode.Append(Environment.NewLine);
								}
							}
						}
					}

				}
			}

			return sGeneratedCode.ToString();
		}

		//-------------------------------------------------------------------------------
        public string GeneratePairSub(SQLDMO.Column colKey1, SQLDMO.Column colKey2, string sTableName, SQLDMO.Columns colsFields)
		{
			StringBuilder sStr = new StringBuilder();
			StringBuilder sStr2 = new StringBuilder();
			StringBuilder sStr3 = new StringBuilder();
			StringBuilder sWhere = new StringBuilder();
			StringBuilder sBody = new StringBuilder();			
			StringBuilder sINSERTValues = new StringBuilder();
			StringBuilder sGeneratedCode = new StringBuilder();
			StringBuilder sParamDeclaration = new StringBuilder();

			// Setup SP code, begining is the same no matter the type
			sGeneratedCode.Append("--============================================================");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("-- {0}",DateTime.Now.ToString());
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.AppendFormat("-- SELECT stored procedure for TABLE {0}",sTableName);
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("-- P J Wesson");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("--============================================================");
			sGeneratedCode.Append(Environment.NewLine);

			sGeneratedCode.AppendFormat("CREATE PROCEDURE dbo.sp{0}SELECTBy{1}By{2}", new string[]{Trimtbl(sTableName),colKey1.Name,colKey2.Name});			
				
			sGeneratedCode.Append(Environment.NewLine);

			// Param Declaration construction
			sParamDeclaration.AppendFormat("	@var{0} {1}", new string[]{colKey1.Name, colKey1.Datatype});				
			
			if (colKey1.Datatype == "decimal")
			{
				sParamDeclaration.AppendFormat("({0},{1})", new string[]{colKey1.NumericPrecision.ToString(),colKey1.NumericScale.ToString()});
			}
			
			if (colKey1.Datatype == "binary" || 
				colKey1.Datatype == "char" || 
				colKey1.Datatype == "nchar" || 
				colKey1.Datatype == "nvarchar" || 
				colKey1.Datatype == "varbinary" || 
				colKey1.Datatype == "varchar")
			{
				sParamDeclaration.AppendFormat("({0})", colKey1.Length);
			}
			if (colKey1.Datatype == "varchar" ||
				colKey1.Datatype == "char")
			{
				sParamDeclaration.Append(" = NULL");
			}
			sParamDeclaration.Append(",");
			sParamDeclaration.Append(Environment.NewLine);
			

			sParamDeclaration.AppendFormat("	@var{0} {1}", new string[]{colKey2.Name, colKey2.Datatype});				
			
			if (colKey2.Datatype == "decimal")
			{
				sParamDeclaration.AppendFormat("({0},{1})", new string[]{colKey2.NumericPrecision.ToString(),colKey2.NumericScale.ToString()});
			}

			if (colKey2.Datatype == "binary" || 
				colKey2.Datatype == "char" || 
				colKey2.Datatype == "nchar" || 
				colKey2.Datatype == "nvarchar" || 
				colKey2.Datatype == "varbinary" || 
				colKey2.Datatype == "varchar")
			{
				sParamDeclaration.AppendFormat("({0})", colKey2.Length);
			}
			if (colKey2.Datatype == "varchar" ||
				colKey2.Datatype == "char")
			{
				sParamDeclaration.Append(" = NULL");
			}
			sParamDeclaration.Append(",");
			sParamDeclaration.Append(Environment.NewLine);
			
			sGeneratedCode.Append(sParamDeclaration.Remove(sParamDeclaration.Length - 3, 3));			
			
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("AS");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("BEGIN");
			sGeneratedCode.Append(Environment.NewLine);
			
			sWhere.Append("	WHERE ");
			sWhere.AppendFormat("[{0}] = @var{0} AND ",colKey1.Name,colKey1.Name);
			sWhere.AppendFormat("[{0}] = @var{0} AND ",colKey2.Name,colKey2.Name);
			
		    //================
			//sGeneratedCode.AppendFormat("	SELECT * FROM {0}",sTableName);
            sGeneratedCode.AppendFormat("	SELECT {0} FROM {1}", StarString(colsFields), sTableName);

			sGeneratedCode.Append(Environment.NewLine);			
			sGeneratedCode.Append(sWhere.Remove(sWhere.Length - 4, 4));
			sGeneratedCode.Append(Environment.NewLine);	
			sGeneratedCode.Append("END");
			sGeneratedCode.Append(Environment.NewLine);			
			sGeneratedCode.Append("GO");

			return sGeneratedCode.ToString();
		}

		/// <summary>
		/// Trimtbl
		/// </summary>
		/// <param name="sStr"></param>
		/// <returns></returns>
		public string Trimtbl(string sStr)
		{
			string sProcessed;
			sProcessed = sStr;

			if (sStr.StartsWith("tbl"))
			{
				sProcessed = sStr.Substring(3,sStr.Length-3);
			}

			return sProcessed;
		}

        //==========================================================================
        //Instead of using SELECT * , name all the columns, SELECT A,B,C,D ...
        //P J Wesson - 2 June 2010
        //==========================================================================
        public string StarString_simple(SQLDMO.Columns colsFields)
        {
            //Declare memory used
            int nCount = 0;
            StringBuilder sStar = new StringBuilder();
            StringBuilder sFinal = new StringBuilder();
       
            //Loop over the columns
            foreach (SQLDMO.Column colKey in colsFields)
            {
                sStar.AppendFormat(" [{0}],", colKey.Name);
                nCount = nCount + 1;
            }

            //Trim off the last comma
            sFinal.Append(sStar.Remove(sStar.Length - 1, 1));
            sFinal.Append(" ");

            //Return the newly formed string
            return sFinal.ToString();
        }

        //==========================================================================
        //Instead of using SELECT * , name all the columns, SELECT A,B,C,D ...
        //Convert any DATE, DATETIME to a String
        //P J Wesson - 2 June 2010
        //==========================================================================
        public string StarString(SQLDMO.Columns colsFields)
        {
            //Declare memory used
            int nCount = 0;
            StringBuilder sStar = new StringBuilder();
            StringBuilder sFinal = new StringBuilder();

            //Loop over the columns
            foreach (SQLDMO.Column colKey in colsFields)
            {
                //Do we have a DATE or DATETIME?

                if (colKey.Datatype == "date" || colKey.Datatype == "datetime")
                {
                    sStar.AppendFormat(" convert(varchar(19),[{0}],120) as [{1}],", colKey.Name, colKey.Name);
                }
                else
                {
                    sStar.AppendFormat(" [{0}],", colKey.Name);
                }
                
                nCount = nCount + 1;
            }

            //Trim off the last comma
            sFinal.Append(sStar.Remove(sStar.Length - 1, 1));
            sFinal.Append(" ");

            //Return the newly formed string
            return sFinal.ToString();
        }

	}
}
