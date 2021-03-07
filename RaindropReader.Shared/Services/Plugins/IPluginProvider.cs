using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Plugins
{
    /// <summary>
    /// Interface to be implemented by app to provide IPlugin instances from name.
    /// </summary>
    public interface IPluginProvider
    {
        IPlugin CreatePlugin(string name);
    }
}
