
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace DatabaseManagementTool
{
    public class SQLiteConnectionManager : IDBConnectionManager
    {
        private readonly string connectionString;

        public SQLiteConnectionManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public SQLiteConnection CreateConnection()
        {
            return new SQLiteConnection(connectionString);
        }

        // Explicitly implement the IDBConnectionManager interface's method
        T IDBConnectionManager.CreateConnection<T>()
        {
            if (typeof(T) == typeof(SQLiteConnection))
            {
                return (T)(object)new SQLiteConnection(connectionString);
            }

            throw new InvalidOperationException("Invalid connection type requested.");
        }
    }

}
