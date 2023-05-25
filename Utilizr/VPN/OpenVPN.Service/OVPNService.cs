using System;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using Utilizr.Info;
//using Utilizr.IPC;
using Utilizr.Logging;
//using Utilizr.OpenVPN.ipc;

namespace Utilizr.OpenVPN.Service
{
    public partial class OVPNService : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        private ServiceStatus _status;

        public OVPNService()
        {
            InitializeComponent();
            //setup logger
            Log.BasicConfigure(
                logFilePath: Path.Combine(AppInfo.LogDirectory, "service.log"),
                format: "{Asctime} : {Category,10} : {LevelName} : {Message} : {InterestingObjects}",
                level: LoggingLevel.INFO
            );
        }

        // protected override void OnStart(string[] args)
        // {
        //     _status = new ServiceStatus();
        //     _status.dwCurrentState = ServiceState.SERVICE_START_PENDING;
        //     _status.dwWaitHint = 100000;
        //     SetServiceStatus(this.ServiceHandle, ref _status);
        //
        //     SingletonServiceHelper<WCFService, IWCFService>.StartServer(WCFService.ADDRESS);
        //
        //     _status.dwCurrentState = ServiceState.SERVICE_RUNNING;
        //     SetServiceStatus(this.ServiceHandle, ref _status);
        // }

        // protected override void OnStop()
        // {
        //     _status.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
        //     SetServiceStatus(this.ServiceHandle, ref _status);
        //     SingletonServiceHelper<WCFService, IWCFService>.StopServer();
        //     _status.dwCurrentState = ServiceState.SERVICE_STOPPED;
        //     SetServiceStatus(this.ServiceHandle, ref _status);
        // }
    }

    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public long dwServiceType;
        public ServiceState dwCurrentState;
        public long dwControlsAccepted;
        public long dwWin32ExitCode;
        public long dwServiceSpecificExitCode;
        public long dwCheckPoint;
        public long dwWaitHint;
    };
}
