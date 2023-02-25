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
	/// Summary description for Using4Keys.
	/// </summary>
	public class Using4Keys
	{
		public Using4Keys()
		{
			//
			// TODO: Add constructor logic here
			//
		}

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

		//-------------------------------------------------------------------------------
		public string GenerateSELECT4Keys(SQLDMO.Columns colsFields, string sTableName, string DatabaseName)
		{
			TableIndices obj = new TableIndices();
			string[] List = obj.ListAllKeys(sTableName, DatabaseName);

			int iCount = 0;
			int jCount = 0;
			int kCount = 0;
			int mCount = 0;
			int nCount = 0;

			StringBuilder sGeneratedCode = new StringBuilder();

			foreach (SQLDMO.Column colKey in colsFields)
			{
				//if (colKey.InPrimaryKey)
				if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colKey.Name, List))
				{
					nCount = nCount + 1;
				}
			}

			//Only worth if more than 4 keys

			if (nCount <4) return "";
			
			//Main loop over keys

			iCount = 0;
			foreach (SQLDMO.Column colKey1 in colsFields)
			{
				//if (colKey1.InPrimaryKey)
				if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colKey1.Name, List))
				{
					iCount = iCount +1;
					jCount = 0;
					foreach (SQLDMO.Column colKey2 in colsFields)
					{
						//if (colKey2.InPrimaryKey)
						if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colKey2.Name, List))
						{
							jCount = jCount+1;
							kCount = 0;
							foreach (SQLDMO.Column colKey3 in colsFields)
							{
								//if (colKey3.InPrimaryKey)
								if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colKey3.Name, List))
								{
									kCount = kCount+1;
									mCount = 0;			
									foreach (SQLDMO.Column colKey4 in colsFields)
									{
										//if (colKey4.InPrimaryKey)
										if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colKey4.Name, List))
										{
											mCount = mCount+1;
									
											//Need to create a stored procedure?

											if (kCount>jCount && jCount>iCount)
											{
												if (!IsDate(colKey1.Datatype) && 
													!IsDate(colKey2.Datatype) &&
													!IsDate(colKey3.Datatype) &&
													IsDate(colKey4.Datatype))
												{
                                                    sGeneratedCode.Append(GenerateStoreProc(colKey1, colKey2, colKey3, colKey4, sTableName, 1, colsFields));
													sGeneratedCode.Append(Environment.NewLine);
                                                    sGeneratedCode.Append(GenerateStoreProc(colKey1, colKey2, colKey3, colKey4, sTableName, 2, colsFields));
													sGeneratedCode.Append(Environment.NewLine);
                                                    sGeneratedCode.Append(GenerateStoreProc(colKey1, colKey2, colKey3, colKey4, sTableName, 3, colsFields));
													sGeneratedCode.Append(Environment.NewLine);
                                                    sGeneratedCode.Append(GenerateStoreProc(colKey1, colKey2, colKey3, colKey4, sTableName, 4, colsFields));
													sGeneratedCode.Append(Environment.NewLine);
                                                    sGeneratedCode.Append(GenerateStoreProc(colKey1, colKey2, colKey3, colKey4, sTableName, 5, colsFields));
													sGeneratedCode.Append(Environment.NewLine);
												}
												
											}
										}
									}
								}
							}
						}
					}
				}
			}

			return sGeneratedCode.ToString();
		}


		//-------------------------------------------------------------------------------
        public string GenerateStoreProc(SQLDMO.Column colKey1, SQLDMO.Column colKey2, SQLDMO.Column colKey3, SQLDMO.Column colKey4, string sTableName, int iChoice, SQLDMO.Columns colsFields)
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

			sGeneratedCode.AppendFormat("CREATE PROCEDURE dbo.sp{0}SELECTBy{1}By{2}By{3}", new string[]{Trimtbl(sTableName),colKey1.Name,colKey2.Name,colKey3.Name});			
			
			if (iChoice == 1) sGeneratedCode.AppendFormat("Max{0}",colKey4.Name);
			if (iChoice == 2) sGeneratedCode.AppendFormat("Between{0}ASC",colKey4.Name);
			if (iChoice == 3) sGeneratedCode.AppendFormat("OrderBy{0}ASC",colKey4.Name);
			if (iChoice == 4) sGeneratedCode.AppendFormat("DistinctBy{0}ASC",colKey4.Name);
			if (iChoice == 5) sGeneratedCode.AppendFormat("Min{0}",colKey4.Name);
			
			sGeneratedCode.Append(Environment.NewLine);

			// Param Declaration construction No.1

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

			// Param Declaration construction No.2

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

			// Param Declaration construction No.3

			sParamDeclaration.AppendFormat("	@var{0} {1}", new string[]{colKey3.Name, colKey3.Datatype});				
			
			if (colKey3.Datatype == "decimal")
			{
				sParamDeclaration.AppendFormat("({0},{1})", new string[]{colKey3.NumericPrecision.ToString(),colKey3.NumericScale.ToString()});
			}

			if (colKey3.Datatype == "binary" || 
				colKey3.Datatype == "char" || 
				colKey3.Datatype == "nchar" || 
				colKey3.Datatype == "nvarchar" || 
				colKey3.Datatype == "varbinary" || 
				colKey3.Datatype == "varchar")
			{
				sParamDeclaration.AppendFormat("({0})", colKey3.Length);
			}
			if (colKey3.Datatype == "varchar" ||
				colKey3.Datatype == "char")
			{
				sParamDeclaration.Append(" = NULL");
			}
			sParamDeclaration.Append(",");
			sParamDeclaration.Append(Environment.NewLine);
			
			// Param Declaration construction No.4 - which is a datetime

			if (iChoice == 2)
			{
				sParamDeclaration.AppendFormat("	@var{0}Start {1},", new string[]{colKey4.Name, colKey4.Datatype});				
				sParamDeclaration.Append(Environment.NewLine);
				sParamDeclaration.AppendFormat("	@var{0}End {1},", new string[]{colKey4.Name, colKey4.Datatype});				
				sParamDeclaration.Append(Environment.NewLine);
			}

			//Knock of the last comma in the build

			sGeneratedCode.Append(sParamDeclaration.Remove(sParamDeclaration.Length - 3, 3));				
			sGeneratedCode.Append(Environment.NewLine);

			//Continue with the stored procedures AS...

			sGeneratedCode.Append("AS");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("BEGIN");
			sGeneratedCode.Append(Environment.NewLine);
			
			sWhere.Append("	WHERE ");
			sWhere.AppendFormat("[{0}] = @var{0} AND ",colKey1.Name,colKey1.Name);
			sWhere.AppendFormat("[{0}] = @var{0} AND ",colKey2.Name,colKey2.Name);
			sWhere.AppendFormat("[{0}] = @var{0} AND ",colKey3.Name,colKey3.Name);
			
			if (iChoice == 1)
			{
				sGeneratedCode.AppendFormat("	SELECT max( [{0}] ) AS DateTime FROM {1}",new string[]{colKey4.Name,sTableName});
			}
			if (iChoice == 2)
			{
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
				sGeneratedCode.AppendFormat("	SELECT DISTINCT {0} FROM {1}",new string[]{colKey4.Name,sTableName});
			}
			if (iChoice == 5)
			{
				sGeneratedCode.AppendFormat("	SELECT min( [{0}] ) AS DateTime FROM {1}",new string[]{colKey4.Name,sTableName});
			}
			
			sGeneratedCode.Append(Environment.NewLine);			
			
			//Add the WHERE....

			sGeneratedCode.Append(sWhere.Remove(sWhere.Length - 4, 4));

			if (iChoice == 2 )
			{
				sGeneratedCode.AppendFormat(" AND [{0}] >= @var{0}Start AND [{0}] <= @var{0}End",colKey4.Name);
			}
			sGeneratedCode.Append(Environment.NewLine);	

			if (iChoice == 2 || iChoice == 3)
			{
				sGeneratedCode.AppendFormat("	ORDER BY [{0}] ASC",colKey4.Name);
				sGeneratedCode.Append(Environment.NewLine);	
			}

			if (iChoice ==4)
			{
				sGeneratedCode.AppendFormat("	ORDER BY [{0}] ASC",colKey4.Name);
				sGeneratedCode.Append(Environment.NewLine);	
			}

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
