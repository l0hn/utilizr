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
            string name = fullurl.StartsWith("http://") || fullurl.StartsWith("https://") ? fullurl : $"http://{fullurl}";

            var url = new Uri(name);
            string host = url.Host;

            var nodes = host.Split('.');

            if (nodes.Length > 2)
            {
                var subdomain = string.Format("{0}.{1}", nodes[nodes.Length - 2], nodes[nodes.Length - 1]);
                subdomains.Add(subdomain);
                subdomains.Add($"*.{subdomain}");
            }

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
