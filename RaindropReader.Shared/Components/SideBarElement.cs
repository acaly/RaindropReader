using RaindropReader.Shared.Services;
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
        Group,
        Separator,
    }

    public sealed class SideBarElement
    {
        public ReaderService ReaderService { get; init; }
        public SideBarElementType Type { get; init; }
        public int Indent { get; init; }
        public string Text { get; init; }
        public string Icon { get; init; }

        public string NewTabGuid { get; init; }
        public string LinkAddress { get; init; }

        public IEnumerable<SideBarElement> GetChildren()
        {
            throw new NotImplementedException();
        }

        public void OpenLink()
        {
            ReaderService.OpenTab(NewTabGuid);
        }
    }
}
