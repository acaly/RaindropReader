using Microsoft.JSInterop;
using RaindropReader.Shared.Services.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaindropReader.Blazor.Services
{
    internal sealed class NavigationHandler : INavigationHandler
    {
        private readonly IJSRuntime _jsRuntime;

        public NavigationHandler(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public Task ClearBrowserUriAsync()
        {
            return _jsRuntime.InvokeVoidAsync("setBrowserUri", Startup.AppRoute).AsTask();
        }

        public string GetAppRoute()
        {
            return Startup.AppRoute;
        }

        public string GetItemRoute(string id)
        {
            return Startup.GetAppItemRoute(id);
        }
    }
}
