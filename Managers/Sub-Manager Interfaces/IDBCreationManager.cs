using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManagementTool
{
    public interface IDBCreationManager
    {
        Task<Guid> InsertObjectAsync(object obj, CancellationToken cancellationToken = default);
        Task CreateTableAsync(Type type, CancellationToken cancellationToken = default);

    }
}
