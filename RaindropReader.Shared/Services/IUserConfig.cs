using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services
{
    /// <summary>
    /// A context to read and write a user's config. The instance of this class
    /// is not thread safe and will be disposed by the IUserConfigStorage by the
    /// end of the session. When multiple sessions are opened for the same user,
    /// this class should handle synchronization and avoid data racing.
    /// </summary>
    public interface IUserConfig
    {
        /// <summary>
        /// The user ID. This value should match the IdentityUser.Id for a shared
        /// server.
        /// </summary>
        string UserID { get; }

        /// <summary>
        /// Get the ItemGuid of the type of all types. This type is provided by
        /// the service through internal bootstrapping and cannot be changed.
        /// </summary>
        Guid TypeItemGuid { get; }

        /// <summary>
        /// All storages of this user. The returned storages are disposed when this
        /// IUserConfig is disposed. The returned collection is a snapshot of the list
        /// and will not reflect changes from local or remote.
        /// </summary>
        /// <returns></returns>
        ICollection<IUserDataStorage> GetAllStorages();
        Task<ICollection<IUserDataStorage>> GetAllStoragesAsync();

        /// <summary>
        /// All types configured by this user. Each item of this user must correspond
        /// to one type in this collection. The returned collection is a snapshot of
        /// the list and will not reflect changes from local or remote.
        /// </summary>
        /// <returns></returns>
        ICollection<IUserItemTypeInfo> GetAllTypes();
        Task<ICollection<IUserItemTypeInfo>> GetAllTypesAsync();
    }
}
