using RaindropReader.Shared.Components;
using RaindropReader.Shared.Services.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Plugins
{
    internal sealed class SystemPlugin : IPlugin
    {
        private class DynamicTestSideBarElement : TestSideBarElement
        {
            private int count = 0;
            public DynamicTestSideBarElement(ReaderService readerService)
                : base(readerService, SideBarElementType.Normal, "test dynamic (0)", "rss", null, "1")
            {
            }

            public void Increment()
            {
                count += 1;
                _text = $"test dynamic ({count})";
                RaiseElementChanged();
            }
        }

        private DynamicTestSideBarElement _dynamicElement;

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
                SideBarElementType.Normal, "test 1", "rss", null, "1"));
            handlerRegistry.RegisterSideBarElement(new TestSideBarElement(handlerRegistry.ReaderService,
                SideBarElementType.Normal, "test 2", "rss", null, "2"));
            handlerRegistry.RegisterSideBarElement(new TestSideBarElement(handlerRegistry.ReaderService,
                SideBarElementType.Normal, "test 3", "rss", null, "3"));

            handlerRegistry.RegisterSideBarElement(new TestSideBarElement(handlerRegistry.ReaderService,
                SideBarElementType.Separator, null, null, null, null));

            _dynamicElement = new DynamicTestSideBarElement(handlerRegistry.ReaderService);
            handlerRegistry.RegisterSideBarElement(_dynamicElement);

            handlerRegistry.RegisterScheduledTask(() =>
            {
                _dynamicElement.Increment();
                return Task.CompletedTask;
            }, TimeSpan.FromSeconds(5));
        }
    }
}
