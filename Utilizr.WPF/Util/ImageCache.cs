using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace Utilizr.WPF.Util
{
    internal static class ImageCache
    {
        private static readonly object LOCK = new();
        private static readonly Dictionary<string, BitmapFrame> _cache;

        private static readonly object LOCK_SVG = new();
        private static readonly Dictionary<string, string> _cacheSvg;

        static ImageCache()
        {
            _cache = new Dictionary<string, BitmapFrame>();
            _cacheSvg = new Dictionary<string, string>();
        }

        internal static BitmapFrame? Get(string resource, string? theme = null)
        {
            var themedResource = string.IsNullOrEmpty(theme)
                ? resource
                : $"{theme}/{resource}";

            BitmapFrame? result;
            if (_cache.TryGetValue(themedResource, out result))
                return result;

            var data = ResourceLoadable.Instance?.Get(themedResource);
            if (data != null)
            {
                using (var memStream = new MemoryStream(data))
                {
                    result = BitmapFrame.Create(memStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
                lock (LOCK)
                {
                    _cache[themedResource] = result;
                }
            }

            return result;
        }

        internal static string? GetSvgSource(string resource)
        {
            if (_cacheSvg.TryGetValue(resource, out string? rawSvg))
                return rawSvg;

            var data = ResourceLoadable.Instance?.Get(resource);
            if (data != null)
            {
                rawSvg = Encoding.UTF8.GetString(data);
                lock (LOCK_SVG)
                {
                    _cacheSvg[resource] = rawSvg;
                }
            }

            return rawSvg;
        }
    }
}
