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
    /// Part of the ReaderService that manages plugins.
    /// </summary>
    public sealed class PluginManager : IDisposable
    {
        private readonly Guid _systemPluginGuid = Guid.NewGuid();

        private bool _disposed = false;

        internal ReaderService ReaderService { get; }
        internal SystemPlugin SystemPlugin { get; private set; }

        private readonly List<IPluginProvider> _providers = new();
        private readonly Dictionary<Guid, PluginHandlerRegistry> _loadedPlugins = new();
        private IUserItemType _pluginType;

        internal PluginManager(ReaderService readerService)
        {
            ReaderService = readerService;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            foreach (var plugin in _loadedPlugins.Values)
            {
                plugin.Unload();
            }
            _loadedPlugins.Clear();
        }

        //Basic API.

        public void AddPluginProvider(IPluginProvider pluginProvider)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PluginManager));
            }
            _providers.Add(pluginProvider);
        }

        internal async Task LoadPluginAsync(Guid instanceGuid, UserPluginInstanceInfo info, bool overwriteExisting)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PluginManager));
            }
            //Check existing.
            if (_loadedPlugins.TryGetValue(instanceGuid, out var existingPlugin))
            {
                if (overwriteExisting)
                {
                    throw new NotImplementedException("Plugin reloading has not been implemented.");
                }
                throw new ArgumentException("Plugin with the same Guid has been loaded. " +
                    $"Existing plugin: {existingPlugin.InstanceInfo.PluginName}. New plugin: {info.PluginName}.");
            }

            //Get the IPlugin interface instance.
            IPlugin pluginInstance = null;
            if (info.PluginName is null && SystemPlugin is null)
            {
                //System plugin uses null as name (only loaded by PluginManager).
                SystemPlugin = new SystemPlugin();
                pluginInstance = SystemPlugin;
            }
            else
            {
                foreach (var provider in _providers)
                {
                    var instance = provider.CreatePlugin(info.PluginName);
                    if (instance is not null)
                    {
                        pluginInstance = instance;
                        break;
                    }
                }
                if (pluginInstance is null)
                {
                    throw new Exception($"Cannot found plugin name {info.PluginName}.");
                }
            }

            //Load the plugin.
            pluginInstance.Init(info.PluginParameters);
            var reg = new PluginHandlerRegistry()
            {
                InstanceInfo = info,
                Owner = this,
            };
            await pluginInstance.LoadAsync(reg);

            //Add to list.
            _loadedPlugins.Add(instanceGuid, reg);
        }

        internal void UnloadPlugin(Guid instanceGuid)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PluginManager));
            }
            if (instanceGuid == _systemPluginGuid)
            {
                throw new InvalidOperationException("System plugin cannot be unloaded.");
            }
            if (_loadedPlugins.Remove(instanceGuid, out var pluginReg))
            {
                pluginReg.Unload();
            }
        }

        //Reader client integration.

        internal async Task InitAsync()
        {
            if (_pluginType is not null)
            {
                throw new InvalidOperationException("InitAsync can only be called once.");
            }
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PluginManager));
            }

            var userConfig = ReaderService.UserConfig;
            var storage = userConfig.GetSystemStorage();

            var existingType = userConfig.GetType(SystemTypeGuids.Plugin);
            if (existingType is null)
            {
                //TODO timeout
                using (var storageLock = await storage.LockStorageAsync(1000))
                {
                    if (storageLock is null)
                    {
                        throw new Exception("Cannot access user storage");
                    }

                    var typeType = userConfig.GetType(SystemTypeGuids.Type);

                    //Create plugin type.
                    storage.AddItemVersion(new Item(typeType, SystemTypeGuids.Plugin,
                        data: new UserItemTypeInfo { DisplayName = "[Types.Plugins]" }));
                }
                existingType = userConfig.GetType(SystemTypeGuids.Plugin);
            }
            _pluginType = existingType;

            //Load system plugin.
            await LoadPluginAsync(_systemPluginGuid, new(), overwriteExisting: false);

            //Start listen to type change event before loading anything else.
            userConfig.BeforeTypeDelete += BeforeUserTypeDelete;

            //Load all plugins.
            var itemList = new List<Item>();
            await ReaderService.UserConfig.GetSystemStorage().GetItemsAsync(_pluginType, default, -1, itemList);
            foreach (var item in itemList)
            {
                if (!item.IsValid) continue;
                var pluginInfo = ItemDataHelper.FromItemData<UserPluginInstanceInfo>(item.Data);
                try
                {
                    await LoadPluginAsync(item.ItemGuid, pluginInfo, overwriteExisting: false);
                }
                catch (Exception e)
                {
                    //TODO properly output the error
                    Console.WriteLine(e);
                }
            }
        }

        private void BeforeUserTypeDelete(object sender, BeforeTypeDeleteEventArgs e)
        {
            if (e.IsCancelled) return;
            foreach (var plugin in _loadedPlugins.Values)
            {
                if (plugin.ContainsType(e.TypeGuid))
                {
                    e.Cancel();
                    return;
                }
            }
        }

        internal (Guid guid, PluginHandlerRegistry reg)[] GetLoadedPluginsInternal()
        {
            return _loadedPlugins.Select(pair => (pair.Key, pair.Value)).ToArray();
        }

        //TODO need a public API to get all loaded plugins (to show to user)

        public async Task<Guid> AddPluginAsync(string pluginName, string data)
        {
            var guid = Guid.NewGuid();
            var info = new UserPluginInstanceInfo
            {
                PluginName = pluginName,
                PluginParameters = data,
            };
            var item = new Item(_pluginType, guid, data: info);

            //TODO timeout
            using var storageLock = await ReaderService.UserConfig.GetSystemStorage().LockStorageAsync(1000);

            //Load before modifying storage (so it will not be loaded ever if we can't load it now).
            await LoadPluginAsync(guid, info, overwriteExisting: false);
            ReaderService.UserConfig.GetSystemStorage().AddItemVersion(item);

            return guid;
        }

        public async Task RemovePluginAsync(Guid guid)
        {
            var storage = ReaderService.UserConfig.GetSystemStorage();

            //TODO timeout
            using (var storageLock = await storage.LockStorageAsync(1000))
            {
                var item = storage.GetItemFromItemGuid(guid, false);
                if (item is null)
                {
                    throw new Exception("Plugin does not exist");
                }
                //Add the deleted version.
                storage.AddItemVersion(new Item(_pluginType, guid, byteData: null));
            }

            UnloadPlugin(guid);
        }

        public IUserDataStorage[] GetAllStorages()
        {
            return Array.Empty<IUserDataStorage>();
        }
    }
}
