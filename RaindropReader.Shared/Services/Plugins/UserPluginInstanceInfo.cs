using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Plugins
{
    public sealed class UserPluginInstanceInfo
    {
        public string PluginName { get; init; }
        public string PluginParameters { get; init; } //Serialized string.
    }
}
