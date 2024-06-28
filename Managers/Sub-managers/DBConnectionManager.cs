using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace DatabaseManagementTool.Managers.Sub_managers
{
    public class DBConnectionManager
    {
        private readonly string connectionString;
        public DBConnectionManager(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public SQLiteConnection CreateConnection()
        {
            var connection = new SQLiteConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}
