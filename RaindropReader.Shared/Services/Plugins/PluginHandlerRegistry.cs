using RaindropReader.Shared.Components;
using RaindropReader.Shared.Services.Client;
using RaindropReader.Shared.Services.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Plugins
{
    /// <summary>
    /// Public API exposed by PluginLoader for plugins to register its handlers. Each plugin
    /// will be provided with its own instance of this class. This also acts as the container
    /// of a plugin, and handles its lifetime.
    /// </summary>
    public sealed class PluginHandlerRegistry
    {
        internal PluginManager Owner { get; init; }
        public UserPluginInstanceInfo InstanceInfo { get; init; }
        public ReaderService ReaderService => Owner.ReaderService;

        internal PluginHandlerRegistry()
        {
        }

        private bool _regFinished = false;
        private readonly HashSet<Guid> _registeredTypes = new();
        private readonly List<SideBarElement> _sideBarElements = new();
        private readonly List<ScheduledPluginTask> _tasks = new();

        internal void FinishRegistration()
        {
            _regFinished = true;
        }

        internal bool ContainsType(Guid guid)
        {
            return _registeredTypes.Contains(guid);
        }

        public void RegisterTypeGuid(Guid typeGuid)
        {
            if (_regFinished)
            {
                throw new InvalidOperationException("This method cannot be called after registration ends.");
            }
            _registeredTypes.Add(typeGuid);
        }

        /// <summary>
        /// Register a static type defined by the plugin. A static type is a type which
        /// is only created once even when multiple instances of plugin exist.
        /// If the type does not exist, this method creates the type. If the type already
        /// exists, this method does not modify it (even if data provided in info is
        /// different).
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="info"></param>
        public async Task RegisterStaticTypeAsync(Guid guid, UserItemTypeInfo info)
        {
            if (_regFinished)
            {
                throw new InvalidOperationException("This method cannot be called after registration ends.");
            }
            var userConfig = Owner.ReaderService.UserConfig;
            if (userConfig.GetType(guid) is not null)
            {
                var storage = userConfig.GetSystemStorage();
                //TODO timeout
                using var storageLock = await storage.LockStorageAsync(1000);
                var typeType = userConfig.GetType(SystemTypeGuids.Type);
                storage.AddItemVersion(new Item(typeType, guid, data: info));
            }
            RegisterTypeGuid(guid);
        }

        internal IEnumerable<SideBarElement> GetSideBarElements() => _sideBarElements;

        public void RegisterSideBarElement(SideBarElement sideBarElement)
        {
            if (_regFinished)
            {
                throw new InvalidOperationException("This method cannot be called after registration ends.");
            }
            _sideBarElements.Add(sideBarElement);
        }
        
        internal async Task RunScheduledTasks(DateTime timeUtcCheck)
        {
            try
            {
                foreach (var t in _tasks)
                {
                    await t.UpdateAsync(timeUtcCheck);
                }
            }
            catch (Exception e)
            {
                //TODO properly output the error
                Console.WriteLine(e);
            }
        }

        public void RegisterScheduledTask(Func<Task> task, TimeSpan interval)
        {
            if (_regFinished)
            {
                throw new InvalidOperationException("This method cannot be called after registration ends.");
            }
            _tasks.Add(new ScheduledPluginTask
            {
                Action = task,
                Interval = interval,
            });
        }

        //TODO other registration

        internal void Unload()
        {
            //Currently we don't need to unload anything. Removing the registry instance is enough.
        }
    }
}
