using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManagementTool
{
    public interface IDBReadingManager
    {
        Task ReadAllAsync(Type table, CancellationToken cancellationToken = default);
        Task GetTableInfoAsync(Type table, CancellationToken cancellationToken = default);
    }
}
