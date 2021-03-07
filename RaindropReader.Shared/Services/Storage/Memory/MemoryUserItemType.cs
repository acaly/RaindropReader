using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Storage.Memory
{
    internal sealed class MemoryUserItemType : IUserItemType
    {
        public MemoryUserConfig Owner { get; init; }
        public Guid ItemGuid { get; init; }
        public Guid StorageGuid { get; init; }

        public UserItemTypeInfo Info { get; internal set; }
        public bool IsValid { get; internal set; }

        public void MoveToStorage(IUserDataStorage newStorage)
        {
            throw new NotSupportedException("Memory storage does not support moving.");
        }

        public Task MoveToStorageAsync(IUserDataStorage newStorage)
        {
            MoveToStorage(newStorage);
            return Task.CompletedTask;
        }

        public bool TryDelete()
        {
            return Owner.TryDeleteType(this);
        }

        public Task<bool> TryDeleteAsync()
        {
            return Task.FromResult(TryDelete());
        }
    }
}
