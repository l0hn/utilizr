using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilizr.Util
{
    public static class DomainHelper
    {
        public static string GetFullUrl(string? url)
        {
            return url.StartsWith("http://") || url.StartsWith("https://") ? url : $"http://{url}";
        }
    }
}
