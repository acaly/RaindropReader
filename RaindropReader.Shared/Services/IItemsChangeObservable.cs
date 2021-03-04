using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services
{
    /// <summary>
    /// Provide the event to inform subscribers when the item list
    /// of a type has changed.
    /// </summary>
    public interface IItemsChangeObservable : IDisposable
    {
        event EventHandler LocalItemsChanged;
        event EventHandler RemoteItemsChanged;
    }
}
