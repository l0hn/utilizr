using System;

namespace Utilizr.Win32.Advapi32.Flags
{
    [Flags]
    public enum ScmAccessRights : uint
    {
        /// <summary>
        /// Required to connect to the service control manager.
        /// </summary>
        SC_MANAGER_CONNECT =                0x1,

        /// <summary>
        /// Required to call the CreateService function to create a service object and add it to the database.
        /// </summary>
        SC_MANAGER_CREATE_SERVICE =         0x2,

        /// <summary>
        /// Required to call the EnumServicesStatus or EnumServicesStatusEx function to list the services that are in the database.
        /// Required to call the NotifyServiceStatusChange function to receive notification when any service is created or deleted.
        /// </summary>
        SC_MANAGER_ENUMERATE_SERVICE =      0x4,

        /// <summary>
        /// Required to call the LockServiceDatabase function to acquire a lock on the database.
        /// </summary>
        SC_MANAGER_LOCK =                   0x8,

        /// <summary>
        /// Required to call the QueryServiceLockStatus function to retrieve the lock status information for the database.
        /// </summary>
        SC_MANAGER_QUERY_LOCK_STATUS =      0x10,

        /// <summary>
        /// Required to call the NotifyBootConfigStatus function.
        /// </summary>
        SC_MANAGER_MODIFY_BOOT_CONFIG =     0x20,
    }


    [Flags]
    public enum ServiceAccessRights : uint
    {
        /// <summary>
        /// Required to call the QueryServiceConfig and QueryServiceConfig2 functions to query the service configuration.
        /// </summary>
        SERVICE_QUERY_CONFIG =                              0x1,

        /// <summary>
        /// Required to call the ChangeServiceConfig or ChangeServiceConfig2 function to change the service configuration.
        /// Because this grants the caller the right to change the executable file that the system runs, it should be granted only to administrators.
        /// </summary>
        SERVICE_CHANGE_CONFIG =                             0x2,

        /// <summary>
        /// Required to call the QueryServiceStatus or QueryServiceStatusEx function to ask the service control manager about the status of the service.
        /// Required to call the NotifyServiceStatusChange function to receive notification when a service changes status.
        /// </summary>
        SERVICE_QUERY_STATUS =                              0x4,

        /// <summary>
        /// Required to call the EnumDependentServices function to enumerate all the services dependent on the service.
        /// </summary>
        SERVICE_ENUMERATE_DEPENDENTS =                       0x8,

        /// <summary>
        /// Required to call the StartService function to start the service.
        /// </summary>
        SERVICE_START =                                     0x10,

        /// <summary>
        /// Required to call the ControlService function to stop the service.
        /// </summary>
        SERVICE_STOP =                                      0x20,

        /// <summary>
        /// Required to call the ControlService function to pause or continue the service.
        /// </summary>
        SERVICE_PAUSE_CONTINUE =                            0x40,

        /// <summary>
        /// Required to call the ControlService function to ask the service to report its status immediately.
        /// </summary>
        SERVICE_INTERROGATE =                               0x80,

        /// <summary>
        /// Required to call the ControlService function to specify a user-defined control code.
        /// </summary>
        SERVICE_USER_DEFINED_CONTROL =                      0x100,

        /// <summary>
        /// Includes STANDARD_RIGHTS_REQUIRED in addition to all access rights in this table.
        /// </summary>
        SERVICE_ALL_ACCESS =                                0xF01FF,
    }
}
