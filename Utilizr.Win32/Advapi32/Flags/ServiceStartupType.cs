using System;

namespace Utilizr.Win32.Advapi32.Flags
{
    public enum ServiceStartupType : uint
    {
        /// <summary>
        /// A device driver started by the system loader. This value is valid only for driver services.
        /// </summary>
        BootStart = 0,

        /// <summary>
        /// A device driver started by the IoInitSystem function. This value is valid only for driver services.
        /// </summary>
        SystemStart = 1,

        /// <summary>
        /// A service started automatically by the service control manager during system startup.
        /// </summary>
        Automatic = 2,

        /// <summary>
        /// A service started by the service control manager when a process calls the StartService function.
        /// </summary>
        Manual = 3,

        /// <summary>
        /// A service that cannot be started. Attempts to start the service result in the error code ERROR_SERVICE_DISABLED.
        /// </summary>
        Disabled = 4
    }
}
