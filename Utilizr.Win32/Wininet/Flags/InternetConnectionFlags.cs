using System;

namespace Utilizr.Win32.Wininet.Flags
{
    [Flags]
    public enum InternetConnectionFlags : int
    {
        INTERNET_CONNECTION_MODEM           = 0x01,
        INTERNET_CONNECTION_LAN             = 0x02,
        INTERNET_CONNECTION_PROXY           = 0x04,
        INTERNET_CONNECTION_MODEM_BUSY      = 0x08,
        INTERNET_RAS_INSTALLED              = 0x10,
        INTERNET_CONNECTION_OFFLINE         = 0x20,
        INTERNET_CONNECTION_CONFIGURED      = 0x40,
    }
}
