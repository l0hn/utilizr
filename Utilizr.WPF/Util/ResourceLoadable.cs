using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Utilizr.Info;
using Utilizr.Win.Util;

namespace Utilizr.WPF.Util
{
    public class ResourceLoadable : LoadableEmbedded<ResourceLoadable>
    {
        [JsonProperty("_resources")]
        private readonly Dictionary<string, byte[]> _resources;

        public override string EmbeddedResourceName => "RESOURCES.URES";

        public override bool ReadOnly => true;

        public ResourceLoadable()
        {
            _resources = new Dictionary<string, byte[]>();
        }

        public void Add(string resourceKey, byte[] resourceData)
        {
            _resources[resourceKey.Replace("\\", "/").ToLowerInvariant()] = resourceData;
        }

        public byte[]? Get(string resourceKey)
        {
            //resourceKey = $"{ResourceHelper.ResourceDir}/{resourceKey}".Trim('/');
            resourceKey = resourceKey.Replace("\\", "/").ToLowerInvariant();
            byte[]? dat = null;
            _resources?.TryGetValue(resourceKey, out dat);
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
