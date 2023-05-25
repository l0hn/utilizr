using System;
using System.Collections.Specialized;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using Utilizr.Info;
using Utilizr.IPC;
using Utilizr.Logging;
using Utilizr.OpenVPN.ipc;

namespace Utilizr.OpenVPN.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Contains("--install"))
            {
                try
                {
                    Uninstall();
                }
                catch (Exception)
                {}
                try
                {
                    Install();
                }
                catch (Exception ex)
                {
                    Log.Exception("SERVICE_INSTALL", ex);
                    Console.WriteLine(ex);
                    throw;
                }
                return;
            }
            if (args.Contains("--uninstall"))
            {
                Uninstall();
                return;
            }
            if (args.Contains("--run"))
            {
                Console.WriteLine("Staring wf service");
                SingletonServiceHelper<WCFService, IWCFService>.StartServer(WCFService.ADDRESS);
                Console.WriteLine("Press a key to exit");
                var wait = new ManualResetEvent(false);
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    wait.Set();
                };
                wait.WaitOne();
                return;
            }

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new OVPNService()
            };
            ServiceBase.Run(ServicesToRun);
        }

        static void Install()
        {
            //install tap driver
            TapInstaller.InstallTapDriver();
            //install service.
            if (!IsInstalled("OVPNService"))
            {
                var installer = new ServiceProcessInstaller();
                installer.Account = ServiceAccount.LocalSystem;
                var serviceInstaller = new ServiceInstaller();

                var path = Process.GetCurrentProcess().MainModule.FileName;
                path = Path.GetFullPath(path);
                var context = new InstallContext(
                    Path.Combine(AppInfo.LogDirectory, "service_install.log"),
                    new[] { $"/assemblypath={path}" }
                    );

                serviceInstaller.Context = context;
                serviceInstaller.ServiceName = "OVPNService";
                serviceInstaller.DisplayName = "OpenVPN Manager Service";
                serviceInstaller.Description = "Responsible for managing OpenVPN connection processes";
                serviceInstaller.StartType = ServiceStartMode.Manual;
                serviceInstaller.Parent = installer;
                serviceInstaller.Install(new ListDictionary());
            }
            
            //set permissions to allow all users to start/stop the service.
            SetServiceACL("OVPNService");
        }

        static void Uninstall()
        {
            if (!IsInstalled("OVPNService"))
                return;

            var serviceInstaller = new ServiceInstaller();
            var context = new InstallContext(Path.Combine(AppInfo.LogDirectory, "service_install.log"), null);
            serviceInstaller.Context = context;
            serviceInstaller.ServiceName = "OVPNService";
            serviceInstaller.Uninstall(null);
        }

        static void SetServiceACL(string serviceName)
        {
            var subinaclPath = Path.Combine(AppInfo.AppDirectory, "bins");
            subinaclPath = Path.Combine(subinaclPath, "subinacl.exe");
            Shell.Exec(subinaclPath, "/SERVICE", $"\"{serviceName}\"", "/GRANT=everyone=F");
        }

        static bool IsInstalled(string serviceName)
        {
            return ServiceController.GetServices().Any(serviceController => serviceController.ServiceName == serviceName);
        }
    }
}
