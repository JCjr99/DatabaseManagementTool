using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManagementTool
{
    internal static class DBManagerHelper
    {
        public static List<string> GetColumnDefinitions(object obj, string prefix = "")
        {
            var columns = new List<string>();
            if (obj == null)
            {
                return columns;
            }
            foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var columnName = $"{prefix}{property.Name}";
                var columnType = GetSqliteColumnType(property.PropertyType);

                if (columnType == null)
                {
                    columns.AddRange(GetColumnDefinitions(property.GetValue(obj), columnName + "_"));
                }
                else
                {
                    columns.Add($"{columnName} {columnType}");
                }
            }

            return columns;
        }

        public static string GetSqliteColumnType(Type type)
        {
            if (type == typeof(int) || type == typeof(long) || type == typeof(short))
                return "INTEGER";
            else if (type == typeof(float) || type == typeof(double))
                return "REAL";
            else if (type == typeof(string) || type == typeof(char))
                return "TEXT";
            else if (type == typeof(bool))
                return "INTEGER";
            else if (type == typeof(DateTime))
                return "DATETIME";
            else if (type.IsArray || type.IsGenericType)
                return "BLOB";

            return null;
        }

        public static void GetColumnNamesAndValues(object obj, List<string> columnNames, List<string> values, string prefix = "")
        {
            if (obj == null) return;
            foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var columnName = $"{prefix}{property.Name}";

                if (IsComplexType(property.PropertyType))
                {
                    GetColumnNamesAndValues(property.GetValue(obj), columnNames, values, columnName + "_");
                }
                else
                {
                    columnNames.Add(columnName);
                    values.Add(columnName);
                }
            }
        }

        public static bool IsComplexType(Type type)
        {
            return type.IsClass && type != typeof(string);
        }

        public static object GetValue(object obj, string propertyName)
        {
            var propertyNames = propertyName.Split('_');
            var currentObj = obj;

            foreach (var propertyNamePart in propertyNames)
            {
                var property = currentObj.GetType().GetProperty(propertyNamePart);
                currentObj = property.GetValue(currentObj);
            }

            return currentObj;
        }
    }
}
