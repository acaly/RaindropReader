using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Storage
{
    /// <summary>
    /// The service that provides storage of feed data. This class is not shared
    /// between users. The instances of this class are managed by the IUserConfig
    /// or plugins and are automatically disposed by the end of the session.
    /// When multiple sessions are opened for the same user, this class should 
    /// handle synchronization and avoid data racing between different instances.
    /// </summary>
    public interface IUserDataStorage
    {
        /// <summary>
        /// The GUID of this storage. It is referenced by a IUserItemTypeInfo.
        /// </summary>
        Guid StorageGuid { get; }

        /// <summary>
        /// Get an immutable Item instance of the latest version of the specified item.
        /// Returns null if the item is not found.
        /// </summary>
        /// <param name="itemGuid">The ItemGuid of the item.</param>
        /// <param name="includeDeletion">
        /// If set to true, this method will return the empty Item instance for deleted
        /// items. If set to false, this method will return null for deleted items.
        /// </param>
        /// <returns>The Item instance of the item.</returns>
        Item GetItemFromItemGuid(Guid itemGuid, bool includeDeletion);
        Task<Item> GetItemFromItemGuidAsync(Guid itemGuid, bool includeDeletion);

        /// <summary>
        /// Get an immutable Item instance of the specified version of an item. Returns
        /// null if the version is not found.
        /// </summary>
        /// <param name="versionGuid">The GUID of the version.</param>
        /// <returns>The Item instance of the version.</returns>
        Item GetItemFromVersionGuid(Guid versionGuid);
        Task<Item> GetItemFromVersionGuidAsync(Guid versionGuid);

        /// <summary>
        /// Get the collection of items of the specified type, filtered by timestamp, limited
        /// by the specified max number.
        /// </summary>
        /// <param name="type">The type of items to read.</param>
        /// <param name="fromTime">Earliest time of the items to read.</param>
        /// <param name="limit">Limit of the number of items to read.</param>
        /// <param name="receiveBuffer">
        /// The List object to receive the results. It will be cleared first. Can be null to get number.
        /// </param>
        /// <returns>The actual number added to the list.</returns>
        int GetItems(IUserItemType type, DateTime fromTime, int limit, List<Item> receiveBuffer);
        Task<int> GetItemsAsync(IUserItemType type, DateTime fromTime, int limit, List<Item> receiveBuffer);

        /// <summary>
        /// Lock the storage for writing. Other writers from the local and remote 
        /// will be blocked. All modifications of the storage must be protected
        /// by this lock. The modifications are submitted when the lock is released.
        /// </summary>
        /// <param name="timeout">Maximum wait time for the lock, in ms.</param>
        /// <returns>
        /// The task that returns the IDisposable, which should be disposed after 
        /// modification. If the storage is currently locked by another client,
        /// and the timeout has exceeded, the task returns null.
        /// </returns>
        Task<IDisposable> LockStorageAsync(int timeout);

        /// <summary>
        /// Create an observable instance that can raise events when items in
        /// the specified type have changed.
        /// </summary>
        /// <param name="typeGuid">The type of items to watch.</param>
        /// <returns>The observable that raises the events.</returns>
        IItemsChangeObservable SubscribeItemsChange(Guid typeGuid);

        /// <summary>
        /// Submite a new version of an item. This can be a new item, an update
        /// of an existing item, or the deletion of an existing item. Note that
        /// this change will not be submitted until the modification lock from
        /// LockStorageAsync() released.
        /// The item should be created after entering the modification lock to
        /// ensure adding to the correct storage (with concurrent call to
        /// IUserItemTypeInfo.MoveToStorage).
        /// </summary>
        /// <param name="newItem"></param>
        void AddItemVersion(Item newItem);
    }
}
