using RaindropReader.Shared.Services.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Plugins
{
    [Plugin]
    internal sealed class TestPlugin : IPlugin
    {
        public void Init(string optionData)
        {
        }

        public void Load(PluginHandlerRegistry handlerRegistry)
        {
        }
    }
}
