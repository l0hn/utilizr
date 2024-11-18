using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilizr.Util
{
    public static class DomainHelper
    {
        public static List<string> GetSubDomain(string? fullurl)
        {
            List<string> subdomains = new List<string>();
            string host = "";
            string absolutepath = "";
            GetDomain(fullurl, out host, out absolutepath);

            subdomains.Add(host);
            subdomains.Add($"*.{host}");

            return subdomains;
        }

        public static bool GetDomain(string? url, out string host, out string absolutepath)
        {
            host = string.Empty;
            absolutepath = string.Empty;

            string name = url.StartsWith("http://") || url.StartsWith("https://") ? url : $"http://{url}";
            var uri = new Uri(name);
            var path = uri.AbsolutePath.Replace('/', ' ').Trim();
            host = uri.Host;
            absolutepath = uri.AbsolutePath;

            if (string.IsNullOrWhiteSpace(path))
                return false;
            else
                return true;
        }
    }
}
