using RaindropReader.Shared.Components;
using RaindropReader.Shared.Services.Plugins;
using RaindropReader.Shared.Services.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Client
{
    /// <summary>
    /// Scoped service that manages the state of the side bar component and
    /// the main view component.
    /// </summary>
    public sealed class ReaderService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly INavigationHandler _navigationHandler;
        public PluginManager PluginManager { get; }

        private readonly List<SideBarElement> _sideBarElements = new();
        private readonly List<ITab> _tabs = new();

        private ITab _selectedTab;
        internal ITab SelectedTab 
        {
            get => _selectedTab;
            set
            {
                if (_tabs.Contains(value))
                {
                    _selectedTab = value;
                    CheckSelectedTab();
                    UpdateTabHeader?.Invoke();
                    UpdateTabContainer?.Invoke();
                }
            }
        }

        public event Action UpdateSideBar;
        public event Action UpdateTabHeader;
        public event Action UpdateTabContainer;

        public IUserConfig UserConfig { get; private set; }

        /// <summary>
        /// DI constructor.
        /// </summary>
        /// <param name="navigationHandler"></param>
        public ReaderService(INavigationHandler navigationHandler, IStorageProvider storageProvider)
        {
            _navigationHandler = navigationHandler;
            _storageProvider = storageProvider;
            InitSideBarElements();
            _tabs.Add(new ListTab(this));
            CheckSelectedTab();
            PluginManager = new(this);
        }

        /// <summary>
        /// Use this method before any other methods when starting a new session, to
        /// load the IUserConfig of the current user.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="createNew"></param>
        public async Task LoadStorageAsync(string userID, bool createNew)
        {
            if (createNew)
            {
                UserConfig = await _storageProvider.AddUserAsync(userID);
                await PluginManager.InitUserAsync();
            }
            else
            {
                UserConfig = await _storageProvider.GetUserAsync(userID);
                await PluginManager.LoadAllPluginsAsync();
            }
        }

        /// <summary>
        /// Use this method to initialize the app when started with a non-default url.
        /// </summary>
        /// <param name="id"></param>
        public void HandleItemRoute(string id)
        {
            _tabs.Clear();
            OpenTab(id);
        }

        /// <summary>
        /// Initialize the JsInterop lib. Must be called in the first AfterRender event.
        /// </summary>
        /// <returns></returns>
        public async Task InitJSAsync()
        {
            await _navigationHandler.ClearBrowserUriAsync();
        }

        private void InitSideBarElements()
        {
            _sideBarElements.Add(new SideBarElement
            {
                ReaderService = this,
                NewTabGuid = "1",
                Type = SideBarElementType.Normal,
                Indent = 0,
                Text = "favorite",
                Icon = "heart",
            });
            _sideBarElements.Add(new SideBarElement
            {
                ReaderService = this,
                NewTabGuid = "2",
                Type = SideBarElementType.Normal,
                Indent = 0,
                Text = "test 1",
                Icon = "rss",
            });
            _sideBarElements.Add(new SideBarElement
            {
                ReaderService = this,
                NewTabGuid = "3",
                Type = SideBarElementType.Normal,
                Indent = 0,
                Text = "test 2",
                Icon = "rss",
            });
        }

        public IEnumerable<SideBarElement> GetSideBarElements()
        {
            return _sideBarElements;
        }

        private void CheckSelectedTab()
        {
            if (SelectedTab is not null && !_tabs.Contains(SelectedTab))
            {
                SelectedTab = null;
            }
            if (SelectedTab is null && _tabs.Count != 0)
            {
                SelectedTab = _tabs[0];
            }
        }

        public IEnumerable<ITab> GetTabs()
        {
            return _tabs;
        }

        internal void OpenTab(string guid, bool show = true)
        {
            var newTab = new ItemTab(this);
            _tabs.Add(newTab);
            CheckSelectedTab();
            UpdateTabHeader?.Invoke();
            if (show)
            {
                SelectedTab = newTab;
                UpdateTabContainer?.Invoke();
            }
        }
    }
}
