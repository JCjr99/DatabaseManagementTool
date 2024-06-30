
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManagementTool
{
    public class DBManager: IDBCreationManager, IDBReadingManager, IDBDeletionManager
    {
        private readonly IDBConnectionManager connectionManager ;
        private readonly IDBCreationManager creationManager;
        private readonly IDBReadingManager readingManager;
        private readonly IDBDeletionManager deletionManager;
        public DBManager(string connectionString, DBType dbtype)
        {
            (connectionManager, creationManager, readingManager, deletionManager) = DBManagerFactory.CreateDBManager(dbtype, connectionString);
        }

        public async Task CreateTableAsync(Type type, CancellationToken cancellationToken = default)
        {
            await creationManager.CreateTableAsync(type, cancellationToken);
        }

        public async Task<Guid> InsertObjectAsync(object obj, CancellationToken cancellationToken = default)
        {
            return await creationManager.InsertObjectAsync(obj, cancellationToken);
        }

        public async Task ReadAllAsync(Type table, CancellationToken cancellationToken = default)
        {
            await readingManager.ReadAllAsync(table, cancellationToken);
        }

        public async Task GetTableInfoAsync(Type table, CancellationToken cancellationToken = default)
        {
            await readingManager.GetTableInfoAsync(table, cancellationToken);
        }

    
        public async Task DeleteTableAsync(Type type, CancellationToken cancellationToken = default)
        {
            await deletionManager.DeleteTableAsync(type, cancellationToken);
        }
    }
}
