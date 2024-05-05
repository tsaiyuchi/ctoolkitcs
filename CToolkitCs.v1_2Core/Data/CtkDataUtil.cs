using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Reflection;

namespace CToolkitCs.v1_2Core.Data
{
    public class CtkDataUtil
    {


        public static List<Dictionary<string, Object>> ToListDictionary(DataTable dataTable)
        {
            var dataRows = dataTable.Select();
            return (from row in dataRows
                    select row.ItemArray.Select((a, i) => new { Name = dataTable.Columns[i].ColumnName, Value = a })
                       .ToDictionary(x => x.Name, x => x.Value)
                       ).ToList();
        }




        public static DataTable CsvToDataTable(string csv)
        {
            DataTable dataTable = new DataTable();

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(csv)))
            using (TextFieldParser parser = new TextFieldParser(ms))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                bool isFirstRow = true;

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();

                    if (isFirstRow)
                    {
                        foreach (string field in fields)
                        {
                            dataTable.Columns.Add(new DataColumn(field, typeof(string)));
                        }
                        isFirstRow = false;
                    }
                    else
                    {
                        DataRow row = dataTable.NewRow();
                        row.ItemArray = fields;
                        dataTable.Rows.Add(row);
                    }
                }
            }

            return dataTable;
        }
        public static List<T> CsvToList<T>(string csv) where T : new()
        {
            var rtn = new List<T>();
            var datatable = CsvToDataTable(csv);
            var type = typeof(T);

            foreach (DataRow row in datatable.Rows)
            {
                var entity = new T();
                foreach (DataColumn col in datatable.Columns)
                {
                    var field = type.GetField(col.ColumnName);
                    var prop = type.GetProperty(col.ColumnName);

                    if (field != null)
                        field.SetValue(entity, row[col]);
                    if (prop != null)
                        prop.SetValue(entity, row[col]);
                }
            }

            return rtn;
        }


        public static string CsvFrom<T>(List<T> objectList)
        {
            StringBuilder csvBuilder = new StringBuilder();
            var type = typeof(T);

            // 添加 CSV 標題行
            var members = type.GetMembers();
            foreach (var mem in members)
            {
                if (mem.MemberType == System.Reflection.MemberTypes.Field
                    || mem.MemberType == System.Reflection.MemberTypes.Property)
                    csvBuilder.Append(mem.Name + ",");
            }
            csvBuilder.AppendLine();

            // 添加物件列表的數據行
            foreach (var obj in objectList)
            {
                foreach (var mem in members)
                {
                    var field = mem as FieldInfo;
                    var prop = mem as PropertyInfo;

                    if (field != null)
                        csvBuilder.Append(field.GetValue(obj) + ",");
                    else if (prop != null)
                        csvBuilder.Append(prop.GetValue(obj) + ",");
                }
                csvBuilder.AppendLine();
            }

            return csvBuilder.ToString();
        }
        public static string CsvFrom(DataTable dataTable)
        {
            StringBuilder csvBuilder = new StringBuilder();

            // 添加 CSV 標題行
            foreach (DataColumn col in dataTable.Columns)
            {
                csvBuilder.Append(col.ColumnName + ",");
            }
            csvBuilder.AppendLine();

            // 添加物件列表的數據行
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (DataColumn col in dataTable.Columns)
                {
                    csvBuilder.Append(row[col] + ",");
                }
                csvBuilder.AppendLine();
            }

            return csvBuilder.ToString();
        }

    }
}
