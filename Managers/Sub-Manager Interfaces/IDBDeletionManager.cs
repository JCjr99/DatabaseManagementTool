using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManagementTool
{
    public interface IDBDeletionManager
    {
        
        Task DeleteTableAsync(Type type, CancellationToken cancellationToken = default);
    }
}
