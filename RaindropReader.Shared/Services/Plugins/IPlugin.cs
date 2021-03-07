using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Plugins
{
    public interface IPlugin
    {
        void Init(string optionData);
        void Load(PluginHandlerRegistry handlerRegistry);
    }
}
