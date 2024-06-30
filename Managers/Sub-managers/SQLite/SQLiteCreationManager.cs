
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManagementTool
{
    public class SQLiteCreationManager : IDBCreationManager
    {
        private readonly SQLiteConnectionManager connectionManager;

        public SQLiteCreationManager(SQLiteConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;
        }

        public async Task<Guid> InsertObjectAsync(object obj, CancellationToken cancellationToken = default)
        {
            using (var connection = connectionManager.CreateConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var command = connection.CreateCommand();
                    command.Transaction = transaction;
                    var type = obj.GetType();
                    var tableName = type.Name;

                    if (!await TableExistsAsync(connection, tableName, cancellationToken))
                    {
                        throw new InvalidOperationException($"Table '{tableName}' does not exist.");
                    }

                    var parameters = await GetParametersAsync(obj, cancellationToken);

                    var query = $"INSERT INTO {tableName} ({string.Join(", ", parameters.Keys)}) VALUES ({string.Join(", ", parameters.Keys.Select(k => $"@{k}"))})";

                    command.CommandText = query;

                    foreach (var parameter in parameters)
                    {
                        command.Parameters.AddWithValue($"@{parameter.Key}", parameter.Value);
                    }

                    await command.ExecuteNonQueryAsync(cancellationToken);
                    transaction.Commit();

                    return Guid.Parse(parameters["Guid"].ToString());
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    // Log exception here
                    throw new InvalidOperationException($"Failed to insert object into table '{obj.GetType().Name}'.", ex);
                }
            }
        }

        public async Task CreateTableAsync(Type type, CancellationToken cancellationToken = default)
        {
            using (var connection = connectionManager.CreateConnection())
            {
                var command = connection.CreateCommand();
                string tableName = type.Name;
                string columns = GetColumns(type);

                columns = $"Id INTEGER PRIMARY KEY AUTOINCREMENT, Guid TEXT NOT NULL UNIQUE, {columns}";

                string query = $"CREATE TABLE IF NOT EXISTS {tableName} ({columns})";

                command.CommandText = query;
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        //change to use connection manager
        private async Task<bool> TableExistsAsync(SQLiteConnection connection, string tableName, CancellationToken cancellationToken)
        {
            var query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@tableName", tableName);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    return reader.HasRows;
                }
            }
        }

        private async Task<Dictionary<string, object>> GetParametersAsync(object obj, CancellationToken cancellationToken)
        {
            var parameters = new Dictionary<string, object>();

            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (IsComplexType(property.PropertyType))
                {
                    var nestedObject = property.GetValue(obj);
                    var nestedGuid = await InsertObjectAsync(nestedObject, cancellationToken);

                    parameters.Add($"{property.Name}_Id", nestedGuid.ToString());
                }
                else
                {
                    parameters.Add(property.Name, property.GetValue(obj) ?? DBNull.Value);
                }
            }

            parameters.Add("Guid", Guid.NewGuid().ToString("D"));

            return parameters;
        }

        private bool IsComplexType(Type type)
        {
            return type.IsClass && type != typeof(string);
        }

        
        private string GetColumns(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var columns = new List<string>();

            foreach (var property in properties)
            {
                if (IsComplexType(property.PropertyType))
                {
                    columns.Add($"{property.Name}_Id TEXT NOT NULL UNIQUE");
                }
                else
                {
                    string columnType = GetSqliteColumnType(property.PropertyType);
                    columns.Add($"{property.Name} {columnType}");
                }
            }
            return string.Join(", ", columns);
        }

        private string GetSqliteColumnType(Type type)
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

       

    }
}
