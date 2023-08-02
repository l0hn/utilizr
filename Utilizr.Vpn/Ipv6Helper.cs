//TODO: fixup for netcore

using System;
#if !NETCOREAPP
using System.Net.Configuration;    
#endif

using Utilizr.Logging;

namespace Utilizr.VPN.Win
{
    public class Ipv6Helper
    {
        private static Ipv6Helper _instance;
        public static Ipv6Helper Instance => _instance ?? (_instance = new Ipv6Helper());

#if NETCOREAPP
        public bool IsEnabled => false;
#else        
        private readonly Ipv6Element _ipv6Element = new Ipv6Element();
        public bool IsEnabled => _ipv6Element.Enabled;
#endif
        
        private readonly bool _originallyEnabled;        

        private Ipv6Helper()
        {
            _originallyEnabled = IsEnabled;
        }

        public void Enable()
        {
            try
            {
#if NETCOREAPP
                //_ipv6Element.Enabled = true;
#else
                _ipv6Element.Enabled = true;
#endif
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
#if NETCOREAPP
                // _ipv6Element.Enabled = false;
#else
                _ipv6Element.Enabled = false;
#endif
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