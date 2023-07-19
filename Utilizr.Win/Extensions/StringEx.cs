using System.Text;
using Utilizr.Win32.ShlwApi;
using Utilizr.Win32.ShlwApi.Flags;

namespace Utilizr.Win.Extensions
{
    public static class StringEx
    {
        /// <summary>
        /// Takes a URL string and converts it into canonical form
        /// </summary>
        /// <param name="pszUrl">URL string</param>
        /// <param name="dwFlags">Shlwapi_URL Enumeration. Flags that specify how the URL is converted to canonical form.</param>
        /// <returns>The converted URL</returns>
        public static string CannonializeUrl(this string pszUrl, Shlwapi_URL dwFlags)
        {
            var buff = new StringBuilder(260);
            int s = buff.Capacity;
            int c = ShlwApi.UrlCanonicalize(pszUrl, buff, ref s, dwFlags);

            if (c == 0)
                return buff.ToString();

            buff.Capacity = s;
            c = ShlwApi.UrlCanonicalize(pszUrl, buff, ref s, dwFlags);
            return buff.ToString();
        }
    }
}
