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
    public sealed class PluginManager
    {
        private readonly List<IPluginProvider> _providers = new();
        private readonly Dictionary<Guid, PluginHandlerRegistry> _loadedPlugins = new();
        private readonly ReaderService _readerService;
        private IUserItemType _pluginType;

        internal PluginManager(ReaderService readerService)
        {
            _readerService = readerService;
        }

        //Basic API.

        public void AddPluginProvider(IPluginProvider pluginProvider)
        {
            lock (this)
            {
                _providers.Add(pluginProvider);
            }
        }

        internal void LoadPlugin(Guid instanceGuid, UserPluginInstanceInfo info, bool overwriteExisting)
        {
            lock (this)
            {
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

                //Load the plugin.
                pluginInstance.Init(info.PluginParameters);
                var reg = new PluginHandlerRegistry()
                {
                    InstanceInfo = info,
                };
                pluginInstance.Load(reg);

                //Add to list.
                _loadedPlugins.Add(instanceGuid, reg);
            }
        }

        internal void UnloadPlugin(Guid instanceGuid)
        {
            lock (this)
            {
                if (_loadedPlugins.Remove(instanceGuid, out var pluginReg))
                {
                    pluginReg.Unload();
                }
            }
        }

        //Reader client integration.

        internal async Task InitUserAsync()
        {
            var userConfig = _readerService.UserConfig;
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
        }

        internal async Task LoadAllPluginsAsync()
        {
            var itemList = new List<Item>();
            await _readerService.UserConfig.GetSystemStorage().GetItemsAsync(_pluginType, default, -1, itemList);
            foreach (var item in itemList)
            {
                if (!item.IsValid) continue;
                var pluginInfo = ItemDataHelper.FromItemData<UserPluginInstanceInfo>(item.Data);
                try
                {
                    LoadPlugin(item.ItemGuid, pluginInfo, overwriteExisting: false);
                }
                catch (Exception e)
                {
                    //TODO properly output the error
                    Console.WriteLine(e);
                }
            }
        }

        public (Guid guid, UserPluginInstanceInfo info)[] GetLoadedPlugins()
        {
            lock (this)
            {
                return _loadedPlugins.Select(pair => (pair.Key, pair.Value.InstanceInfo)).ToArray();
            }
        }

        public async Task<Guid> AddPlugin(string pluginName, string data)
        {
            var guid = Guid.NewGuid();
            var info = new UserPluginInstanceInfo
            {
                PluginName = pluginName,
                PluginParameters = data,
            };
            var item = new Item(_pluginType, guid, data: info);

            //TODO timeout
            using var storageLock = await _readerService.UserConfig.GetSystemStorage().LockStorageAsync(1000);

            //Load before modifying storage (so it will not be loaded ever if we can't load it now).
            LoadPlugin(guid, info, overwriteExisting: false);
            _readerService.UserConfig.GetSystemStorage().AddItemVersion(item);

            return guid;
        }

        public async Task RemovePlugin(Guid guid)
        {
            var storage = _readerService.UserConfig.GetSystemStorage();

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
