using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilizr.Util
{
    public static class DomainHelper
    {
        public static string GetSubDomain(string fullurl)
        {
            string name = fullurl.StartsWith("http://") || fullurl.StartsWith("https://") ? fullurl : $"http://{fullurl}";
            var url = new Uri(name);

            if (url.HostNameType == UriHostNameType.Dns)
            {
                string host = url.Host;

                var nodes = host.Split('.');

                if (nodes.Length > 2)
                {
                    return string.Format("{0}.{1}", nodes[nodes.Length - 2], nodes[nodes.Length - 1]);
                }
                return null;
            }

            return null;
        }

        public static string GetDomain(string url)
        {
            string name = url.StartsWith("http://") || url.StartsWith("https://") ? url : $"http://{url}";
            var uri = new Uri(name);
            return uri.Host;
        }

        public static bool GetExcemption(string exemption, out string sdkExemption)
        {
            sdkExemption = string.Empty;

            bool IsAsterisk = false;
            string tempDomain = exemption;
            if (tempDomain.StartsWith("*."))
            {
                tempDomain = tempDomain.Replace("*.", "");
                IsAsterisk = true;
            }
            string name = tempDomain.StartsWith("http://") || tempDomain.StartsWith("https://") ? tempDomain : $"http://{tempDomain}";

            var uri = new Uri(name);
            var host = uri.Host;
            var path = uri.AbsolutePath.Replace('/', ' ').Trim();

            if (string.IsNullOrWhiteSpace(path))
            {
                if (IsAsterisk)
                    sdkExemption = $"*.{host}";
                else
                    sdkExemption = host;

                return true;
            }
            else
                sdkExemption = $"{host}{uri.AbsolutePath}";

            return false;
        }
    }
}
