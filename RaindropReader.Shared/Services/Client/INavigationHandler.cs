using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Client
{
    /// <summary>
    /// Scoped service to be implemented by the app to help the component
    /// handle routing and browser uri.
    /// </summary>
    public interface INavigationHandler
    {
        string GetAppRoute();
        string GetItemRoute(string id);
        Task ClearBrowserUriAsync();
    }
}
