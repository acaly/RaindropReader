using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Storage
{
    public class BeforeTypeDeleteEventArgs
    {
        public Guid TypeGuid { get; init; }
        public bool IsCancelled { get; private set; }

        //TODO we should provide some information to the user (e.g., which plugin is
        //using this type).
        public void Cancel()
        {
            IsCancelled = true;
        }
    }

    /// <summary>
    /// A context to access a user's config (including system-defined types).
    /// A instance of this class is not thread safe and will be disposed by
    /// the IUserConfigStorage by the end of the session. When multiple sessions
    /// are opened for the same user, this class should handle synchronization
    /// and avoid data racing between different instances.
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
        /// Get the storage that should be used by the system-defined types. This storage
        /// should be provided for new users by IStorageProvider, while other storages
        /// might be added by plugins.
        /// </summary>
        /// <returns></returns>
        IUserDataStorage GetSystemStorage();

        /// <summary>
        /// Get the type info instance given the GUID, which is both the type GUID of
        /// the type and the item GUID of the type's corresponding item. Returns null
        /// if not found.
        /// </summary>
        /// <param name="typeItemGuid">GUID of the type</param>
        /// <returns></returns>
        IUserItemType GetType(Guid typeGuid);

        /// <summary>
        /// Raised before the type is deleted from local client. Plugin manager will
        /// use this event to prevent the operation. Note that this does not work for
        /// remote changes if the storages is shared. It is assumed that each client
        /// implements appropriate check before deleting.
        /// </summary>
        event EventHandler<BeforeTypeDeleteEventArgs> BeforeTypeDelete;
    }
}
