using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManagementTool.Managers.Sub_managers
{
    public class DBTableManager
    {
        private readonly DBConnectionManager connectionManager;

        public DBTableManager(DBConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;
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

        public async Task DeleteTableAsync(Type type, CancellationToken cancellationToken = default)
        {
            using (var connection = connectionManager.CreateConnection())
            {
                var command = connection.CreateCommand();
                string tableName = type.Name;

                string query = $"DROP TABLE IF EXISTS {tableName}";

                command.CommandText = query;
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            Console.WriteLine($"Table \"{type.Name}\" deleted successfully");
        }

        private string GetColumns(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var columns = new List<string>();

            foreach (var property in properties)
            {
                if (DBManagerHelper.IsComplexType(property.PropertyType))
                {
                    columns.Add($"{property.Name}_Id TEXT NOT NULL UNIQUE");
                }
                else
                {
                    string columnType = DBManagerHelper.GetSqliteColumnType(property.PropertyType);
                    columns.Add($"{property.Name} {columnType}");
                }
            }
            return string.Join(", ", columns);
        }
    }
}
