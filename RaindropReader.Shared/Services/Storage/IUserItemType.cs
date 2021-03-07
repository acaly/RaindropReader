using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Storage
{
    /// <summary>
    /// Represent a type of a user. Unlike Item, this class is not
    /// immutable, but is shared and thread-safe. It always represents 
    /// the latest state of the type in the case of changing.
    /// </summary>
    public interface IUserItemType
    {
        /// <summary>
        /// The ItemGuid of the item that stores info of this type. This
        /// is also used as the GUID of this type.
        /// </summary>
        Guid ItemGuid { get; }

        /// <summary>
        /// The StorageGuid of the storage that stores items of this
        /// type.
        /// </summary>
        Guid StorageGuid { get; }

        /// <summary>
        /// The data of this type, including its name.
        /// </summary>
        UserItemTypeInfo Info { get; }

        /// <summary>
        /// Returns whether this type is valid, that is, has not been deleted.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Try to delete the type. Will fail and return false if there
        /// are items that use this type. This operation requires modification
        /// lock on the storage for types.
        /// </summary>
        /// <returns>True if succeeded; false otherwise.</returns>
        bool TryDelete();
        Task<bool> TryDeleteAsync();

        /// <summary>
        /// Move all items of this type to a new storage. This will keep
        /// the ItemGuid and VersionGuid of all items. This operation requires
        /// the modification lock on both storages.
        /// </summary>
        /// <param name="newStorage"></param>
        void MoveToStorage(IUserDataStorage newStorage);
        Task MoveToStorageAsync(IUserDataStorage newStorage);
    }
}
