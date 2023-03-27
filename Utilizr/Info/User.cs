using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Runtime.InteropServices;
using Utilizr.Async;
using Utilizr.Logging;
using Utilizr.Win32.Advapi32;
using Utilizr.Win32.Advapi32.Flags;
using Utilizr.Win32.Advapi32.Structs;

namespace Utilizr.Windows
{
    public static class User
    {
        public static SecurityIdentifier GetCurrentUserSID()
        {
            var identity = WindowsIdentity.GetCurrent();

            if (identity.User == null)
                throw new Exception("WindowsIdentity.User returned null security identifier.");

            return identity.User;
        }

        public static string GetCurrentUserSIDString()
        {
            return GetCurrentUserSID().ToString();
        }

        public static string GetCurrentUsername()
        {
            return WindowsIdentity.GetCurrent().Name;
        }

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool IsDomainUser()
        {
            return IsDomainUser(GetCurrentUsername());
        }

        /// <summary>
        /// Whether the user is on a domain account.
        /// </summary>
        /// <param name="userName">The username as specified from WindowsIdentity.Name</param>
        public static bool IsDomainUser(string userName)
        {
            // Correct method to check whether the user is on a domain is only available in .NET 3.5+ via PrincipalContext.
            // http://stackoverflow.com/a/12710452/1229237. Was using reflection to invoke the correct functions if it's available.
            // However, appear to need to use the overload where you specify the server, kept getting "The server could not
            // be contacted". Not sure how you could get that information. Tried using IPGlobalProperties.GetIPGlobalProperties()
            // to access domain name property, however MSDN says that the domain name will still be returned when you have left
            // the domain: https://msdn.microsoft.com/en-us/library/system.net.networkinformation.ipglobalproperties.domainname(v=vs.100).aspx
            // Instead, using what would have been the fallback if the reflection failed, just check that the machine's name doesn't
            // match on the username. 

            var userIn = userName.Split('\\')?.FirstOrDefault();
            bool isDomain = userIn?.ToUpperInvariant() != Environment.MachineName.ToUpperInvariant();
            return isDomain;
        }

        /// <summary>
        /// Waits for and duplicates the token from explorer.exe to return the logon user's SID.
        /// </summary>
        /// <returns></returns>
        public static SecurityIdentifier? GetLogonUserSID()
        {
            SecurityIdentifier? logonIdentifier = null;
            try
            {
                var explorerProcess = GetExplorerProcess(out _);
                if (explorerProcess == null)
                    return null;

                var token = Security.DuplicateProcessToken((uint)explorerProcess.Id);
                if (token == IntPtr.Zero)
                    return null;

                var safeHandle = new SafeAccessTokenHandle(token);
                WindowsIdentity.RunImpersonated(
                    safeHandle,
                    () => logonIdentifier = WindowsIdentity.GetCurrent().User
                );

                safeHandle.Close();
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }

            return logonIdentifier;
        }

        /// <summary>
        /// Blocks while waiting to get the explorer process.
        /// </summary>
        /// <param name="maxAttempt"></param>
        /// <param name="delaySecondsPerAttempt"></param>
        /// <returns>Null if process is still not running after exceeding <paramref name="maxAttempt"/></returns>
        public static Process? GetExplorerProcess(
            out int attemptsMade,
            int maxAttempt = 3,
            int delaySecondsPerAttempt = 10)
        {
            void sleep(int attemptCount)
            {
                if (attemptCount < maxAttempt)
                {
                    Sleeper.Sleep(Time.Time.SECOND * delaySecondsPerAttempt);
                }
            }

            attemptsMade = 0;
            while (attemptsMade < maxAttempt)
            {
                attemptsMade++;
                try
                {
                    var explorerProcess = Process.GetProcessesByName("explorer").FirstOrDefault();
                    if (explorerProcess != null)
                        return explorerProcess;

                    sleep(attemptsMade);
                }
                catch (Exception)
                {
                    sleep(attemptsMade);
                }
            }

            return null;
        }

        /// <summary>
        /// Wraps a function in impersonation using a linked token
        /// </summary>
        /// <typeparam name="T">Functions return type</typeparam>
        /// <param name="func">Func object</param>
        /// <returns>Return value of wrapped function</returns>
        public static T RunAsImpersonated<T>(Func<T> func) where T : new()
        {
            IntPtr hToken = IntPtr.Zero;
            var returnedValue = new T();

            if (Advapi32.OpenProcessToken(Process.GetCurrentProcess().Handle, (uint)TOKEN_PRIVILEGE_FLAGS.TOKEN_QUERY, ref hToken) == 0)
            {
                return returnedValue;
            }

            try
            {
                var elevationResult = TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault;
                int bufferSize = Marshal.SizeOf((int)elevationResult);
                IntPtr pElevationType = Marshal.AllocHGlobal(bufferSize);

                bool result = Advapi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenElevationType, pElevationType, bufferSize, ref bufferSize);
                if (result)
                {
                    elevationResult = (TOKEN_ELEVATION_TYPE)Marshal.ReadInt32(pElevationType);
                }

                if (elevationResult == TOKEN_ELEVATION_TYPE.TokenElevationTypeFull)
                {
                    bufferSize = sizeof(uint);

                    IntPtr pLinkedToken = Marshal.AllocHGlobal(bufferSize);
                    result = Advapi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenLinkedToken, pLinkedToken, bufferSize, ref bufferSize);

                    if (result)
                    {
                        var linkedToken = Marshal.PtrToStructure<TOKEN_LINKED_TOKEN>(pLinkedToken);
                        var tokenHandle = new SafeAccessTokenHandle(linkedToken.LinkedToken);
                        returnedValue = WindowsIdentity.RunImpersonated(tokenHandle, () => func());
                    }

                    Marshal.FreeHGlobal(pLinkedToken);
                }

                Marshal.FreeHGlobal(pElevationType);
            }
            catch (Exception)
            {
                throw;
            }

            return returnedValue;
        }
    }
}