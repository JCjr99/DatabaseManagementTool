using DatabaseManagementTool.Managers.Sub_managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManagementTool.Managers
{
    public class DBManager
    {
        private readonly DBConnectionManager connectionManager;
        private readonly DBTableManager tableManager;
        private readonly DBDataInserter dataInserter;
        private readonly DBDataRetriever dataRetriever;

        public DBManager(string connectionString)
        {
            connectionManager = new DBConnectionManager(connectionString);
            tableManager = new DBTableManager(connectionManager);
            dataInserter = new DBDataInserter(connectionManager);
            dataRetriever = new DBDataRetriever(connectionManager);
        }

        public async Task<Guid> InsertObjectAsync(object obj, CancellationToken cancellationToken = default)
        {
            return await dataInserter.InsertObjectAsync(obj, cancellationToken);
        }

        public async Task ReadAllAsync(Type table, CancellationToken cancellationToken = default)
        {
            await dataRetriever.ReadAllAsync(table, cancellationToken);
        }

        public async Task GetTableInfoAsync(Type table, CancellationToken cancellationToken = default)
        {
            await dataRetriever.GetTableInfoAsync(table, cancellationToken);
        }

        public async Task CreateTableAsync(Type type, CancellationToken cancellationToken = default)
        {
            await tableManager.CreateTableAsync(type, cancellationToken);
        }

        public async Task DeleteTableAsync(Type type, CancellationToken cancellationToken = default)
        {
            await tableManager.DeleteTableAsync(type, cancellationToken);
        }
    }
}
