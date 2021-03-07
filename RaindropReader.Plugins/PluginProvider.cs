using RaindropReader.Shared.Services.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Plugins
{
    public sealed class PluginProvider : IPluginProvider
    {
        private readonly Dictionary<string, Type> _pluginTypes = new();
        public static readonly PluginProvider Instance = new();

        private PluginProvider()
        {
            foreach (var t in Assembly.GetExecutingAssembly().GetTypes())
            {
                var attr = t.GetCustomAttribute<PluginAttribute>();
                if (attr is not null)
                {
                    _pluginTypes.Add(t.FullName, t);
                }
            }
        }

        public IPlugin CreatePlugin(string name)
        {
            if (_pluginTypes.TryGetValue(name, out var type))
            {
                return (IPlugin)Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
