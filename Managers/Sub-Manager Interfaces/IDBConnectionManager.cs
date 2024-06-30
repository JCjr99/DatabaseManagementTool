using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManagementTool
{
    public interface IDBConnectionManager  
    {
        T CreateConnection<T>();
    }
}
