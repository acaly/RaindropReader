using RaindropReader.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Components
{
    public interface ITab
    {
        string Title { get; }
        bool IsSelected { get; set; }
    }

    public class ItemTab : ITab
    {
        private readonly ReaderService _service;

        public ItemTab(ReaderService service)
        {
            _service = service;
        }

        public string Title => "Item";

        public bool IsSelected
        {
            get => _service.SelectedTab == this;
            set => _service.SelectedTab = this;
        }
    }

    public class ListTab : ITab
    {
        private readonly ReaderService _service;

        public ListTab(ReaderService service)
        {
            _service = service;
        }

        public string Title => "List";

        public bool IsSelected
        {
            get => _service.SelectedTab == this;
            set => _service.SelectedTab = this;
        }
    }
}
