using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Storage
{
    public static class ItemDataHelper
    {
        private static JsonSerializerOptions _options = new JsonSerializerOptions();

        public static byte[] ToItemData(object obj)
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj, options: _options);
        }

        public static T FromItemData<T>(ReadOnlySpan<byte> data)
        {
            return JsonSerializer.Deserialize<T>(data, options: _options);
        }
    }
}
