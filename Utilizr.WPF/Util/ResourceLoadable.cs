using System.Collections.Generic;
using System.IO;
using Utilizr.Info;
using Utilizr.Win.Util;

namespace Utilizr.WPF.Util
{
    public class ResourceLoadable : LoadableEmbedded<ResourceLoadable>
    {
        public override string EmbeddedResourceName => "RESOURCES_URES";

        private Dictionary<string, byte[]> Resources { get; set; } = new();

        public override bool ReadOnly => true;

        public void Add(string resourceKey, byte[] resourceData)
        {
            Resources[resourceKey.Replace("\\", "/").ToLowerInvariant()] = resourceData;
        }

        public byte[]? Get(string resourceKey)
        {
            //resourceKey = $"{ResourceHelper.ResourceDir}/{resourceKey}".Trim('/');
            resourceKey = resourceKey.Replace("\\", "/").ToLowerInvariant();
            byte[]? dat = null;
            Resources?.TryGetValue(resourceKey, out dat);
            return dat;
        }

        protected override string CustomDeserializeStep(string source)
        {
            return source;
        }

        protected override string CustomSerializeStep(string source)
        {
            return source;
        }

        protected override string GetLoadPath()
        {
            return Path.Combine(AppInfo.AppDirectory, "resources.ures");
        }
    }
}