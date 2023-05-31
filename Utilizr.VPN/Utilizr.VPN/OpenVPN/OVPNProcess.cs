using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Utilizr.Extensions;
using Utilizr.FileSystem;
using Utilizr.Info;
using Utilizr.Logging;
using Utilizr.Network;
using Utilizr.Win;
using Utilizr.Win.Service;

namespace Utilizr.OpenVPN
{
    public class OVPNProcess: IDisposable
    {
        public const string HANDLE = "Global\\com.utilizr.vpn.ovpnhandle.68493289234698";
        public Process Proc { get; private set; }
        ///// <summary>
        ///// Null when the process is no longer running
        ///// </summary>
        //public uint? ProcessId { get; private set; }
        public int ManagementPort { get; private set; }
        public string RemoteHost { get; private set; }

        //StreamWriter _standardInput;

        string CreateCommandLineArgs(Dictionary<string, string> customOptions, string managementPwd)
        {
            var argsDict = new Dictionary<string, string>();
            foreach (var customOption in customOptions)
            {
                argsDict.Add(customOption.Key, customOption.Value);
            }

            ManagementPort = NetUtil.GetRandomAvailablePort(30000, 40000);


            var logPath = Path.Combine(AppInfo.LogDirectory, "ovpn.log");

            //overrides anything in customOptions
            var mandatoryOptions = new Dictionary<string, string>()
            {
                {"--client", ""},
                {"--remote", RemoteHost},
                {"--nobind", ""},
                {"--dev", "tun"},
                {"--auth-nocache", ""},
                {"--script-security", "3"},
                {"--auth-user-pass", ""},
                {"--explicit-exit-notify", ""},
                {"--management-hold", ""},
                {"--management-query-passwords", ""},
                {"--management-up-down", ""},
                {"--management-forget-disconnect", ""},
                {"--auth-retry", "interact"},
                {"--up-restart", "" }
            };

#if !MONO
            if (Platform.IsWindows && !Platform.IsXPOrLower)//There's no GUID in Win32_NetworkAdapter on Windows XP
            {
                try
                {
                    var adapterGuid = TapInstaller.GetTapAdapterInfo().GUID;
                    mandatoryOptions.Add("--dev-node", adapterGuid);
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
#endif

            if (managementPwd.IsNullOrEmpty())
            {
                mandatoryOptions.Add("--management", $"127.0.0.1 {ManagementPort}");
            }
            else
            {
                try
                {
                    var pwFile = Path.Combine(AppInfo.DataDirectory, "ovpn.mgmnt");
                    File.WriteAllText(pwFile, managementPwd);
                    mandatoryOptions.Add("--management", $"127.0.0.1 {ManagementPort} {EscapeAndQuotePath(pwFile)}");
                }
                catch (Exception e)
                {
                    mandatoryOptions.Add("--management", $"127.0.0.1 {ManagementPort}");
                }
            }

            //doesn't override customOptions
            var saneDefaults = new Dictionary<string, string>()
            {
                { "--port", "1194"},
                { "--ping", "10"},
                { "--ping-exit", "30"},
                { "--proto", "udp"},
                {"--log-append", $"\"{logPath}\""},
                {"--block-outside-dns", ""}
            };

            if (Platform.IsXPOrLower)
                saneDefaults.Remove("--block-outside-dns");
            
            foreach (var mandatoryOption in mandatoryOptions)
            {
                argsDict[mandatoryOption.Key] = mandatoryOption.Value;
            }
            foreach (var saneDefault in saneDefaults)
            {
                if (!argsDict.ContainsKey(saneDefault.Key))
                    argsDict[saneDefault.Key] = saneDefault.Value;
            }
            
            if (Platform.IsWindows)
            {
                argsDict["--service"] = $"{HANDLE} 0";
            }

            if (argsDict.TryGetValue("--proto", out var protoVal) && protoVal.ToLower() == "tcp")
            {
                argsDict.Remove("--explicit-exit-notify");
            }

            var sb = new StringBuilder();
            foreach (var arg in argsDict)
            {
                sb.Append($"{arg.Key} {arg.Value}".Trim() + " ");
#if DEBUG
                Console.WriteLine($"{arg.Key} {arg.Value}");
#endif
            }
            return sb.ToString().Trim();
        }

        void StartDhcpService()
        {
#if !MONO
            try
            {
                ServiceUtil.StartWindowsService("dhcp");
            }
            catch (Exception e)
            {
                Log.Exception("ovpn_proc", e);
            }
#endif
        }

        public static string OvpnBinaryPath
        {
            get
            {
                string command = Platform.IsWindows ? "openvpn.exe" : "openvpn";

                var file = Path.Combine(AppInfo.AppDirectory, "ovpn");

                file = Path.Combine(file, command);

                return file;
            }
        }

        void Start(Dictionary<string, string> options, string managementPwd)
        {
            try
            {
                Shell.Exec("netsh", null, "interface", "ip", "delete", "destinationcache");
            }
            catch (Exception e)
            {
                
            }

            StartDhcpService();

            var args = CreateCommandLineArgs(options, managementPwd);

            //Shell.ExecProtectedAsync(
            //    new ProcessStartInfo
            //    {
            //        FileName = OvpnBinaryPath,
            //        Arguments = args,
            //        RedirectStandardInput = true,
            //        RedirectStandardOutput = true,
            //        RedirectStandardError = true,
            //        CreateNoWindow = true,
            //    },
            //    (pid) => 
            //    { 
            //        ProcessId = pid;
            //        Proc = Process.GetProcessById((int)pid);
            //    },
            //    (inputWriter) => { _standardInput = inputWriter; },
            //    (outputLine) => { Log.Info("OVPN_STDOUT", outputLine); },
            //    (errorLine) => { Log.Info("OVPN_STDERR", errorLine); }
            //);

            Proc = new Process();
            Proc.StartInfo = new ProcessStartInfo(OvpnBinaryPath, args);
            Proc.StartInfo.UseShellExecute = false;
            Proc.StartInfo.RedirectStandardOutput = true;
            Proc.StartInfo.RedirectStandardError = true;
            Proc.StartInfo.RedirectStandardInput = true;
            //Proc.StartInfo.CreateNoWindow = true;
            //Proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Proc.OutputDataReceived += (o, eventArgs) =>
            {
                if (eventArgs.Data == null)
                    return;

                Log.Info("OVPN_STDOUT", eventArgs.Data);
            };
            Proc.ErrorDataReceived += (sender, eventArgs) =>
            {
                if (eventArgs.Data == null)
                    return;
                Log.Error("OVPN_STDERR", eventArgs.Data);
            };
            Proc.Start();
            Proc.BeginOutputReadLine();
            Proc.BeginErrorReadLine();
        }




        string EscapeAndQuotePath(string path)
        {
            if (Platform.IsWindows)
                return $"\"{path.Replace(@"\", @"\\")}\"";

            return $"\"{path}\"";
        }


        public static OVPNProcess FromConfig(string remote, string config, string upScript = null, string downscript = null, int pingTimeout = 30, string managementPwd = null, int port = 1194, string protocol = "udp")
        {
            return new OVPNProcess(remote, config, upScript, downscript, pingTimeout, managementPwd, port, protocol);
        }

        public static OVPNProcess WithCAFile(string remote, string caFile, string proto="udp", int pingTimeout=30, string upScript = null, string downscript = null, string managementPwd = null)
        {
            return new OVPNProcess(remote, caFile, proto, pingTimeout, upScript, downscript, managementPwd);
        }

        /// <summary>
        /// Create a new open vpn client process
        /// Will auto configure many things such as management interface port and management-hold arguments.
        /// I.E you will need to connect to the management interface and issue hold release
        /// </summary>
        /// <param name="remote">remote host to connect to</param>
        /// <param name="caFile">path to openvpn certificate</param>
        /// <param name="proto">udp / tcp</param>
        /// <param name="upScript">optional bat file to run on up</param>
        /// <param name="downScript">optional bat file to run on down</param>
        private OVPNProcess(string remote, string caFile, string proto, int pingTimeout, string upScript, string downScript, string managementPwd = null, int port = 1194)
        {
            RemoteHost = remote;
            var args = new Dictionary<string, string>()
            {
                {"--cipher", "AES-256-CBC"},
                {"--proto", proto},
                {"--ca", $"\"{caFile}\"" },
                {"--ping-exit", $"{pingTimeout}" },
                {"--ping", "10" },
                {"--port", $"{port}" },
            };

            if (upScript.IsNotNullOrEmpty())
                args["--up"] = EscapeAndQuotePath(upScript);

            if (downScript.IsNotNullOrEmpty())
                args["--down"] = EscapeAndQuotePath(downScript);


            Start(args, managementPwd);
        }

        private OVPNProcess(string remote, string configFile, string upScript, string downScript, int pingTimeout = 30, string managementPwd = null, int port = 1194, string proto = "udp")
        {
            RemoteHost = remote;
            var args = new Dictionary<string, string>()
            {
                {"--config", EscapeAndQuotePath(configFile)},
                {"--port", $"{port}" },
                {"--proto", proto }
            };

            if (upScript.IsNotNullOrEmpty())
                args["--up"] = EscapeAndQuotePath(upScript);

            if (downScript.IsNotNullOrEmpty())
                args["--down"] = EscapeAndQuotePath(downScript);

            args["--ping-exit"] = pingTimeout.ToString();

            Start(args, managementPwd);
        }

        public bool Running()
        {
            if (Proc == null)
                return false; // Something must have gone wrong during setup

            Proc.Refresh();
            try
            {
                return !Proc.HasExited;
            }
            catch (Exception)
            { }
            return false;
        }

        public void Dispose()
        {
            if (Running())
            {
                try
                {
                    Proc.StandardInput.WriteLine("\x3");
                    //_standardInput.WriteLine("\x3");
                }
                catch (Exception)
                {
                    
                }
                // A few seconds to allow --down script to run
                var ellapsed = 0;
                var waitTime = 100;
                while (Proc?.HasExited == false && ellapsed < 10000)
                {
                    Thread.Sleep(waitTime);
                    ellapsed += waitTime;
                }
                if (Proc?.HasExited == false)
                {
                    // down script probably hasn't executed...
                    Proc?.Kill();
                }
            }
            Proc?.Close();
            Proc?.Dispose();
        }

        ~OVPNProcess()
        {
            try
            {
                Dispose();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

//        [DllImport("kernel32.dll", SetLastError = true)]
//        static extern bool GenerateConsoleCtrlEvent(ConsoleCtrlEvent sigevent, int dwProcessGroupId);
//
//        public enum ConsoleCtrlEvent
//        {
//            CTRL_C = 0,
//            CTRL_BREAK = 1,
//            CTRL_CLOSE = 2,
//            CTRL_LOGOFF = 5,
//            CTRL_SHUTDOWN = 6
//        }
    }
}