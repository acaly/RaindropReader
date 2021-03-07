using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Storage.Memory
{
    public sealed class MemoryStorageProvider : IStorageProvider
    {
        private MemoryUserConfig _localUser;

        private static void CheckUserID(string userID)
        {
            if (userID != IStorageProvider.LocalUser)
            {
                throw new NotSupportedException("Memory storage only supports local user.");
            }
        }

        public IUserConfig AddUser(string userID)
        {
            CheckUserID(userID);
            lock (this)
            {
                if (_localUser is not null)
                {
                    throw new InvalidOperationException("Local user has already been created.");
                }
                _localUser = new MemoryUserConfig();
                return _localUser;
            }
        }

        public Task<IUserConfig> AddUserAsync(string userID)
        {
            return Task.FromResult(AddUser(userID));
        }

        public void DeleteUser(string userID)
        {
            CheckUserID(userID);
            lock (this)
            {
                if (_localUser is null)
                {
                    throw new InvalidOperationException("Local user has not been created.");
                }
                DisposeLocalUser();
            }
        }

        public Task DeleteUserAsync(string userID)
        {
            DeleteUser(userID);
            return Task.CompletedTask;
        }

        public IUserConfig GetUser(string userID)
        {
            CheckUserID(userID);
            lock (this)
            {
                if (_localUser is null)
                {
                    throw new InvalidOperationException("Local user has not been created.");
                }
                return _localUser;
            }
        }

        public Task<IUserConfig> GetUserAsync(string userID)
        {
            return Task.FromResult(GetUser(userID));
        }

        private void DisposeLocalUser()
        {
            if (_localUser is not null)
            {
                _localUser.Dispose();
                _localUser = null;
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                DisposeLocalUser();
            }
        }
    }
}
