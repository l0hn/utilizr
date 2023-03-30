using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace Utilizr.WPF.Util
{
    internal static class ImageCache
    {
        private static readonly object LOCK = new();
        private static readonly Dictionary<string, BitmapFrame> _cache;

        static ImageCache()
        {
            _cache = new Dictionary<string, BitmapFrame>();
        }

        internal static BitmapFrame? Get(string resource)
        {
            BitmapFrame? result;
            if (_cache.TryGetValue(resource, out result))
                return result;

            var data = ResourceLoadable.Instance?.Get(resource);
            if (data != null)
            {
                using (var memStream = new MemoryStream(data))
                {
                    result = BitmapFrame.Create(memStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
                lock (LOCK)
                {
                    _cache[resource] = result;
                }
            }

            return result;
        }
    }
}
