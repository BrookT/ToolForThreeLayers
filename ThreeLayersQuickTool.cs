//Brook 2013-6-28 11:26
//Brook 2013-6-28 15:38 Modify for extension of js,model
//Brook 2013-6-28 17:19 Modify for extension getitem of DAL
//Brook 2013-7-1 17:29 Modify for extension dal 
//Brook 2013-7-2 16:48 Modify for extension dal

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace workTools
{


    class Program
    {
        static void Main(string[] args)
        {
            TSQL tsql = new TSQL();
            List<DataModel> ColumnList = new List<DataModel>() { 
                new DataModel(){
                    ColumnID=0,
                    ColumnName="ID",
                    ColumnType=ColumnType.INT.ToString(),
                    NotAllowNull=true,
                    IsPrimaryKey=true,
                    IsAutoGrow=true
                },
                new DataModel(){
                    ColumnID=1,
                    ColumnName="PageName",
                    ColumnType=ColumnType.NVARCHAR20.ToString()
                },
                new DataModel(){
                    ColumnID=2,
                    ColumnName="SiteID",
                    ColumnType=ColumnType.INT.ToString()
                },
                new DataModel(){
                    ColumnID=3,
                    ColumnName="Language",
                    ColumnType=ColumnType.NVARCHAR20.ToString()
                },
                new DataModel(){
                    ColumnID=4,
                    ColumnName="Content",
                    ColumnType=ColumnType.NTEXT.ToString()
                },
                new DataModel(){
                    ColumnID=5,
                    ColumnName="IsDisplay",
                    ColumnType=ColumnType.BIT.ToString()
                },
                new DataModel(){
                    ColumnID=6,
                    ColumnName="CreateDate",
                    ColumnType=ColumnType.DATETIME.ToString(),
                    DefaultValue="GETDATE()",
                    IsCreateTime=true
                },
                new DataModel(){
                    ColumnID=7,
                    ColumnName="UpdateDate",
                    ColumnType=ColumnType.DATETIME.ToString(),
                    DefaultValue="GETDATE()",
                    IsUpdateTime=true
                }
            };
            Console.WriteLine("Creating ...");
            tsql.output(tsql.CreateTable(ColumnList), TSQL.outputPath, TSQL.dir, TSQL.tblName, ".sql");
            string procResult = TSQL.createTSQLHeader() + tsql.CreateProc(ColumnList, "create") + tsql.CreateProc(ColumnList, "delete") + tsql.CreateProc(ColumnList, "update") + tsql.CreateProc(ColumnList, "get") + tsql.CreateProc(ColumnList, "getList");
            tsql.output(procResult, TSQL.outputPath, TSQL.dir, TSQL.procName, ".sql");
            tsql.output(tsql.CreateCSharp(ColumnList, "Model"), TSQL.outputPath, TSQL.dir, TSQL.metaName + "Model", ".cs");
            tsql.output(tsql.CreateCSharp(ColumnList, "DAL"), TSQL.outputPath, TSQL.dir, TSQL.metaName + "DAL", ".cs");
            tsql.output(tsql.CreateCSharp(ColumnList, "BLL"), TSQL.outputPath, TSQL.dir, TSQL.metaName + "BLL", ".cs");
            tsql.output(tsql.CreateCSharp(ColumnList, "webservice"), TSQL.outputPath, TSQL.dir, TSQL.metaName + "WS", ".asmx.cs");
            tsql.output(tsql.CreateJS(ColumnList), TSQL.outputPath, TSQL.dir, TSQL.metaName + "Package", ".js");
            Console.Write("complete!");
            Console.ReadKey();
        }
    }

    public class DataModel
    {
        public int ColumnID { get; set; }

        public string ColumnName { get; set; }

        public string ColumnType
        {
            get;
            set;
        }

        public bool NotAllowNull { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsAutoGrow { get; set; }

        public string DefaultValue { get; set; }

        public bool IsUpdateTime { get; set; }

        public bool IsCreateTime { get; set; }
    }

    public class ColumnType
    {
        public static string INT = "int";
        public static string BIT = "bit";
        public static string NTEXT = "NTEXT";
        public static string NVARCHAR20 = "NVARCHAR(20)";
        public static string NVARCHAR8 = "NVARCHAR(8)";
        public static string NVARCHAR255 = "NVARCHAR(255)";
        public static string NVARCHARMAX = "NVARCHAR(max)";
        public static string DATETIME = "DateTime";
    }



    public interface ICreateFile
    {
        void output(string content, string pathName, string dir, string fileName, string ex);
    }

    public class CreateFile : ICreateFile
    {
        public void output(string content, string pathName, string dir, string fileName, string ex)
        {
            if (!Directory.Exists(pathName + dir))
            {
                Directory.CreateDirectory(pathName + dir);
            }
            StreamWriter sw = new StreamWriter(pathName + dir + fileName + ex);
            string[] line = content.Split('\t');
            foreach (string temp in line)
            {
                sw.WriteLine(temp);
            }
            sw.Close();
        }
    }

    public class TSQL
    {
        public static string outputPath = @"J:\Brook Tang\WorkTool\";
        public static string metaName = "ResearchBanner";
        public static string dbName = "[main_new]";
        public static string dir = metaName + @"\";
        public static string tblName = "ringier_T_" + metaName;
        public static string procName = "proc_" + metaName;
        public static string procCName = "Insert" + metaName;
        public static string procRName = "Get" + metaName;
        public static string procUName = "Update" + metaName;
        public static string procDName = "Delete" + metaName;
        public static string procGLName = "Get" + metaName + "List";
        public static string devName = "Brook";
        public static string projName = "RMS";
        public static string modelNS = String.IsNullOrEmpty(projName) ? "Models" : projName + ".Models";
        public static string dalNS = string.IsNullOrEmpty(projName) ? "DAL" : projName + ".DAL";
        public static string bllNS = string.IsNullOrEmpty(projName) ? "BLL" : projName + ".BLL";
        IList<DataModel> ColumnList;
        public TSQL()
        {

        }

        public static string getNameSpace(string projName, string classLibName)
        {
            string result = "";
            projName = TSQL.projName;
            result = string.IsNullOrEmpty(projName) ? classLibName : projName + "." + classLibName;
            return result;
        }

        public string CreateJS(List<DataModel> ColumnList)
        {
            string result = "";
            result += CreateJSSignature(TSQL.devName, metaName);
            result += "var " + metaName + "Request={\t";//request start
            result += "     list:function(postData){Post.Ajax(postData,'" + procGLName + "'," + metaName + "CallBack.list," + metaName + "Callback.error);},\t";
            result += "     item:function(postData){Post.Ajax(postData,'" + procRName + "'," + metaName + "Callback.item," + metaName + "Callback.error);},\t";
            result += "     insert:function(postData{Post.Ajax(postData,'" + procCName + "'," + metaName + "Callback.insert," + metaName + "Callback.error);},\t";
            result += "     update:function(postData){Post.Ajax(postData,'" + procUName + "'," + metaName + "Callback.update," + metaName + "Callback.error);},\t";
            result += "     delete:function(postData){Post.Ajax(postData,'" + procDName + "'," + metaName + "Callback.delete," + metaName + "Callback.error);}\t";
            result += "}\t";//request end
            result += "var " + metaName + "CallBack={\t";//callback start
            result += "     list:function(result){result=result.d;if(result.Flag){}},\t";
            result += "     item:function(result){result=result.d;if(result.Flag){}},\t";
            result += "     update:function(result){result=result.d;if(result.Flag){}},\t";
            result += "     insert:function(result){result=result.d;if(result.Flag){}},\t";
            result += "     delete:function(result){result=result.d;if(result.Flag){}},\t";
            result += "}\t";//callback end
            return result;
        }



        public string CreateTable(List<DataModel> ColumnList)
        {
            int columnCount = 0;
            string result = "";
            result += createTSQLHeader();
            result += createSignature("Brook", "Create Ringier_T_" + metaName, "create table ringier_T_" + metaName);
            result += "CREATE TABLE " + tblName + "(\t";
            foreach (DataModel column in ColumnList)
            {
                columnCount++;
                result += column.ColumnName + " " + column.ColumnType + " ";
                if (column.IsPrimaryKey)
                {
                    result += "PRIMARY KEY ";
                }
                if (column.NotAllowNull)
                {
                    result += "NOT NULL ";
                }
                if (column.IsAutoGrow)
                {
                    result += "IDENTITY(1,1) ";
                }
                if (column.DefaultValue != null)
                {
                    result += "DEFAULT(" + column.DefaultValue + ") ";
                }
                if (columnCount != ColumnList.Count)
                {
                    result += ",";
                }
                result += "\t";
            }
            result += ")\t";

            return result;
        }

        public string CreateCSharp(List<DataModel> ColumnList, string CSharpFileName)
        {
            string result = "";
            switch (CSharpFileName)
            {
                case "Model": result = CreateCSharpModel(ColumnList); break;
                case "DAL": result = CreateCSharpDAL(ColumnList); break;
                case "BLL": result = CreateCSharpBLL(ColumnList); break;
                case "webservice": result = CreateCSharpWS(ColumnList); break;
            }
            return result;
        }

        public string CreateCSharpModel(List<DataModel> ColumnList)
        {
            string result = "";
            result += "using System;\t";
            result += "using System.Collections.Generic;\t";
            result += "using System.Text;\t";
            result += "namespace " + TSQL.getNameSpace(TSQL.projName, "Model") + "\t";
            result += "{\t";//namespace start
            result += "public class " + metaName + "Model \t";
            result += " {";//class start
            foreach (DataModel column in ColumnList)
            {
                result += "     public " + ParseSqlTypeIntoCSharpType(column.ColumnType) + " " + column.ColumnName + "{    get;set;    }\t";
            }
            result += " }\t";//class end
            result += "}\t";//namespace end
            return result;
        }

        public string CreateCSharpDAL(List<DataModel> ColumnList)
        {
            string result = "";
            result += "using System;\t";
            result += "using System.Collections.Generic;\t";
            result += "using System.Text;\t";
            result += "using System.Data;\t";
            result += "using System.Data.SqlClient;\t";
            result += "namespace " + TSQL.getNameSpace(TSQL.projName, "DAL") + "\t";
            result += "{\t";//namespace start
            result += "     public class " + procCName + "DAL\t";//create 
            result += "     {\t";//class start
            result += "         private string procName;\t";
            result += "         private IDataParameter[] parameters;\t";
            result += "         public Models." + metaName + "Model " + TSQL.procRName + "(Models." + metaName + "Model searchM)\t";
            result += "         {\t";//method start
            result += "         try{\t";
            string InstanceName = "resultM";
            result += "             Models." + metaName + "Model " + InstanceName + "=new Models." + metaName + "Model();\t";
            result += "             procName=\"" + TSQL.procRName + "\";\t";
            result += "             parameters=new IDataParameter[]{\t";//param start
            foreach (DataModel column in ColumnList)
            {
                if (column.IsPrimaryKey)
                {
                    result += "               new SqlParameter(\"@" + column.ColumnName + "\",searchM." + column.ColumnName + ")\t";
                }
            }
            result += "               };\t";//param end
            result += "               SqlHelper sqlhelper=new SqlHelper();\t";
            result += "               using(SqlDataReader sr=sqlhelper.RunProcedure(procName,parameters))\t";
            result += "               {\t";//using start
            result += "                 while(sr.read())\t";
            result += "                 {\t";//while start
            foreach (DataModel column in ColumnList)
            {
                result += TSQL.SetValueWhenReadDB(column, "resultM");
            }
            result += "                 }\t";//while end
            result += "               }\t";//using end
            result += "             return " + InstanceName + ";\t";
            result += "               }\t";
            result += "             catch(exception e)\t";
            result += "             {\t";
            result += "                 throw ExceptionControl.Exception(e," + procRName + " DAL);\t";
            result += "             }\t";
            result += "         }\t";//method end

            result += "     public int " + TSQL.procDName + " (";//delete
            foreach (DataModel column in ColumnList)
            {
                if (column.IsPrimaryKey)
                {
                    result += column.ColumnType + " " + column.ColumnName;
                }
            }
            result += ")\t";
            result += "     {\t";//method start
            result += "         try{\t";//try start
            result += "         procName=\"" + TSQL.procDName + "\";\t";
            result += "         parameters=new IDataParameter[]{\t";//param start
            foreach (DataModel column in ColumnList)
            {
                if (column.IsPrimaryKey)
                {
                    result += "         new SqlParameter(@" + column.ColumnName + "," + column.ColumnName + ")\t";
                }
            }
            result += "         }\t;";//param end
            result += "         Sqlhelper sqlhelper=new SqlHelper();\t";
            result += "         int result=sqlhelper.ExecProcedure(procName,parameters);\t";
            result += "         return result;\t";
            result += "         }\t";
            result += "         catch(exception e)\t";
            result += "         {\t";//catch start
            result += "            throw ExceptionControl.Exception(e," + procDName + " DAL);\t";
            result += "         }\t";//catch end
            result += "     }\t";//method end
            string refName = "ref" + metaName + "Model";
            result += "     public int " + procUName + "(Models." + metaName + "Model " + refName + ")\t";
            result += "     {\t";//method start
            result += "         try{\t";//try start
            result += "                 procName=\"" + procUName + "\"\t";
            result += "                 parameters=new IDataParameter[]{\t";//param start
            int columnCount = 0;
            foreach (DataModel column in ColumnList)
            {
                columnCount++;
                result += "                 new SqlParameter(@" + column.ColumnName + ",refName." + column.ColumnName + ")";
                if (columnCount != ColumnList.Count)
                {
                    result += ",";
                }
                result += "\t";
            }
            result += "         Sqlhelper sqlhelper=new SqlHelper();\t";
            result += "         int result=sqlhelper.ExecProcedure(procName,parameters);\t";
            result += "         return result;\t";
            result += "                   }\t";//param end
            result += "               }\t";//try end
            result += "         catch(exception e){\t";//catch start
            result += "             throw ExceptionControl(e," + procUName + " DAL);\t";
            result += "           }\t";//catch end
            result += "     }\t";//method end
            foreach (DataModel column in ColumnList)
            {
                if (column.IsPrimaryKey)
                {
                    result += "     public Models." + metaName + "Model " + procRName + "(" + column.ColumnType + " " + column.ColumnName + ")\t";//get
                }
            }
            result += "     {\t";//method start
            string resultInstance = "resultM";
            result += "         Models." + metaName + "Model " + resultInstance + "=new Models." + metaName + "Model();\t";
            result += "         procName=\"" + procRName + "\";\t";
            result += "         parameters=new IDataParameter[](\t";//param start
            columnCount = 0;
            foreach (DataModel column in ColumnList)
            {
                if (column.IsPrimaryKey)
                {
                    result += "             new SqlParameter(@" + column.ColumnName + "," + column.ColumnName + ")";
                    result += "\t";
                }
            }
            result += "           );\t";//param end
            result += "         SqlHelper sqlhelper=new SqlHelper();\t";
            result += "         using(SqlDataReader sr=sqlhelper.ExecProcedure(procName,parameters))\t";
            result += "         {\t";//using start
            foreach (DataModel column in ColumnList)
            {
                if (!IsTypeStringable(column.ColumnType))
                {
                    result += "             " + resultInstance + "." + column.ColumnName + "=" + ParseSqlTypeIntoCSharpType(column.ColumnType) + ".Parse(sr[\"" + column.ColumnName + "\"].ToString());\t";
                }
                else
                {
                    result += "             " + resultInstance + "." + column.ColumnName + "=sr[\"" + column.ColumnName + "\"].ToString();\t";
                }
            }
            result += "         }\t";//using end
            result += "     return " + InstanceName + ";\t";
            result += "     }\t";//method end
            
            result += "     public List<Models."+metaName+"Model> "+procGLName+"(Models."+metaName+"Model "+InstanceName+",int startRowID,int endRowID)\t";//get list
            result += "     {\t";//method start
            resultInstance = "resultL";
            result += "         List<Models."+metaName+"Model> "+resultInstance+"=new List<Models."+metaName+">();\t";
            result += "         procName=\""+procGLName+"\";\t";
            result += "         parameters=new IDataParameter[]{\t";//param start
            columnCount = 0;
            foreach (DataModel column in ColumnList)
            {
                columnCount++;
                if (!column.IsPrimaryKey && !column.IsCreateTime && !column.IsUpdateTime)
                {
                    result += "             new SqlParameter(@" + column.ColumnName + "," + column.ColumnName + ")";//param start
                    if ( !column.IsPrimaryKey && !column.IsCreateTime && !column.IsUpdateTime)
                    {
                        result += ",";
                    }
                    result += "\t";
                }
            }
            result += "             new SqlParameter(@StartRowID,startRowID),\t";
            result += "             new SqlParameter(@EndRowID,endRowID)\t";
            result += "             };\t";//param end
            result += "         SqlHelper sqlhelper=new SqlHelper();\t";
            result += "         using(SqlDataReader sr=sqlhelper.RunProcedure(procName,parameters))\t";
            result += "         {\t";
            result += "             Models." + metaName + "Model tempM=new Models." + metaName + "Model();\t" ;
            foreach (DataModel column in ColumnList)
            {
                result += "             result."+resultInstance+"=sr[\""+column.ColumnName+"\"].ToString();\t";
            }
            result+= "          );\t";//param end
            result += "             " + resultInstance + ".Add(tempM);\t";
            result += "             return "+resultInstance+";\t";
            result += "     }\t";//method end

            result += "     }\t";//class end
            result += "}\t";//namespace end
            return result;
        }

        public string CreateCSharpBLL(List<DataModel> ColumnList)
        {
            string result = "";
            return result;
        }

        public string CreateCSharpWS(List<DataModel> ColumnList)
        {
            string result = "";
            return result;
        }

        public string CreateProc(List<DataModel> ColumnList, string operation)
        {
            string result = "";
            int columnCount = 0;
            result += createSignature(TSQL.devName, operation + metaName, operation + " " + metaName);
            switch (operation)
            {
                case "create":
                    result += "CREATE PROC " + procCName + " \t";
                    foreach (DataModel column in ColumnList)
                    {
                        columnCount++;
                        if (column.IsPrimaryKey)
                        {
                            result += "@sys_id int out ";
                        }
                        else
                        {
                            result += "@" + column.ColumnName + " " + column.ColumnType + " ";
                        }
                        if (columnCount != ColumnList.Count)
                        {
                            result += ",";
                        }
                        result += "\t";
                    }
                    result += "AS\t";
                    result += "BEGIN\t";
                    result += "INSERT INTO [" + tblName + "] (";
                    columnCount = 0;
                    foreach (DataModel column in ColumnList)
                    {
                        columnCount++;
                        if (!column.IsPrimaryKey)
                        {
                            result += "[" + column.ColumnName + "]";
                        }
                        if (columnCount != ColumnList.Count && !column.IsPrimaryKey)
                        {
                            result += ",";
                        }
                    }
                    result += ")";
                    result += " values ( ";
                    foreach (DataModel column in ColumnList)
                    {
                        columnCount++;
                        if (!column.IsPrimaryKey)
                        {
                            result += "@" + column.ColumnName + "";
                        }
                        if (columnCount != ColumnList.Count && !column.IsPrimaryKey && columnCount != ColumnList.Count && !column.IsCreateTime)
                        {
                            result += ",";
                        }
                    }
                    result += ");\t";
                    result += "SET @sys_id=@@IDENTITY;\t";
                    result += "END\t";
                    result += "GO\t\t";
                    break;
                case "get":
                    result += "CREATE PROC " + procRName + "\t";
                    foreach (DataModel column in ColumnList)
                    {
                        if (column.IsPrimaryKey)
                        {
                            result += "@" + column.ColumnName + " " + column.ColumnType + "\t";
                        }
                    }
                    result += "AS\t";
                    result += "BEGIN\t";
                    result += "SELECT * FROM [" + tblName + "] WHERE ";
                    foreach (DataModel column in ColumnList)
                    {
                        if (column.IsPrimaryKey)
                        {
                            result += "@" + column.ColumnName + "=" + column.ColumnName + ";\t";
                        }
                    }
                    result += "END\t";
                    result += "GO\t";
                    ; break;
                case "getList":
                    result += "CREATE PROC " + procGLName + "\t";
                    List<DataModel> ColumnTemp = new List<DataModel>();
                    foreach (DataModel column in ColumnList)
                    {
                        if (!column.IsPrimaryKey && !column.IsCreateTime && !column.IsUpdateTime)
                        {
                            ColumnTemp.Add(column);
                        }
                    }
                    columnCount = 0;
                    foreach (DataModel column in ColumnTemp)
                    {
                        result += "@" + column.ColumnName + " " + column.ColumnType + " ,\t";
                    }
                    result += "@StartRowID int,\t";
                    result += "@EndRowID int \t";
                    result += "AS\t";
                    result += "BEGIN\t";
                    result += "DECLARE @SQL NVARCHAR(4000)='';\t";
                    result += "DECLARE @WHERE NVARCHAR(4000)='';\t";
                    result += "DECLARE @ORDER NVARCHAR(4000)='';\t";
                    result += "SET @SQL=@SQL+'SELECT * from [" + tblName + "]'; \t";
                    result += "SET @WHERE=@WHERE+'WHERE 1=1 ' ;\t";
                    result += "SET @ORDER=@ORDER+'ORDER BY \t";
                    foreach (DataModel column in ColumnList)
                    {
                        if (column.IsPrimaryKey)
                        {
                            result += "[" + column.ColumnName + "] desc,";
                        }
                        if (column.IsCreateTime)
                        {
                            result += "[" + column.ColumnName + "] desc,";
                        }
                        if (column.IsUpdateTime)
                        {
                            result += "[" + column.ColumnName + "] desc';\t";
                        }
                    }
                    foreach (DataModel column in ColumnList)
                    {
                        string obj_temp;
                        if (!column.IsPrimaryKey && !column.IsUpdateTime && !column.IsCreateTime)
                        {
                            if (IsTypeStringable(column.ColumnType))
                            {
                                obj_temp = "@" + column.ColumnName + "";
                            }
                            else
                            {
                                obj_temp = "CONVERT(NVARCHAR(20),@" + column.ColumnName + ")";
                            }
                            result += "IF(LTRIM(RTRIM(ISNULL(" + obj_temp + ",'')))<>'')\t";
                            result += "BEGIN\t";
                            if (IsTypeStringable(column.ColumnType))
                            {
                                result += "SET @WHERE=@WHERE+' AND [" + column.ColumnName + "]='''+" + obj_temp + "+''' ';\t";
                            }
                            else
                            {
                                result += "SET @WHERE=@WHERE+' AND [" + column.ColumnName + "]='+" + obj_temp + "+ ' ';\t";
                            }
                            result += "END\t";
                        }
                    }
                    result += "SET @SQL=@SQL+@WHERE+@ORDER;\t";
                    result += "EXEC (@SQL);\t";
                    result += "END\t";
                    result += "GO\t\t";
                    break;
                case "update":
                    result += "CREATE PROC " + procUName + " \t";
                    columnCount = 0;
                    foreach (DataModel column in ColumnList)
                    {
                        columnCount++;
                        result += "@" + column.ColumnName + " " + column.ColumnType + "";
                        if (columnCount == ColumnList.Count)
                        {
                            result += ",";
                        }
                        result += "\t";
                    }
                    result += "AS\t";
                    result += "BEGIN\t";
                    result += "UPDATE [" + tblName + "] SET \t";
                    columnCount = 0;
                    foreach (DataModel column in ColumnList)
                    {
                        columnCount++;
                        if (!column.IsPrimaryKey && !column.IsUpdateTime && !column.IsCreateTime)
                        {
                            result += "[" + column.ColumnName + "]=@" + column.ColumnName + "";
                        }
                        else if (column.IsUpdateTime)
                        {
                            result += "[" + column.ColumnName + "]=" + column.DefaultValue + "";
                        }
                        if (columnCount != ColumnList.Count && !column.IsPrimaryKey&&!column.IsCreateTime)
                        {
                            result += ",";
                        }
                        result += "\t";
                    }
                    result += " WHERE \t";
                    foreach (DataModel column in ColumnList)
                    {
                        if (column.IsPrimaryKey)
                        {
                            result += "@" + column.ColumnName + "=" + column.ColumnName + "\t";
                        }
                    }
                    result += "END\t";
                    result += "GO\t\t";
                    break;
                case "delete":
                    result += "CREATE PROC " + procDName + " \t";
                    foreach (DataModel column in ColumnList)
                    {
                        if (column.IsPrimaryKey)
                        {
                            result += "@" + column.ColumnName + " " + column.ColumnType + " \t";
                        }
                    }
                    result += "AS\t";
                    result += "BEGIN\t";
                    result += "DELETE FROM [" + tblName + "] WHERE ";
                    foreach (DataModel column in ColumnList)
                    {
                        if (column.IsPrimaryKey)
                        {
                            result += "[" + column.ColumnName + "]=" + "@" + column.ColumnName + " \t";
                        }
                    }
                    result += "END\t";
                    result += "GO\t\t";
                    ; break;
            }
            return result;
        }

        public static bool IsTypeStringable(string type)
        {
            bool result = false;
            switch (type)
            {
                case "int":
                case "bit":
                case "DateTime": result = false; break;
                case "NTEXT":
                case "NVARCHAR(20)":
                case "NVARCHAR(8)":
                case "NVARCHAR(255)":
                case "NVARCHAR(max)": result = true; break;
                default: break;
            }
            return result;
        }

        public static string SetValueWhenReadDB(DataModel column, string InstanceName)
        {
            string result = "";
            switch (column.ColumnType)
            {
                case "int": result = "                   " + InstanceName + "." + column.ColumnName + "=int.parse(sr[\"" + column.ColumnName + "\"].ToString());\t"; break;
                case "bit": result = "                   " + InstanceName + "." + column.ColumnName + "=sr[\"" + column.ColumnName + "\"].ToString()==\"1\"?true:false;\t"; break;
                case "DateTime": result = "                  " + InstanceName + "." + column.ColumnName + "=sr[\"" + column.ColumnName + "\"].ToString();\t"; break;
                case "NTEXT":
                case "NVARCHAR(20)":
                case "NVARCHAR(8)":
                case "NVARCHAR(255)":
                case "NVARCHAR(max)": result = "                 " + InstanceName + "." + column.ColumnName + "=sr[\"" + column.ColumnName + "\"].ToString();\t"; break;
                default: break;
            }
            return result;
        }

        public static string ParseSqlTypeIntoCSharpType(string type)
        {
            string result = "";
            switch (type)
            {
                case "int": result = "int"; break;
                case "bit": result = "bool"; break;
                case "DateTime": result = "DateTime"; break;
                case "NTEXT":
                case "NVARCHAR(20)":
                case "NVARCHAR(8)":
                case "NVARCHAR(255)":
                case "NVARCHAR(max)": result = "string"; break;
                default: break;
            }
            return result;
        }

        public static string createTSQLHeader()
        {
            string result = "";
            result += "USE " + dbName + "\t";
            result += "go\t";
            result += "SET ANSI_NULLS ON\t";
            result += "GO\t";
            return result;
        }

        public string createSignature(string devName, string procName, string description)
        {
            string result = "";
            result += "--===================================\t";
            result += "-- Author : " + devName + "\t";
            result += "-- Create Date : " + DateTime.Now + "\t";
            result += "-- Proc Name : " + procName + "\t";
            result += "-- Description : " + description + "\t";
            result += "--===================================\t";
            return result;
        }

        public string CreateJSSignature(string devName, string description)
        {
            string result = "";
            result += "//   Author : " + devName + "\t";
            result += "//   Create Date : " + DateTime.Now + "\t";
            result += "//   Description : " + description + "\t";
            result += "//   FileName : " + metaName + "Package.js" + "\t";
            return result;
        }

        public void output(string content, string pathName, string dir, string fileName, string ex)
        {
            if (!Directory.Exists(pathName + dir))
            {
                Directory.CreateDirectory(pathName + dir);
            }
            StreamWriter sw = new StreamWriter(pathName + dir + fileName + ex);
            string[] line = content.Split('\t');
            foreach (string temp in line)
            {
                sw.WriteLine(temp);
            }
            sw.Close();
        }
    }
}
