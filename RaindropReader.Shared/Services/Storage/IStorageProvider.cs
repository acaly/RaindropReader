using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Storage
{
    /// <summary>
    /// Scoped service that manages all users' configs. Refer to IUserConfig for
    /// details on user config.
    /// </summary>
    public interface IStorageProvider : IDisposable
    {
        /// <summary>
        /// For local apps (desktop), use this value as the userID parameter.
        /// </summary>
        public const string LocalUser = "LOCAL";

        /// <summary>
        /// Add a new user to the storage. The config is initialized to a default state.
        /// </summary>
        /// <param name="userID">The ID of the user that might be linked with the server db.</param>
        IUserConfig AddUser(string userID);
        Task<IUserConfig> AddUserAsync(string userID);

        /// <summary>
        /// Delete a user's config from the storage.
        /// </summary>
        /// <param name="userID">The ID of the user.</param>
        void DeleteUser(string userID);
        Task DeleteUserAsync(string userID);

        /// <summary>
        /// Get the config of a user. 
        /// </summary>
        /// <param name="userID">The ID of the user.</param>
        /// <returns>
        /// The config of the user. Returns null if the user is not in the storage (need to AddUser).
        /// The returned instance should only be used in the current session.
        /// </returns>
        IUserConfig GetUser(string userID);
        Task<IUserConfig> GetUserAsync(string userID);
    }
}
