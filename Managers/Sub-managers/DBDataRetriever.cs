using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManagementTool.Managers.Sub_managers
{
    public class DBDataRetriever
    {
        private readonly DBConnectionManager connectionManager;
        public DBDataRetriever(DBConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;
        }
        public async Task ReadAllAsync(Type table, CancellationToken cancellationToken = default)
        {
            using (var connection = connectionManager.CreateConnection())
            {
                var query = $"SELECT * FROM {table.Name}";
                var command = connection.CreateCommand();
                command.CommandText = query;
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write(reader.GetValue(i) + " ");
                        }
                        Console.WriteLine();
                    }
                }
            }
        }
        public async Task GetTableInfoAsync(Type table, CancellationToken cancellationToken = default)
        {
            using (var connection = connectionManager.CreateConnection())
            {
                var query = $"PRAGMA table_info({table.Name})";
                var command = connection.CreateCommand();
                command.CommandText = query;
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        Console.WriteLine($"Column Name: {reader["name"]}, Column Type: {reader["type"]}");
                    }
                }
            }
        }
    }
}
