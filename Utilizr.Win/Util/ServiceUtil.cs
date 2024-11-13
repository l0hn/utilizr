using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using Utilizr.Async;
using Utilizr.Logging;
using Utilizr.Win32.Advapi32;
using Utilizr.Win32.Advapi32.Flags;
using Utilizr.Win32.Advapi32.Structs;

namespace Utilizr.Win.Util
{
    public static class ServiceUtil
    {
        const string LOG_CAT = "service-util";
        public delegate bool CanChangeServiceStartupTypeDelegate(ServiceStartupType currentStartupType);

        const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;

        public static bool IsInstalled(string serviceName)
        {
            try
            {
                var state = new ServiceController(serviceName).Status;
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public static bool IsRunning(string serviceName)
        {
            try
            {
                return GetServiceStatus(serviceName) == ServiceControllerStatus.Running;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private static ServiceControllerStatus GetServiceStatus(string serviceName)
        {
            using (var controller = new ServiceController(serviceName))
            {
                return controller.Status;
            }
        }

        /// <summary>
        /// Start and wait for the service to be in a running state, timing out after 2 minutes.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns>True if service was already running before check</returns>
        public static bool StartWindowsService(string serviceName)
        {
            return StartWindowsService(serviceName, TimeSpan.FromSeconds(120));
        }

        /// <summary>
        /// Start and optionally wait for the service to be in the running state.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns>True if service was already running before check</returns>
        public static bool StartWindowsService(string serviceName, TimeSpan? timeout = null)
        {
            using (var controller = new ServiceController(serviceName))
            {
                bool wasAlreadyRunning = controller.Status == ServiceControllerStatus.Running;
                if (!wasAlreadyRunning)
                {
                    Log.Info(LOG_CAT, $"Starting {serviceName} service");
                    controller.Start();
                    if (timeout != null)
                        controller.WaitForStatus(ServiceControllerStatus.Running, timeout.Value);
                }
                return wasAlreadyRunning;
            }
        }

        public static void StopWindowsService(string serviceName, TimeSpan? timeout = null)
        {
            try
            {
                if (!IsInstalled(serviceName))
                    return;

                var controller = new ServiceController(serviceName);
                controller.Stop();

                if (timeout != null)
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, timeout.Value);
                else
                    controller.WaitForStatus(ServiceControllerStatus.Stopped);
            }
            catch (Exception ex)
            {
                Log.Exception(LOG_CAT, ex);
            }
        }

        public static void StartServiceWait(string serviceName)
        {
            Log.Info(LOG_CAT, $"Starting service {serviceName}");
            while (GetServiceStatus(serviceName) != ServiceControllerStatus.Running)
            {
                try
                {
                    Log.Info(LOG_CAT, $"Service Status: {GetServiceStatus(serviceName)}");

                    StartWindowsService(serviceName);

                    if (GetServiceStatus(serviceName) != ServiceControllerStatus.Running)
                        Sleeper.Sleep(500);

                }
                catch (Exception e)
                {
                    Log.Exception(LOG_CAT, e, $"Failed to start service '{serviceName}'!");
                }
            }
            Log.Info(LOG_CAT, $"Service status: {GetServiceStatus(serviceName)}");
        }

        public static void StopServiceWait(string serviceName)
        {
            Log.Info(LOG_CAT, $"Stopping service '{serviceName}'");

            while (GetServiceStatus(serviceName) != ServiceControllerStatus.Stopped)
            {
                try
                {
                    Log.Info(LOG_CAT, $"Service Status: {GetServiceStatus(serviceName)}");

                    StopWindowsService(serviceName, TimeSpan.FromSeconds(20));

                    //If its stuck running ater 20 seconds - kill the process!
                    if (GetServiceStatus(serviceName) != ServiceControllerStatus.Stopped)
                    {
                        Log.Info(LOG_CAT, $"Force stop service {GetServiceStatus(serviceName)}");
                        ForceStopService(serviceName);
                    }

                    if (GetServiceStatus(serviceName) != ServiceControllerStatus.Stopped)
                        Sleeper.Sleep(500);
                }
                catch (Exception e)
                {
                    Log.Exception(LOG_CAT, e, "Failed to start service!");
                }
            }
            Log.Info(LOG_CAT, $"Service status: {GetServiceStatus(serviceName)}");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="action">Action to perform while service is stopped before being restarted</param>
        public static void RestartService(string serviceName, Action action)
        {
            try
            {
                Log.Info(LOG_CAT, $"Stopping service '{serviceName}'");
                StopServiceWait(serviceName);
                try
                {
                    Log.Info(LOG_CAT, "Performing restart action");
                    action?.Invoke();
                }
                finally
                {
                    Log.Info(LOG_CAT, $"Restarting service '{serviceName}'");
                    StartServiceWait(serviceName);
                }
            }
            catch (Exception ex)
            {
                Log.Exception(LOG_CAT, ex);
                throw;
            }
        }

        private static void ForceStopService(string serviceName)
        {
            var serviceProc = Process.GetProcessesByName(serviceName).FirstOrDefault();
            serviceProc?.Kill();
        }

        /// <summary>
        /// Attempt to change the startup type of a given service.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="newStartupType"></param>
        /// <param name="allowStartupChange">Optional callback to allow only changing for certain values.</param>
        public static void ChangeServiceStartupType(
            string serviceName, 
            ServiceStartupType newStartupType,
            CanChangeServiceStartupTypeDelegate? allowStartupChange = null)
        {
            IntPtr scmHandle = IntPtr.Zero;
            IntPtr serviceHandle = IntPtr.Zero;

            try
            {
                //Obtain a handle to the service control manager database
                scmHandle = Advapi32.OpenSCManager(null, null, (uint)ScmAccessRights.SC_MANAGER_CONNECT);
                if (scmHandle == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to obtain a handle to the service control manager database.");

                //Obtain a handle to the specified windows service
                serviceHandle = Advapi32.OpenService(scmHandle, serviceName, (uint)(ServiceAccessRights.SERVICE_QUERY_CONFIG | ServiceAccessRights.SERVICE_CHANGE_CONFIG));
                if (serviceHandle == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), $"Failed to obtain a handle to service '{serviceName}'.");


                uint dwBytesNeeded = 0;
                Advapi32.QueryServiceConfig(serviceHandle, IntPtr.Zero, 0, out dwBytesNeeded); // get size we need

                IntPtr ptr = Marshal.AllocHGlobal((int)dwBytesNeeded);
                if (!Advapi32.QueryServiceConfig(serviceHandle, ptr, dwBytesNeeded, out dwBytesNeeded))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), $"Failed to obtain a config information to service '{serviceName}'.");

                var queryServiceConfig = new QueryServiceConfig();
                Marshal.PtrToStructure(ptr, queryServiceConfig);

                bool canChange;
                try
                {
                    canChange = allowStartupChange?.Invoke((ServiceStartupType)queryServiceConfig.dwStartType) == true;
                }
                finally
                {
                    // don't leak if windows adds a new startup option
                    Marshal.FreeHGlobal(ptr);
                }

                if (!canChange)
                    return;

                //Change the start mode
                bool changeServiceSuccess = Advapi32.ChangeServiceConfig(
                    serviceHandle,
                    SERVICE_NO_CHANGE,
                    (uint)newStartupType,
                    SERVICE_NO_CHANGE,
                    null,
                    null,
                    IntPtr.Zero,
                    null,
                    null,
                    null,
                    null
                );

                if (!changeServiceSuccess)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), $"Failed to update service configuration for service '{serviceName}'.");
            }
            finally
            {
                //Clean up
                if (scmHandle != IntPtr.Zero)
                    Advapi32.CloseServiceHandle(scmHandle);

                if (serviceHandle != IntPtr.Zero)
                    Advapi32.CloseServiceHandle(serviceHandle);
            }
        }
    }
}
