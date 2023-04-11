using System.Text;
using Utilizr.Util;

namespace Utilizr.Win.Util;

/// <summary>
/// Embedded resource config loader
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class LoadableEmbedded<T>: Loadable<T> where T : Loadable<T>, new()
{
    public virtual string? EmbeddedResourceName { get; }
    
    public override string? RawLoad(string customLoadPath = "")
    {
        if (!string.IsNullOrEmpty(EmbeddedResourceName))
        {
            var resourceData = Win32.Kernel32.ResourceHelper.LoadResourceFile(EmbeddedResourceName, customLoadPath);
            if (resourceData != null)
            {
                return Encoding.UTF8.GetString(resourceData);
            }
        }

        return null;
    }
}