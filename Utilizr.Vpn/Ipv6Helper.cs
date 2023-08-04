//TODO: fixup for netcore
using System;
//using System.Net.Configuration;

using Utilizr.Logging;

namespace Utilizr.Vpn
{
    public class Ipv6Helper
    {
        private static Ipv6Helper? _instance;
        public static Ipv6Helper Instance => _instance ??= new Ipv6Helper();

        public bool IsEnabled => false;
        //private readonly Ipv6Element _ipv6Element = new Ipv6Element();
        //public bool IsEnabled => _ipv6Element.Enabled;
        
        private readonly bool _originallyEnabled;

        private Ipv6Helper()
        {
            _originallyEnabled = IsEnabled;
        }

        public void Enable()
        {
            try
            {
                //_ipv6Element.Enabled = true;
            }
            catch (Exception ex)
            {
                Log.Exception("ipv6", ex, "Failed to enable");
            }
        }

        public void Disable()
        {
            try
            {
                // _ipv6Element.Enabled = false;
            }
            catch (Exception ex)
            {
                Log.Exception("ipv6", ex, "Failed to disable");
            }
        }

        public void StartIpv6LeakProtection()
        {
            if (_originallyEnabled)
                Disable();
        }

        public void StopIpv6LeakProtection()
        {
            if (_originallyEnabled)
                Enable();
        }
    }
}