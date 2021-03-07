using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Storage
{
    /// <summary>
    /// Represent a version of a feed item. This class is immutable.
    /// </summary>
    public sealed class Item
    {
        public Item(IUserItemType type, Guid itemGuid, byte[] byteData,
            Guid? versionGuid = null, DateTime? timestamp = null)
        {
            ItemGuid = itemGuid;
            VersionGuid = versionGuid ?? Guid.NewGuid();
            Timestamp = timestamp ?? DateTime.UtcNow;
            ItemType = type;
            _data = byteData;
        }

        public Item(IUserItemType type, Guid itemGuid, object data,
            Guid? versionGuid = null, DateTime? timestamp = null)
            : this(type, itemGuid, data == null ? null : ItemDataHelper.ToItemData(data), versionGuid, timestamp)
        {
        }

        /// <summary>
        /// The GUID of this item. 
        /// </summary>
        public Guid ItemGuid { get; }

        /// <summary>
        /// The GUID of this version.
        /// </summary>
        public Guid VersionGuid { get; }

        /// <summary>
        /// The UTC time of this version's creation.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// The type of this item.
        /// </summary>
        public IUserItemType ItemType { get; }

        private readonly byte[] _data;

        /// <summary>
        /// Returns whether this version is the deleted version. Deleted
        /// version of an item clears the content of the item.
        /// </summary>
        public bool IsValid => _data is not null;

        /// <summary>
        /// Get the content of this item. The content is usually a UTF8 
        /// json string. User should not try to modify this content.
        /// </summary>
        public ReadOnlySpan<byte> Data => _data;
    }
}
