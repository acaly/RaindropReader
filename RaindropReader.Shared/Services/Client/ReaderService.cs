using RaindropReader.Shared.Components;
using RaindropReader.Shared.Services.Plugins;
using RaindropReader.Shared.Services.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Client
{
    /// <summary>
    /// Scoped service that manages the state of the side bar component and
    /// the main view component.
    /// </summary>
    public sealed class ReaderService : IDisposable
    {
        private readonly IStorageProvider _storageProvider;
        private readonly INavigationHandler _navigationHandler;
        public PluginManager PluginManager { get; }

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

        //Assign the InvokeAsync of a component to allow the reader to dispatch
        //background tasks.
        public Func<Func<Task>, Task> DispatcherMethod { get; set; }
        private readonly Timer _timer;
        private readonly SemaphoreSlim _timerLock = new SemaphoreSlim(1);
        public TimeSpan TimerInterval =
#if DEBUG
            TimeSpan.FromSeconds(5);
#else
            TimeSpan.FromMinutes(5);
#endif

        /// <summary>
        /// DI constructor.
        /// </summary>
        /// <param name="navigationHandler"></param>
        public ReaderService(INavigationHandler navigationHandler, IStorageProvider storageProvider)
        {
            _navigationHandler = navigationHandler;
            _storageProvider = storageProvider;
            _tabs.Add(new ListTab(this));
            CheckSelectedTab();
            PluginManager = new(this);
            _timer = new Timer(state => RunScheduledTasksAsync().Wait(), null, TimerInterval, TimerInterval);
        }

        public void Dispose()
        {
            _timer.Dispose();
            PluginManager.Dispose();
        }

        private async Task RunScheduledTasksAsync()
        {
            if (!_timerLock.Wait(0)) return;
            try
            {
                var utcCheck = DateTime.UtcNow;
                foreach (var (_, p) in PluginManager.GetLoadedPluginsInternal())
                {
                    //Run on component's sync context.
                    await DispatcherMethod(() => p.RunScheduledTasks(utcCheck));
                }
            }
            finally
            {
                _timerLock.Release();
            }
        }

        /// <summary>
        /// Use this method before any other methods when starting a new session, to
        /// load the IUserConfig of the current user.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="createNew"></param>
        public async Task LoadStorageAsync(string userID, bool createNew)
        {
            if (UserConfig is not null)
            {
                throw new InvalidOperationException("LoadStorageAsync can only be called once.");
            }
            if (createNew)
            {
                UserConfig = await _storageProvider.AddUserAsync(userID);
            }
            else
            {
                UserConfig = await _storageProvider.GetUserAsync(userID);
            }
            await PluginManager.InitAsync();
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

        public IEnumerable<SideBarElement> GetSideBarElements()
        {
            bool needSeparator = false;
            foreach (var (_, plugin) in PluginManager.GetLoadedPluginsInternal())
            {
                bool hasElement = false;
                foreach (var e in plugin.GetSideBarElements())
                {
                    if (needSeparator)
                    {
                        needSeparator = false;
                        yield return new TestSideBarElement(this, SideBarElementType.Separator, null, null, null, null);
                    }
                    yield return e;
                    hasElement = true;
                }
                if (hasElement)
                {
                    needSeparator = true;
                }
            }
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
