using System;

namespace Utilizr.VPN
{
    [Serializable]
    public class HostEntry
    {
        public string Hostname { get; set; }
        public string IpAddress { get; set; }

        public HostEntry(string hostname, string ipAddress)
        {
            Hostname = hostname;
            IpAddress = ipAddress;
        }
    }
}