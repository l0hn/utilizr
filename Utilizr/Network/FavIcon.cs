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
    public static class Favicon
    {
        private const string DOMAIN_REGEX = "\\b((?=[a-z0-9-]{1,63}\\.)(xn--)?[a-z0-9]+(-[a-z0-9]+)*\\.)+[a-z]{2,63}\\b";
        static Regex _domainRegex = new Regex(DOMAIN_REGEX, RegexOptions.IgnoreCase);
        private const string USER_AGENT = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        static object DICT_LOCK = new object();
        static Dictionary<string, object> _locks = new Dictionary<string, object>();
        public static readonly string ICO_CACHE_DIR = Path.Combine(AppInfo.CacheDir, "favicons");

        private static string[] _icoTypes =
        {
            "shortcut icon",
            "icon",
            "apple-touch-icon",
            "apple-touch-icon-precomposed"
        };

        private static Dictionary<string, FavIcoSize> _sizes = new Dictionary<string, FavIcoSize>()
        {
            {"shortcut icon", FavIcoSize.Small},
            {"icon", FavIcoSize.Small},
            {"apple-touch-icon", FavIcoSize.Large},
            {"apple-touch-icon-precomposed", FavIcoSize.Large},
        };

        public static Task GetFaviconAsync(string? url, params FavIcoSize[] sizeOrder)
        {
            return Task.Run(() => GetFavicon(url, sizeOrder));
        }

        public static FavIco? GetFavicon(string? url, params FavIcoSize[] sizeOrder)
        {
            return GetFavicons(url, sizeOrder)?.FirstOrDefault();
        }

        public static FavIco[]? GetFavicons(string? url, params FavIcoSize[] sizeOrder)
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
                        .OrderBy(i => orderDict.ContainsKey(i.FavIcoSize) ? orderDict[i.FavIcoSize] : 99)
                        .ThenBy(i => i.Rel != "shortcut icon");

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

        static IEnumerable<FavIco> DownloadIcos(IEnumerable<FavIco> icoLinks)
        {
            foreach (var icoLink in icoLinks)
            {
                if (!Directory.Exists(ICO_CACHE_DIR))
                    Directory.CreateDirectory(ICO_CACHE_DIR);
                else if (File.Exists(icoLink.FilePath))
                    continue;

                var path = Path.Combine(ICO_CACHE_DIR, $"{Hash.MD5(icoLink.Domain)}_{icoLink.FavIcoSize}_");
                if (icoLink.Rel == "icon")
                    path += "1.dat";
                else
                    path += "0.dat";

                icoLink.FilePath = path;

                try
                {
                    NetUtil.DownloadFile(icoLink.Href, path, requestTimeout: 5000, userAgent: USER_AGENT);
                }
                catch
                {
                    continue;
                }
                yield return icoLink;
            }
        }

        static IEnumerable<FavIco>? GetIconLinks(string url)
        {
            return null;
            //Uri baseUri = new Uri(url);
            //HtmlWeb htmlWeb = new HtmlWeb();
            //List<string> found = new List<string>();
            //HtmlDocument doc = null;
            //HtmlNodeCollection nodes = null;

            //try
            //{
            //    doc = htmlWeb.Load(url);
            //}
            //catch
            //{
            //    htmlWeb.UserAgent = USER_AGENT;
            //    try
            //    {
            //        doc = htmlWeb.Load(url);
            //    }
            //    catch
            //    {
            //        yield break;
            //    }
            //}

            //nodes = doc.DocumentNode.SelectNodes("/html/head/link[@rel][@href]");
            //baseUri = new Uri(htmlWeb.ResponseUri.Scheme + "://" + htmlWeb.ResponseUri.Host);

            //foreach (var node in nodes ?? new HtmlNodeCollection(null))
            //{
            //    var rel = node.Attributes["rel"]?.Value;
            //    if (rel.IsNullOrEmpty())
            //        continue;

            //    if (found.Contains(rel))
            //        continue;
                
            //    rel = rel.ToLower();

            //    var link = node.Attributes["href"]?.Value;
            //    if (link.IsNullOrEmpty())
            //        continue;
                
            //    if (_icoTypes.Contains(rel))
            //    {
            //        found.Add(rel);
            //        yield return new FavIco()
            //        {
            //            Domain = url,
            //            Rel = rel,
            //            Href = new Uri(baseUri, link).AbsoluteUri,
            //            FavIcoSize = _sizes[rel]
            //        };
            //    }
            //}

            //if (!found.Contains("shortcut icon"))
            //{
            //    //try the favicon
            //    yield return new FavIco()
            //    {
            //        Domain = url,
            //        Rel = "shortcut icon",
            //        Href = new Uri(baseUri, "/favicon.ico").AbsoluteUri,
            //        FavIcoSize = FavIcoSize.Small
            //    };
            //}
        }

        public static string? SanitizeUrl(string? url)
        {
            if (string.IsNullOrEmpty(url) || url.Length < 3)
                return null;

            if (!_domainRegex.IsMatch(url))
                return null;

            var scheme = "http://";
            url = url.ToLower();
            scheme = url.StartsWith("https://") ? "https://" : "http://";
            url = url.Replace("http://", "").Replace("https://", "");
            //if (!url.StartsWith("www."))
            //    url = "www." + url;
            
            url = scheme + url;

            var uri = new Uri(url);
            return $"{uri.Scheme}://{uri.Host}";
        }
    }

    public enum FavIcoSize
    {
        Small,
        Medium,
        Large
    }

    public class FavIco
    {
        public string Domain { get; set; }
        internal string Rel { get; set; }
        internal string Href { get; set; }

        public FavIcoSize FavIcoSize { get; set; }

        public string FilePath { get; internal set; }

        internal static FavIco? LoadFromCache(string domain, FavIcoSize size)
        {
            var path = Path.Combine(Favicon.ICO_CACHE_DIR, $"{Hash.MD5(domain)}_{size}_");
            var i = new FavIco() { Domain = domain, FavIcoSize = size };
          
            if (File.Exists(path + "0.dat"))
                i.FilePath = path + "0.dat";

            if (string.IsNullOrEmpty(i.FilePath) && File.Exists(path + "1.dat"))
                i.FilePath = path + "1.dat";
            
            if (!string.IsNullOrEmpty(i.FilePath))
                return i;
            
            return null;
        }
    }
}