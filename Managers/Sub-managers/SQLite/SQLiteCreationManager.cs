
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
        private readonly SQLiteCommandBuilder queryBuilder;   

        public SQLiteCreationManager(SQLiteConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;
            this.queryBuilder = new SQLiteCommandBuilder();
        }

        public async Task InsertObjectAsync(object obj, CancellationToken cancellationToken = default)
        {
            using (var connection = connectionManager.CreateConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                   
                   
                    var commands = await queryBuilder.GetSQLiteCommands(transaction, obj, "INSERT", cancellationToken);
                    foreach (SQLiteCommand command in commands)
                    {
                        //needs changed to get name from command
                        string tableName = obj.GetType().Name;
                        if (!await TableExistsAsync(transaction, tableName, cancellationToken))
                        {
                            await CreateTableAsync(obj.GetType(), cancellationToken);
                        }
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }

                    transaction.Commit();

                    
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
        
        private async Task<bool> TableExistsAsync(SQLiteTransaction transaction, string tableName, CancellationToken cancellationToken)
        {
            var query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName";
            using (var command = new SQLiteCommand(query, transaction.Connection))
            {
                command.Parameters.AddWithValue("@tableName", tableName);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    return reader.HasRows;
                }
            }
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
        private bool IsComplexType(Type type)
        {
            return type.IsClass && type != typeof(string);
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
