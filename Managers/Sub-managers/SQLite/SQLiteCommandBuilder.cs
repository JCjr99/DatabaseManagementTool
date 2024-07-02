using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace DatabaseManagementTool
{
    internal class SQLiteCommandBuilder
    {
        private List<SQLiteCommand> commandList;
        
        public SQLiteCommandBuilder()
        {
            commandList = new List<SQLiteCommand>();
        }
        public async Task<List<SQLiteCommand>> GetSQLiteCommands(SQLiteTransaction transaction, object obj, string mode, CancellationToken cancellationToken) 
        {
            await BuildCommandAsync(transaction, obj, mode, cancellationToken);
            return commandList;
        }
        private async Task<Guid> BuildCommandAsync(SQLiteTransaction transaction, object obj, string mode, CancellationToken cancellationToken)
        {
            var command = transaction.Connection.CreateCommand();
            command.Transaction = transaction;
            string tableName = obj.GetType().Name;

            var parameters = await GetParametersAsync(transaction,obj, mode, cancellationToken);
            var query = BuildQueryAsync(tableName, parameters, mode);
           
            command.CommandText = query;
            foreach (var parameter in parameters)
            {
                command.Parameters.AddWithValue($"@{parameter.Key}", parameter.Value);
            }
            commandList.Add(command);
            return Guid.Parse(command.Parameters["Guid"].ToString());
            
        }

        public string BuildQueryAsync(string tableName, Dictionary<string, object> parameters, string mode)
        {
            var query = "";
            switch (mode)
            {
                case "INSERT":
                    query = $"INSERT INTO {tableName} ({string.Join(", ", parameters.Keys)}) VALUES ({string.Join(", ", parameters.Keys.Select(k => $"@{k}"))})";
                    break;
                default:
                    throw new ArgumentException("Mode argument is not one of the available types");


            }
            return query;
        }
        private async Task<Dictionary<string, object>> GetParametersAsync(SQLiteTransaction transaction, object obj, string mode, CancellationToken cancellationToken)
        {
            var parameters = new Dictionary<string, object>();

            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (IsComplexType(property.PropertyType))
                {
                    var nestedObject = property.GetValue(obj);
                    var nestedGuid = await BuildCommandAsync(transaction, nestedObject, mode, cancellationToken);

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


      

    }
}
