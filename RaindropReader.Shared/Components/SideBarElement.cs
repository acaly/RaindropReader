using RaindropReader.Shared.Services.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Components
{
    public enum SideBarElementType
    {
        Normal,
        Separator,
    }

    public abstract class SideBarElement
    {
        public ReaderService ReaderService { get; protected init; }
        public SideBarElementType Type { get; protected init; }
        public int Indent { get; }

        public abstract string Text { get; }
        public abstract string Icon { get; }
        public abstract string LinkAddress { get; }
        public abstract event Action ElementChanged;

        public SideBarElement(SideBarElement parent = null)
        {
            Indent = parent?.Indent + 1 ?? 0;
        }

        public IEnumerable<SideBarElement> GetChildren()
        {
            throw new NotImplementedException();
        }

        public abstract void OpenLink();
    }

    internal class TestSideBarElement : SideBarElement
    {
        protected string _text, _icon, _linkAddress, _tabGuid;

        public override string Text => _text;
        public override string Icon => _icon;
        public override string LinkAddress => _linkAddress;
        public override event Action ElementChanged;

        public TestSideBarElement(ReaderService readerService, SideBarElementType type,
            string text, string icon, string linkAddr, string tabGuid)
        {
            ReaderService = readerService;
            Type = type;
            _text = text;
            _icon = icon;
            _linkAddress = linkAddr;
            _tabGuid = tabGuid;
        }

        public override void OpenLink()
        {
            ReaderService.OpenTab(_tabGuid);
        }

        protected void RaiseElementChanged()
        {
            ElementChanged?.Invoke();
        }
    }
}
