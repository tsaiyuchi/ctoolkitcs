using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;

namespace CToolkit.v1_0.Data
{
    public class CtkExcel
    {
        private OleDbConnection conn;    //連線物件
        List<OleDbCommand> cmdList = new List<OleDbCommand>();//命令集合
        List<OleDbDataReader> drList = new List<OleDbDataReader>();//讀取器集合
        List<OleDbDataAdapter> daList = new List<OleDbDataAdapter>();//讀取器集合

        public OleDbConnection connObj
        {
            get { return conn; }
            set { conn = value; }
        }

        public const string excelConnStr = "Provider=Microsoft.Jet.OLEDB.4.0;"
            + "Extended Properties=Excel 8.0;"
            + "Data Source=";

        public const string refExcelConnString_00001 = "Provider=Microsoft.Jet.OLEDB.4.0; "
            + "Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'"
            + "Data Source={0};";//For all data type to string
        public const string refCsvConnString_00001 = @"Provider=Microsoft.Jet.OleDb.4.0; "
               + @"Extended Properties='Text;HDR=YES;FMT=Delimited'; "
               + @"Data Source=A Directory";//CSV, set directory & select file name


        public const string defConnStr = excelConnStr;



        /*=====資料庫連線相關=============================================================*/
        public CtkExcel()
        {
        }
        public CtkExcel(string dbName)
        {
            connOpen(defConnStr + dbName);
        }
        //解構子-關閉連線
        ~CtkExcel()
        {
            Close();
        }


        //開啟連線
        public void connOpen(string connStr)
        {
            try
            {
                Close();
                conn = new OleDbConnection(connStr);
                conn.Open();
            }
            catch (CtkException ex) { throw new CtkException("連線建立失敗" + ex.Message); }
        }

        //傳回連線是否存在
        public bool IsConnOpen()
        {
            if (conn == null) { return false; }
            if (conn.State == ConnectionState.Open) { return true; }
            return false;
        }


        /*=========關閉=============================================================*/
        //關閉連線
        public void Close()
        {
            CloseDataAdapter();
            CloseDataReader();
            CloseCommand();
            CloseCnnection();
        }
        public void CloseCnnection()
        {
            try { if (conn != null) { conn.Close(); conn.Dispose(); } }
            catch (CtkException) { }
        }
        public void CloseCommand()
        {
            lock (cmdList)
            {
                foreach (OleDbCommand loop in cmdList)
                {
                    try { CloseCommand(loop); }
                    catch (CtkException) { }
                }
            }
        }
        public void CloseCommand(OleDbCommand argcmd)
        {
            if (argcmd == null) { return; }
            argcmd.Connection = null;
            argcmd.Dispose();
        }
        public void CloseDataReader()
        {
            lock (drList)
            {
                foreach (OleDbDataReader loop in drList)
                {
                    try { CloseDataReader(loop); }
                    catch (CtkException) { }
                }
            }
        }
        public void CloseDataReader(OleDbDataReader argdr)
        {
            if (argdr == null) { return; }
            argdr.Close();
        }
        public void CloseDataAdapter()
        {
            lock (daList)
            {
                foreach (OleDbDataAdapter loop in daList)
                {
                    try { CloseDataAdapter(loop); }
                    catch (CtkException) { }
                }
            }
        }
        public void CloseDataAdapter(OleDbDataAdapter argda)
        {
            if (argda == null) { return; }
            argda.Dispose();
        }





        /*=========GetDataAdapter=============================================================*/
        public OleDbDataAdapter GetDataAdapter(string sSql)
        {
            OleDbDataAdapter reda = null;
            try
            {
                reda = new OleDbDataAdapter(sSql, conn);
                daList.Add(reda);
                return reda;
            }
            finally { CloseDataAdapter(reda); }
        }








        /*=========Query=============================================================*/
        public DataTable Query(string sSql, List<OleDbParameter> paraList)
        {
            OleDbDataAdapter dataAdapter = null;
            OleDbCommand command = null;
            DataTable reData = null;
            try
            {
                command = new OleDbCommand(sSql, conn);
                command.Parameters.AddRange(paraList.ToArray());
                cmdList.Add(command);
                dataAdapter = new OleDbDataAdapter(command);
                daList.Add(dataAdapter);

                reData = new DataTable();
                dataAdapter.Fill(reData);
            }
            finally { CloseDataAdapter(dataAdapter); }
            return reData;
        }


        //取得所有資料表(Sheet)
        public DataTable QueryAllTable()
        {
            return conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            //dataTable.Rows[0]["TABLE_NAME"] is table name...
        }








        /*=========Execute=============================================================*/
        public int Execute(string sSql) { return Execute(sSql, null); }
        public int Execute(string sSql, List<OleDbParameter> paraList)
        {
            OleDbCommand mycmd = null;
            try
            {
                mycmd = new OleDbCommand();
                mycmd.Connection = connObj;
                mycmd.CommandText = sSql;
                if (paraList != null) { mycmd.Parameters.AddRange(paraList.ToArray()); }
                return mycmd.ExecuteNonQuery();
            }
            finally { CloseCommand(mycmd); }
        }





        /*=========Excel特殊轉換=============================================================*/
        public string dbStr_yearDate(object argObj)
        {
            return argObj.ToString();
        }
        public string dbStr_year(object argObj)
        {
            return "Year(" + argObj.ToString() + ")";
        }
        public string dbStr_month(object argObj)
        {
            return "Month(" + argObj.ToString() + ")";
        }
        public string dbStr_day(object argObj)
        {
            return "Day(" + argObj.ToString() + ")";
        }

    }
}
