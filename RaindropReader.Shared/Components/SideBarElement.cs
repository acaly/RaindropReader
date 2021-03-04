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
        public SideBarElementType Type { get; init; }
        public int Indent { get; init; }
        public string Text { get; init; }
        public string Icon { get; init; }

        public int ClickCount { get; private set; }

        public IEnumerable<SideBarElement> GetChildren()
        {
            throw new NotImplementedException();
        }

        public void Click()
        {
            //TODO
            ClickCount += 1;
        }
    }
}
