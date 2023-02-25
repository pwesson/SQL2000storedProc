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
	/// Summary description for Using3Keys.
	/// </summary>
	public class Using3Keys
	{
		public Using3Keys()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		//-------------------------------------------------------------------------------
		public string GenerateSELECT3Keys(SQLDMO.Columns colsFields, string sTableName, string DatastreamName)
		{
			TableIndices obj = new TableIndices();
			string[] List = obj.ListAllKeys(sTableName, DatastreamName);

			int iCount = 0;
			int jCount = 0;
			int kCount = 0;
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

			//Only worth this is there are at least three keys
			if (nCount < 3) return "";	
		
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
										if (jCount > iCount) 
										{
                                            if (!IsDate(colKey1.Datatype) && 
												!IsDate(colKey2.Datatype) &&
												IsDate(colKey3.Datatype))
											{
                                                sGeneratedCode.Append(GenerateSubExtra3(colKey1, colKey2, colKey3, sTableName, 1, colsFields));
												sGeneratedCode.Append(Environment.NewLine);
                                                sGeneratedCode.Append(GenerateSubExtra3(colKey1, colKey2, colKey3, sTableName, 2, colsFields));
												sGeneratedCode.Append(Environment.NewLine);
                                                sGeneratedCode.Append(GenerateSubExtra3(colKey1, colKey2, colKey3, sTableName, 3, colsFields));
												sGeneratedCode.Append(Environment.NewLine);
                                                sGeneratedCode.Append(GenerateSubExtra3(colKey1, colKey2, colKey3, sTableName, 4, colsFields));
												sGeneratedCode.Append(Environment.NewLine);
                                                sGeneratedCode.Append(GenerateSubExtra3(colKey1, colKey2, colKey3, sTableName, 5, colsFields));
												sGeneratedCode.Append(Environment.NewLine);
											}
										}
									}
								}
							}
						}
					}
			}

			//Only worth this is there are at least three keys
			
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
										if (jCount > iCount) 
										{
											if (!IsDate(colKey1.Datatype) && 
												!IsDate(colKey2.Datatype) &&
												IsDate(colKey3.Datatype))
											{
												foreach (SQLDMO.Column colKey4 in colsFields)
												{
													//if (colKey4.InPrimaryKey == false && colKey4.Name.StartsWith("Sum"))
                                                    if (Wesson.Win.Dev.GenStoredProcedures.TableIndices.IsIndex(colKey4.Name, List) == false && 
																					colKey4.Name.StartsWith("Sum"))
													{
														sGeneratedCode.Append(GenerateSubExtra3b(colKey1,colKey2,colKey3,colKey4,sTableName,6));
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
        public string GenerateSubExtra3(SQLDMO.Column colKey1, SQLDMO.Column colKey2, SQLDMO.Column colKey3, string sTableName, int iChoice, SQLDMO.Columns colsFields)
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
			
			if (iChoice == 1) sGeneratedCode.AppendFormat("Max{0}",colKey3.Name);
			if (iChoice == 2) sGeneratedCode.AppendFormat("Between{0}ASC",colKey3.Name);
			if (iChoice == 3) sGeneratedCode.AppendFormat("OrderBy{0}ASC",colKey3.Name);
			if (iChoice == 4) sGeneratedCode.AppendFormat("DistinctBy{0}ASC",colKey3.Name);
			if (iChoice == 5) sGeneratedCode.AppendFormat("Min{0}",colKey3.Name);
			if (iChoice == 6) sGeneratedCode.AppendFormat("Between{0}Sum",colKey3.Name);
			
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

			if (iChoice == 2 || iChoice == 6)
			{
				sParamDeclaration.AppendFormat("	@var{0}Start {1},", new string[]{colKey3.Name, colKey3.Datatype});				
				sParamDeclaration.Append(Environment.NewLine);
				sParamDeclaration.AppendFormat("	@var{0}End {1},", new string[]{colKey3.Name, colKey3.Datatype});				
				sParamDeclaration.Append(Environment.NewLine);
			}

			sGeneratedCode.Append(sParamDeclaration.Remove(sParamDeclaration.Length - 3, 3));				
			sGeneratedCode.Append(Environment.NewLine);

			sGeneratedCode.Append("AS");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("BEGIN");
			sGeneratedCode.Append(Environment.NewLine);
			
			sWhere.Append("	WHERE ");
			sWhere.AppendFormat("[{0}] = @var{0} AND ",colKey1.Name,colKey1.Name);
			sWhere.AppendFormat("[{0}] = @var{0} AND ",colKey2.Name,colKey2.Name);
			
			if (iChoice == 1)
			{
				sGeneratedCode.AppendFormat("	SELECT max( [{0}] ) AS DateTime FROM {1}",new string[]{colKey3.Name,sTableName});
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
				sGeneratedCode.AppendFormat("	SELECT DISTINCT {0} FROM {1}",new string[]{colKey3.Name,sTableName});
			}
			if (iChoice == 5)
			{
				sGeneratedCode.AppendFormat("	SELECT min( [{0}] ) AS DateTime FROM {1}",new string[]{colKey3.Name,sTableName});
			}
			if (iChoice == 6)
			{
				//sGeneratedCode.AppendFormat("	SELECT sum(SumLnRatio)/count(SumLnRatio) As SumLnRatio FROM {1}",new string[]{colKey3.Name,sTableName});
			}


			sGeneratedCode.Append(Environment.NewLine);			
			
			sGeneratedCode.Append(sWhere.Remove(sWhere.Length - 4, 4));

			if (iChoice == 2 )
			{
				sGeneratedCode.AppendFormat(" AND [{0}] >= @var{0}Start AND [{0}] <= @var{0}End",colKey3.Name);
			}
			if (iChoice == 6)
			{
				sGeneratedCode.AppendFormat(" AND [{0}] > @var{0}Start AND [{0}] < @var{0}End",colKey3.Name);
			}			sGeneratedCode.Append(Environment.NewLine);	

			if (iChoice == 2 || iChoice == 3)
			{
				sGeneratedCode.AppendFormat("	ORDER BY [{0}] ASC",colKey3.Name);
				sGeneratedCode.Append(Environment.NewLine);	
			}

			if (iChoice ==4)
			{
				sGeneratedCode.AppendFormat("	ORDER BY [{0}] ASC",colKey3.Name);
				sGeneratedCode.Append(Environment.NewLine);	
			}

			sGeneratedCode.Append("END");
			sGeneratedCode.Append(Environment.NewLine);			

			sGeneratedCode.Append("GO");
			
			return sGeneratedCode.ToString();
		}

		//-------------------------------------------------------------------------------
		public string GenerateSubExtra3b(SQLDMO.Column colKey1, SQLDMO.Column colKey2, SQLDMO.Column colKey3, SQLDMO.Column colKey4, string sTableName, int iChoice)
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
			
			if (iChoice == 6) sGeneratedCode.AppendFormat("Between{0}{1}",colKey3.Name,colKey4.Name);
			
			sGeneratedCode.Append(Environment.NewLine);

			// Param Declaration construction
			sParamDeclaration.AppendFormat("	@var{0} {1}", new string[]{colKey1.Name, colKey1.Datatype});				
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

			if (iChoice == 6)
			{
				sParamDeclaration.AppendFormat("	@var{0}Start {1},", new string[]{colKey3.Name, colKey3.Datatype});				
				sParamDeclaration.Append(Environment.NewLine);
				sParamDeclaration.AppendFormat("	@var{0}End {1},", new string[]{colKey3.Name, colKey3.Datatype});				
				sParamDeclaration.Append(Environment.NewLine);
			}

			sGeneratedCode.Append(sParamDeclaration.Remove(sParamDeclaration.Length - 3, 3));				
			sGeneratedCode.Append(Environment.NewLine);

			sGeneratedCode.Append("AS");
			sGeneratedCode.Append(Environment.NewLine);
			sGeneratedCode.Append("BEGIN");
			sGeneratedCode.Append(Environment.NewLine);
			
			sWhere.Append("	WHERE ");
			sWhere.AppendFormat("[{0}] = @var{0} AND ",colKey1.Name,colKey1.Name);
			sWhere.AppendFormat("[{0}] = @var{0} AND ",colKey2.Name,colKey2.Name);
			
			if (iChoice == 6)
			{
				//sGeneratedCode.AppendFormat("	SELECT sum({0})/count({1}) As {2} FROM {3}",new string[]{colKey4.Name, colKey4.Name, colKey4.Name, sTableName});
			}

			sGeneratedCode.Append(Environment.NewLine);			
			
			sGeneratedCode.Append(sWhere.Remove(sWhere.Length - 4, 4));

			if (iChoice == 6)
			{
				sGeneratedCode.AppendFormat(" AND [{0}] > @var{0}Start AND [{0}] < @var{0}End",colKey3.Name);
			}			sGeneratedCode.Append(Environment.NewLine);	

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
