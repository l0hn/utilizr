﻿using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using Utilizr.Async;
using Utilizr.Logging;

namespace Utilizr.Win.Util
{
    public static class ServiceUtil
    {
        const string LOG_CAT = "service-util";

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

        public static void StartWindowsService(string serviceName)
        {
            using (var controller = new ServiceController(serviceName))
            {
                if (controller.Status != ServiceControllerStatus.Running)
                {
                    Log.Info(LOG_CAT, $"Starting {serviceName} service");
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(120));
                }
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
    }
}