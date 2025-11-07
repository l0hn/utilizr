using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilizr.Crypto;
using Utilizr.Info;

namespace Utilizr.Network
{
    public enum FavIcoSize
    {
        Small,
        Medium,
        Large
    }

    public static class FavIcon
    {
        private const string DOMAIN_REGEX = "\\b((?=[a-z0-9-]{1,63}\\.)(xn--)?[a-z0-9]+(-[a-z0-9]+)*\\.)+[a-z]{2,63}\\b";
        static Regex _domainRegex = new Regex(DOMAIN_REGEX, RegexOptions.IgnoreCase);
        private const string USER_AGENT = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        static object DICT_LOCK = new();
        static Dictionary<string, object> _locks = new();
        public static readonly string ICO_CACHE_DIR = Path.Combine(AppInfo.CacheDir, "favicons2");

        public static Task GetFavIconAsync(string? url, params FavIcoSize[] sizeOrder)
        {
            return Task.Run(() => GetFavIcon(url, sizeOrder));
        }

        public static FavIco? GetFavIcon(string? url, params FavIcoSize[] sizeOrder)
        {
            return GetFavIcons(url, sizeOrder)?.FirstOrDefault();
        }

        public static FavIco[]? GetFavIcons(string? url, params FavIcoSize[] sizeOrder)
        {
            var cleanUrl = SanitizeUrl(url);

            if (string.IsNullOrEmpty(cleanUrl))
                return null;

            object? lockObj = null;
            lock (DICT_LOCK)
            {
                if (!_locks.TryGetValue(cleanUrl, out lockObj))
                {
                    _locks[cleanUrl] = lockObj = new object();
                }
            }

            try 
            {
                lock (lockObj) 
                {
                    var orderDict = new Dictionary<FavIcoSize, int>();
                    var cached = new List<FavIco>();
                    for (int i = 0; i < sizeOrder.Length; i++)
                    {
                        var icoLink = FavIco.LoadFromCache(cleanUrl, sizeOrder[i]);
                        if (icoLink != null)
                            cached.Add(icoLink);

                        orderDict[sizeOrder[i]] = i;
                    }

                    if (cached.Any())
                        return cached.ToArray();

                    var links = GetIconLinks(cleanUrl)
                        .Where(p => p != null)
                        .OrderBy(i => orderDict.ContainsKey(i.FavIcoSize) ? orderDict[i.FavIcoSize] : 99);

                    return DownloadIcos(links).ToArray();
                }
            }
            finally
            {
                lock (DICT_LOCK)
                {
                    _locks.Remove(cleanUrl);
                }
            }
        }

        static IEnumerable<FavIco> DownloadIcos(IEnumerable<FavIco?> icoLinks)
        {
            foreach (var icoLink in icoLinks)
            {
                if (icoLink == null)
                    continue;

                if (!Directory.Exists(ICO_CACHE_DIR))
                    Directory.CreateDirectory(ICO_CACHE_DIR);
                else if (File.Exists(icoLink.FilePath))
                    continue;

                var path = Path.Combine(ICO_CACHE_DIR, $"{Hash.MD5(icoLink.Domain)}_{icoLink.FavIcoSize}_0.dat");
                icoLink.FilePath = path;

                try
                {
                    if (string.IsNullOrEmpty(icoLink.Href))
                        continue;

                    NetUtil.DownloadFile(icoLink.Href, path, requestTimeout: 5000, userAgent: USER_AGENT);
                }
                catch
                {
                    continue;
                }
                yield return icoLink;
            }
        }

        static IEnumerable<FavIco?> GetIconLinks(string? url)
        {
            if (string.IsNullOrEmpty(url))
                yield break;

            // https://www.google.com/s2/favicons?domain=facebook.com&sz=32
            // https://icons.duckduckgo.com/ip3/facebook.com.ico

            var googleIcons = new List<(FavIcoSize Size, int SzParam)>
            {
                (FavIcoSize.Large, 64),
                (FavIcoSize.Medium, 32),
                (FavIcoSize.Small, 16),
            };

            foreach (var googleIcon in googleIcons)
            {
                yield return new FavIco(url, googleIcon.Size)
                {
                    Href = $"https://www.google.com/s2/favicons?domain={url}&sz={googleIcon.SzParam}",
                };
            }

            // ddg fallback, no size option
            yield return new FavIco(url, FavIcoSize.Large)
            {
                Href = $"https://icons.duckduckgo.com/ip3/{url}.ico",
            };
        }

        public static string? SanitizeUrl(string? url)
        {
            if (string.IsNullOrEmpty(url) || url.Length < 3)
                return null;

            if (!_domainRegex.IsMatch(url))
                return null;

            url = url.ToLower();
            var scheme = url.StartsWith("https://") ? "https://" : "http://";
            url = url.Replace("http://", "").Replace("https://", "");
            //if (!url.StartsWith("www."))
            //    url = "www." + url;

            url = scheme + url;

            var uri = new Uri(url);
            var subDomainRemover = new SubDomainStripper(uri);
            return $"{uri.Scheme}://{subDomainRemover.HostWithoutSubdomain}";
        }
    }

    public class FavIco
    {
        public string Domain { get; set; }
        public FavIcoSize FavIcoSize { get; set; }

        internal string? Href { get; set; }
        public string? FilePath { get; internal set; }

        public FavIco(string domain, FavIcoSize size)
        {
            Domain = domain;
            FavIcoSize = size;
        }

        internal static FavIco? LoadFromCache(string domain, FavIcoSize size)
        {
            var path = Path.Combine(FavIcon.ICO_CACHE_DIR, $"{Hash.MD5(domain)}_{size}_");
            var i = new FavIco(domain, size);
          
            if (File.Exists(path + "0.dat"))
                i.FilePath = path + "0.dat";

            //if (string.IsNullOrEmpty(i.FilePath) && File.Exists(path + "1.dat"))
            //    i.FilePath = path + "1.dat";
            
            if (!string.IsNullOrEmpty(i.FilePath))
                return i;

            return null;
        }
    }
}