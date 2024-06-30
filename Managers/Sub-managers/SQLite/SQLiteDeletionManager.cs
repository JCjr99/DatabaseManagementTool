using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManagementTool
{
    public class SQLiteDeletionManager: IDBDeletionManager
    {
        private readonly SQLiteConnectionManager connectionManager;

        public SQLiteDeletionManager(SQLiteConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;
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

      
    }
}
