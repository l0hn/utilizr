using System;

namespace Utilizr.Win32.ShlwApi.Flags
{
    [Flags]
    public enum Shlwapi_URL : uint
    {
        /// <summary>
        /// Treat "/./" and "/../" in a URL string as literal characters, not as shorthand for navigation. 
        /// </summary>
        URL_DONT_SIMPLIFY = 0x08000000,
        /// <summary>
        /// Convert any occurrence of "%" to its escape sequence.
        /// </summary>
        URL_ESCAPE_PERCENT = 0x00001000,
        /// <summary>
        /// Replace only spaces with escape sequences. This flag takes precedence over URL_ESCAPE_UNSAFE, but does not apply to opaque URLs.
        /// </summary>
        URL_ESCAPE_SPACES_ONLY = 0x04000000,
        /// <summary>
        /// Replace unsafe characters with their escape sequences. Unsafe characters are those characters that may be altered during transport across the Internet, and include the (<, >, ", #, {, }, |, \, ^, ~, [, ], and ') characters. This flag applies to all URLs, including opaque URLs.
        /// </summary>
        URL_ESCAPE_UNSAFE = 0x20000000,
        /// <summary>
        /// Combine URLs with client-defined pluggable protocols, according to the World Wide Web Consortium (W3C) specification. This flag does not apply to standard protocols such as ftp, http, gopher, and so on. If this flag is set, UrlCombine does not simplify URLs, so there is no need to also set URL_DONT_SIMPLIFY.
        /// </summary>
        URL_PLUGGABLE_PROTOCOL = 0x40000000,
        /// <summary>
        /// Un-escape any escape sequences that the URLs contain, with two exceptions. The escape sequences for "?" and "#" are not un-escaped. If one of the URL_ESCAPE_XXX flags is also set, the two URLs are first un-escaped, then combined, then escaped.
        /// </summary>
        URL_UNESCAPE = 0x10000000
    }
}
