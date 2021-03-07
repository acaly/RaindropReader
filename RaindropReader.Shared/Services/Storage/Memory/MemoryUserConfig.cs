using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Storage.Memory
{
    internal sealed class MemoryUserConfig : IUserConfig, IUserDataStorage, IDisposable
    {
        private static readonly Guid _storageGuid = default;

        public string UserID => IStorageProvider.LocalUser;
        public Guid TypeItemGuid => SystemTypeGuids.Type;
        public Guid StorageGuid => _storageGuid;

        private readonly Dictionary<Guid, MemoryUserItemType> _types = new();
        private readonly Dictionary<Guid, Item> _data = new(); //Version guid -> item
        private readonly Dictionary<Guid, Guid> _itemVersions = new(); //Item GUID -> latest version GUID
        private readonly MemoryUserItemType _typeType;
        private readonly ConcurrentDictionary<ChangeObservable, int> _observables = new();

        public MemoryUserConfig()
        {
            //Bootstrap type.
            var typeTypeInfo = new UserItemTypeInfo { DisplayName = "[Types.Types]" };
            //We use null as the type instance. When comparing with _typeType, which is also
            //null, the AddItemVersion method will treat this as a type.
            AddItemVersion(new Item(null, SystemTypeGuids.Type, data: typeTypeInfo));
            _typeType = _types.Values.First();
        }

        public void Dispose()
        {
            _observables.Clear();
            _itemVersions.Clear();
            _data.Clear();
            _types.Clear();
        }

        internal bool TryDeleteType(MemoryUserItemType type)
        {
            foreach (var versionGuid in _itemVersions.Values)
            {
                if (_data.TryGetValue(versionGuid, out var item) && item.ItemType == type && item.IsValid)
                {
                    return false;
                }
            }
            //Add a deleted version. The remaining work will be handled by AddItemVersion.
            AddItemVersion(new Item(_typeType, type.ItemGuid, data: null));
            return true;
        }

        //IUserConfig

        public IUserDataStorage GetSystemStorage()
        {
            return this;
        }

        public IUserItemType GetType(Guid typeGuid)
        {
            if (_types.TryGetValue(typeGuid, out var ret))
            {
                return ret;
            }
            return null;
        }

        //IUserDataStorage

        public void AddItemVersion(Item newItem)
        {
            UserItemTypeInfo newTypeInfo = null;
            //Be careful: the bootstrapping of type in ctor depends on this equality comparison.
            if (newItem.ItemType == _typeType && newItem.IsValid)
            {
                //Validate type info before we actually do anything.
                newTypeInfo = ItemDataHelper.FromItemData<UserItemTypeInfo>(newItem.Data);
            }
            _data[newItem.VersionGuid] = newItem;
            _itemVersions[newItem.ItemGuid] = newItem.VersionGuid;
            //Be careful: the bootstrapping of type in ctor depends on this equality comparison.
            if (newItem.ItemType == _typeType)
            {
                //The item represents a type. We need to update the MemoryUserItemType instance.
                if (_types.TryGetValue(newItem.ItemGuid, out var typeInstance))
                {
                    if (!newItem.IsValid)
                    {
                        //Delete.
                        typeInstance.IsValid = false;
                    }
                    else
                    {
                        //Update
                        typeInstance.Info = newTypeInfo;
                    }
                }
                else if (newItem.IsValid)
                {
                    //Create.
                    typeInstance = new MemoryUserItemType
                    {
                        Owner = this,
                        ItemGuid = newItem.ItemGuid,
                        IsValid = true,
                        StorageGuid = StorageGuid,
                        Info = newTypeInfo,
                    };
                    _types[newItem.ItemGuid] = typeInstance;
                }
            }

            //Trigger observables.
            foreach (var obs in _observables.Keys)
            {
                if (obs.Type == newItem.ItemType.ItemGuid)
                {
                    obs.TriggerLocal();
                }
            }
        }

        public Item GetItemFromItemGuid(Guid itemGuid, bool includeDeletion)
        {
            if (_itemVersions.TryGetValue(itemGuid, out var version) &&
                _data.TryGetValue(version, out var item))
            {
                if (!item.IsValid && !includeDeletion)
                {
                    return null;
                }
                return item;
            }
            return null;
        }

        public Task<Item> GetItemFromItemGuidAsync(Guid itemGuid, bool includeDeletion)
        {
            return Task.FromResult(GetItemFromItemGuid(itemGuid, includeDeletion));
        }

        public Item GetItemFromVersionGuid(Guid versionGuid)
        {
            if (_data.TryGetValue(versionGuid, out var item))
            {
                return item;
            }
            return null;
        }

        public Task<Item> GetItemFromVersionGuidAsync(Guid versionGuid)
        {
            return Task.FromResult(GetItemFromVersionGuid(versionGuid));
        }

        public int GetItems(IUserItemType type, DateTime fromTime, int limit, List<Item> receiveBuffer)
        {
            int count = 0;
            foreach (var versionGuid in _itemVersions.Values)
            {
                if (_data.TryGetValue(versionGuid, out var item) && item.ItemType == type && item.IsValid)
                {
                    receiveBuffer?.Add(item);
                    count += 1;
                }
            }
            return count;
        }

        public Task<int> GetItemsAsync(IUserItemType type, DateTime fromTime, int limit, List<Item> receiveBuffer)
        {
            return Task.FromResult(GetItems(type, fromTime, limit, receiveBuffer));
        }

        public Task<IDisposable> LockStorageAsync(int timeout)
        {
            //We don't need to lock, because this instance cannot be shared with other instances.
            return Task.FromResult<IDisposable>(new StorageLock());
        }

        public IItemsChangeObservable SubscribeItemsChange(Guid typeGuid)
        {
            return new ChangeObservable { Owner = this, Type = typeGuid };
        }

        private class StorageLock : IDisposable
        {
            public void Dispose()
            {
            }
        }

        private class ChangeObservable : IItemsChangeObservable
        {
            public MemoryUserConfig Owner { get; init; }
            public Guid Type { get; init; }

            public event EventHandler LocalItemsChanged;
            public event EventHandler RemoteItemsChanged;

            public void Dispose()
            {
                Owner._observables.TryRemove(this, out _);
            }

            public void TriggerLocal()
            {
                LocalItemsChanged?.Invoke(Owner, EventArgs.Empty);
            }
        }
    }
}
