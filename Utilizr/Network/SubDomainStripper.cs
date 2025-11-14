using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilizr.Network
{
    /// <summary>
    /// Attempt to remove any subdomains from a URI, without tripping up on the public suffix.
    /// Not intended for anything critical.
    /// Offered on a 'best effort' scenario, checking some common edge cases, but will fail for many.
    /// </summary>
    public class SubDomainStripper
    {
        public string HostWithoutSubdomain { get; }
        public string OriginalHost { get; }


        public SubDomainStripper(Uri uri)
        {
            OriginalHost = uri.Host;

            // It's not reliable to just check for two dots due to the public suffix.
            // Examples include co.uk and gov.au, see: https://publicsuffix.org/list/
            // Not ideal, but assume any three letters or under if part of this suffix

            var parts = OriginalHost.Split('.');

            if (parts.Length <= 2)
            {
                HostWithoutSubdomain = uri.Host;
                return;
            }

            // assume last is always part of public fuffix
            var tld = parts.Last();

            // assume second is part of public suffix if 3 chars or less
            for (int i = parts.Length - 2; i >= 0; i--)
            {
                var part = parts[i];
                if (part.Length > 3)
                {
                    HostWithoutSubdomain = $"{part}.{tld}";
                    return;
                }

                tld = $"{part}.{tld}";
            }

            // failed to remove, no changes made
            HostWithoutSubdomain = OriginalHost;
        }
    }
}
