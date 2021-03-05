using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RaindropReader.Blazor.Services
{
    internal sealed class DynamicStyleSheetsProvider : IFileProvider
    {
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            throw new InvalidOperationException();
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return NonExistingFileInfo.Instance;
        }

        public IChangeToken Watch(string filter)
        {
            throw new InvalidOperationException();
        }

        private class NonExistingFileInfo : IFileInfo
        {
            public static IFileInfo Instance => new NonExistingFileInfo();

            public bool Exists => false;

            public bool IsDirectory => throw new InvalidOperationException();
            public DateTimeOffset LastModified => throw new InvalidOperationException();
            public long Length => throw new InvalidOperationException();
            public string Name => throw new InvalidOperationException();
            public string PhysicalPath => throw new InvalidOperationException();
            public Stream CreateReadStream() => throw new InvalidOperationException();
        }
    }
}
