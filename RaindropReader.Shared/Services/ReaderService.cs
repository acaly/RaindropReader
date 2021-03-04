using RaindropReader.Shared.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services
{
    /// <summary>
    /// Scoped service that manages the state of the side bar component and
    /// the main view component.
    /// </summary>
    public sealed class ReaderService
    {
        private readonly INavigationHandler _navigationHandler;

        public ReaderService(INavigationHandler navigationHandler)
        {
            _navigationHandler = navigationHandler;
        }

        /// <summary>
        /// Use this method to initialize the app when started with a non-default url.
        /// </summary>
        /// <param name="id"></param>
        public void HandleItemRoute(string id)
        {
            //TODO
        }

        private List<SideBarElement> _elements = GetInitSideBarElements().ToList();

        private static IEnumerable<SideBarElement> GetInitSideBarElements()
        {
            yield return new SideBarElement
            {
                Type = SideBarElementType.Normal,
                Indent = 0,
                Text = "favorite",
                Icon = "heart",
            };
            yield return new SideBarElement
            {
                Type = SideBarElementType.Normal,
                Indent = 0,
                Text = "test 1",
                Icon = "rss",
            };
            yield return new SideBarElement
            {
                Type = SideBarElementType.Normal,
                Indent = 0,
                Text = "test 2",
                Icon = "rss",
            };
        }

        public IEnumerable<SideBarElement> GetSideBarElements()
        {
            return _elements;
        }
    }
}
