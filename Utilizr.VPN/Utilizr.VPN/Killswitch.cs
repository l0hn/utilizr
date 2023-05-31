using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Utilizr.Logging;
using Utilizr.OpenVPN;
using Utilizr.Extensions;

namespace Utilizr.VPN
{
    public class Killswitch
    {
        const string _logCat = "killswitch";

        [DllImport("Netlib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int KillswitchEngage2(
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]  ADDR_AND_MASK[] remoteAddrs,
            int addrCount,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]  ADDR_AND_MASK[] localAddrs,
            int localAddrCount,
            uint tapAdapterIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string ovpnBinaryPath,
            [MarshalAs(UnmanagedType.Bool)] bool persistReboot,
            [MarshalAs(UnmanagedType.LPWStr)] string displayName);

        [DllImport("Netlib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int KillswitchDisengage();

        [DllImport("Netlib.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern Boolean KillswitchIsEngaged();

        readonly string _appName;

        public Killswitch(string appName)
        {
            _appName = appName;
        }

        public bool IsEngaged()
        {
            return KillswitchIsEngaged();
        }

        public void Engage(HostEntry[] hostEntries, ADDR_AND_MASK[] remoteAddrs, ADDR_AND_MASK[] localAddrs, bool persistReboot, string displayName)
        {
            uint adapter = 999999;

            try
            {
                adapter = TapInstaller.GetTapAdapterIndex();
            }
            catch (Exception)
            {
                Log.Warning(_logCat, "failed to get tap adapter index, killswitch will still be enabled");
            }

            AddHostFileEntries(hostEntries);

            Log.Info(_logCat, $"enabling killswitch for adapter:{adapter} reboot:{persistReboot}");
#if DEBUG
            Console.WriteLine($"enabling killswitch for adapter:{adapter} reboot:{persistReboot}");
#endif

            var res = KillswitchEngage2(
                remoteAddrs,
                remoteAddrs.Length,
                localAddrs,
                localAddrs.Length,
                adapter,
                OVPNProcess.OvpnBinaryPath,
                persistReboot,
                displayName);

            if (res != 0)
            {
                throw new Win32Exception(res);
            }
        }

        void AddHostFileEntries(HostEntry[] hostsEntries)
        {
            var hostFilePath = "%windir%\\system32\\drivers\\etc\\hosts".ExpandVars();
            var fileData = File.ReadAllLines(hostFilePath).ToList();
            if (fileData.Count > 0)
            {
                for (int i = fileData.Count - 1; i > 0; i--)
                {
                    if (fileData[i].EndsWith($"#{_appName} do not modify"))
                    {
                        fileData.RemoveAt(i);
                    }
                }
            }

            foreach (var hostsEntry in hostsEntries)
            {
                fileData.Add($"{hostsEntry.IpAddress} {hostsEntry.Hostname} #{_appName} do not modify");
            }

            File.WriteAllLines(hostFilePath, fileData.ToArray());
        }

        void DeleteHostEntries()
        {
            var hostFilePath = "%windir%\\system32\\drivers\\etc\\hosts".ExpandVars();
            var fileData = File.ReadAllLines(hostFilePath).ToList();

            if (fileData.Count > 0)
            {
                for (int i = fileData.Count - 1; i > 0; i--)
                {
                    if (fileData[i].EndsWith($"#{_appName} do not modify"))
                    {
                        fileData.RemoveAt(i);
                    }
                }
            }

            File.WriteAllLines(hostFilePath, fileData.ToArray());
        }

        public void Disengage()
        {
            try
            {
                DeleteHostEntries();
            }
            catch (Exception e)
            {
                Log.Exception(_logCat, e);
            }

            Log.Info(_logCat, $"disabling killswitch");

            var res = KillswitchDisengage();
            if (res != 0)
            {
                https://learn.microsoft.com/en-us/windows/win32/api/fwpmu/nf-fwpmu-fwpmengineopen0
                throw new Win32Exception(res, $"Filter engine open result '{res}'");
            }
        }

        public static ADDR_AND_MASK[] GetLocalIPv4Addrs()
        {
            var output = new List<ADDR_AND_MASK>();
            foreach (var item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.OperationalStatus == OperationalStatus.Up)
                {
                    var adapterProperties = item.GetIPProperties();

                    if (adapterProperties.GatewayAddresses.FirstOrDefault() != null)
                    {
                        foreach (var ip in adapterProperties.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                output.Add(new ADDR_AND_MASK()
                                {
                                    szIpAddr = ip.Address.ToString(),
                                    szMask = ip.IPv4Mask.ToString(),
                                });
                            }
                        }
                    }
                }
            }

            return output.ToArray();
        }
    }

    public enum KillswitchMode
    {
        Manual,
        Automatic,
    }
}