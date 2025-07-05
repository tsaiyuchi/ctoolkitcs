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




        public static DataTable CsvToDataTable(string csv) { return TextFieldToDataTable(csv, ","); }
        public static List<T> CsvToList<T>(string csv) where T : new() { return TextFieldToList<T>(csv, ","); }


        public static string CsvFrom<T>(List<T> objectList) { return TextFieldFrom(objectList, ","); }
        public static string CsvFrom(DataTable dataTable) { return TextFieldFrom(dataTable, ","); }



        public static DataTable IsvToDataTable(string isv) { return TextFieldToDataTable(isv, "\t"); }
        public static List<T> IsvToList<T>(string isv) where T : new() { return TextFieldToList<T>(isv, "\t"); }


        public static string IsvFrom<T>(List<T> objectList) { return TextFieldFrom(objectList, "\t"); }
        public static string IsvFrom(DataTable dataTable) { return TextFieldFrom(dataTable, "\t"); }




        public static DataTable TextFieldToDataTable(string text, params string[] delimiters)
        {
            DataTable dataTable = new DataTable();

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            using (TextFieldParser parser = new TextFieldParser(ms))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(delimiters);
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
        public static List<T> TextFieldToList<T>(string text, params string[] delimiters) where T : new()
        {
            var rtn = new List<T>();
            var datatable = TextFieldToDataTable(text, delimiters);
            var type = typeof(T);

            foreach (DataRow row in datatable.Rows)
            {
                var entity = new T();
                foreach (DataColumn col in datatable.Columns)
                {
                    var member = type.GetMember(col.ColumnName).FirstOrDefault();
                    if (member == null) continue;

                    if (member.MemberType == MemberTypes.Field)
                    {
                        var field = member as FieldInfo;
                        if (field == null) continue;

                        if (field.FieldType.IsEnum)
                        {
                            var val = CtkUtil.EnumParse(row[col] as string, field.FieldType);
                            field.SetValue(entity, val);
                        }
                        else
                        {
                            var val = Convert.ChangeType(row[col] as string, field.FieldType);
                            field.SetValue(entity, val);
                        }

                    }
                    else if (member.MemberType == MemberTypes.Property)
                    {
                        var prop = member as PropertyInfo;
                        if (prop == null) continue;
                        if (prop.PropertyType.IsEnum)
                        {
                            var val = CtkUtil.EnumParse(row[col] as string, prop.PropertyType);
                            prop.SetValue(entity, val);
                        }
                        else
                        {
                            var val = Convert.ChangeType(row[col] as string, prop.PropertyType);
                            prop.SetValue(entity, val);
                        }

                    }

                }
                rtn.Add(entity);
            }

            return rtn;
        }


        public static string TextFieldFrom<T>(List<T> objectList, string delimiter)
        {
            StringBuilder textBuilder = new StringBuilder();
            var type = typeof(T);

            // 添加 CSV 標題行
            var members = type.GetMembers();
            foreach (var mem in members)
            {
                if (mem.MemberType == System.Reflection.MemberTypes.Field
                    || mem.MemberType == System.Reflection.MemberTypes.Property)
                    textBuilder.Append(mem.Name + delimiter);
            }
            textBuilder.AppendLine();

            // 添加物件列表的數據行
            foreach (var obj in objectList)
            {
                foreach (var mem in members)
                {
                    var field = mem as FieldInfo;
                    var prop = mem as PropertyInfo;

                    if (field != null)
                        textBuilder.Append(field.GetValue(obj) + delimiter);
                    else if (prop != null)
                        textBuilder.Append(prop.GetValue(obj) + delimiter);
                }
                textBuilder.AppendLine();
            }

            return textBuilder.ToString();
        }
        public static string TextFieldFrom(DataTable dataTable, string delimiter)
        {
            StringBuilder textBuilder = new StringBuilder();

            // 添加 CSV 標題行
            foreach (DataColumn col in dataTable.Columns)
            {
                textBuilder.Append(col.ColumnName + delimiter);
            }
            textBuilder.AppendLine();

            // 添加物件列表的數據行
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (DataColumn col in dataTable.Columns)
                {
                    textBuilder.Append(row[col] + delimiter);
                }
                textBuilder.AppendLine();
            }

            return textBuilder.ToString();
        }




    }
}
