using RaindropReader.Shared.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Plugins
{
    internal sealed class SystemPlugin : IPlugin
    {
        public void Init(string optionData)
        {
        }

        public async Task LoadAsync(PluginHandlerRegistry handlerRegistry)
        {
            handlerRegistry.RegisterSideBarElement(new TestSideBarElement(handlerRegistry.ReaderService,
                SideBarElementType.Normal, "favorite", "heart", null, "0"));
            handlerRegistry.RegisterSideBarElement(new TestSideBarElement(handlerRegistry.ReaderService,
                SideBarElementType.Separator, null, null, null, null));
            handlerRegistry.RegisterSideBarElement(new TestSideBarElement(handlerRegistry.ReaderService,
                SideBarElementType.Normal, "test 1!", "rss", null, "1"));
            handlerRegistry.RegisterSideBarElement(new TestSideBarElement(handlerRegistry.ReaderService,
                SideBarElementType.Normal, "test 2!!", "rss", null, "2"));
            handlerRegistry.RegisterSideBarElement(new TestSideBarElement(handlerRegistry.ReaderService,
                SideBarElementType.Normal, "test 3!!!", "rss", null, "3"));
        }
    }
}
