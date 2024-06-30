
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManagementTool
{
    public static class DBManagerFactory
    {
        

        public static (IDBConnectionManager, IDBCreationManager, IDBReadingManager, IDBDeletionManager ) CreateDBManager(DBType dbType, string connectionString)
        {
            switch (dbType)
            {
                case DBType.SQLite:
                    var connectionManager = new SQLiteConnectionManager(connectionString);
                    var creationManager = new SQLiteCreationManager(connectionManager);
                    var readingManager = new SQLiteReadingManager(connectionManager);
                    var deletionManager = new SQLiteDeletionManager(connectionManager);
                    
                    return (connectionManager, creationManager, readingManager, deletionManager);
                // Add other cases here for different DB types
                default:
                    throw new ArgumentException("Invalid database type", nameof(dbType));
            }
        }
    }

}
