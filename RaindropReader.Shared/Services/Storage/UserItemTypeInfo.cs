using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Storage
{
    /// <summary>
    /// The immutable class representing data of a item type. This is also the
    /// serialized type of the item that defines a type.
    /// </summary>
    public sealed class UserItemTypeInfo
    {
        /// <summary>
        /// Readable name of the type. Usually set by user. Modification
        /// requires modification lock on type storage.
        /// System types uses [] to provide localized name.
        /// </summary>
        public string DisplayName { get; init; }
    }
}
