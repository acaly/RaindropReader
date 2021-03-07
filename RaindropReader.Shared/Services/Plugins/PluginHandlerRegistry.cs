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
        public UserPluginInstanceInfo InstanceInfo { get; init; }

        internal PluginHandlerRegistry()
        {

        }

        //TODO
        //register type (GUID)

        internal void Unload()
        {
            throw new NotImplementedException();
        }
    }
}
