using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManagementTool.Managers.Sub_managers
{
    public class DBDataInserter
    {
        private readonly DBConnectionManager connectionManager;

        public DBDataInserter(DBConnectionManager connectionManager)
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
                if (DBManagerHelper.IsComplexType(property.PropertyType))
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
    }
}
