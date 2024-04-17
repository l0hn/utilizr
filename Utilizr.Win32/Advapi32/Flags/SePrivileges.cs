using System;

namespace Utilizr.Win32.Advapi32.Flags
{
    public enum SecurityEntity
    {
        SE_CREATE_TOKEN_NAME,
        SE_ASSIGNPRIMARYTOKEN_NAME,
        SE_LOCK_MEMORY_NAME,
        SE_INCREASE_QUOTA_NAME,
        SE_UNSOLICITED_INPUT_NAME,
        SE_MACHINE_ACCOUNT_NAME,
        SE_TCB_NAME,
        SE_SECURITY_NAME,
        SE_TAKE_OWNERSHIP_NAME,
        SE_LOAD_DRIVER_NAME,
        SE_SYSTEM_PROFILE_NAME,
        SE_SYSTEMTIME_NAME,
        SE_PROF_SINGLE_PROCESS_NAME,
        SE_INC_BASE_PRIORITY_NAME,
        SE_CREATE_PAGEFILE_NAME,
        SE_CREATE_PERMANENT_NAME,
        SE_BACKUP_NAME,
        SE_RESTORE_NAME,
        SE_SHUTDOWN_NAME,
        SE_DEBUG_NAME,
        SE_AUDIT_NAME,
        SE_SYSTEM_ENVIRONMENT_NAME,
        SE_CHANGE_NOTIFY_NAME,
        SE_REMOTE_SHUTDOWN_NAME,
        SE_UNDOCK_NAME,
        SE_SYNC_AGENT_NAME,
        SE_ENABLE_DELEGATION_NAME,
        SE_MANAGE_VOLUME_NAME,
        SE_IMPERSONATE_NAME,
        SE_CREATE_GLOBAL_NAME,
        SE_CREATE_SYMBOLIC_LINK_NAME,
        SE_INC_WORKING_SET_NAME,
        SE_RELABEL_NAME,
        SE_TIME_ZONE_NAME,
        SE_TRUSTED_CREDMAN_ACCESS_NAME
    }

    public static class SePrivileges
    {
        public const int SE_PRIVILEGE_ENABLED = 0x00000002;
        public const int ERROR_NOT_ALL_ASSIGNED = 1300;

        public static string GetSecurityEntityValue(SecurityEntity securityEntity)
        {
            return securityEntity switch
            {
                SecurityEntity.SE_ASSIGNPRIMARYTOKEN_NAME => "SeAssignPrimaryTokenPrivilege",
                SecurityEntity.SE_AUDIT_NAME => "SeAuditPrivilege",
                SecurityEntity.SE_BACKUP_NAME => "SeBackupPrivilege",
                SecurityEntity.SE_CHANGE_NOTIFY_NAME => "SeChangeNotifyPrivilege",
                SecurityEntity.SE_CREATE_GLOBAL_NAME => "SeCreateGlobalPrivilege",
                SecurityEntity.SE_CREATE_PAGEFILE_NAME => "SeCreatePagefilePrivilege",
                SecurityEntity.SE_CREATE_PERMANENT_NAME => "SeCreatePermanentPrivilege",
                SecurityEntity.SE_CREATE_SYMBOLIC_LINK_NAME => "SeCreateSymbolicLinkPrivilege",
                SecurityEntity.SE_CREATE_TOKEN_NAME => "SeCreateTokenPrivilege",
                SecurityEntity.SE_DEBUG_NAME => "SeDebugPrivilege",
                SecurityEntity.SE_ENABLE_DELEGATION_NAME => "SeEnableDelegationPrivilege",
                SecurityEntity.SE_IMPERSONATE_NAME => "SeImpersonatePrivilege",
                SecurityEntity.SE_INC_BASE_PRIORITY_NAME => "SeIncreaseBasePriorityPrivilege",
                SecurityEntity.SE_INCREASE_QUOTA_NAME => "SeIncreaseQuotaPrivilege",
                SecurityEntity.SE_INC_WORKING_SET_NAME => "SeIncreaseWorkingSetPrivilege",
                SecurityEntity.SE_LOAD_DRIVER_NAME => "SeLoadDriverPrivilege",
                SecurityEntity.SE_LOCK_MEMORY_NAME => "SeLockMemoryPrivilege",
                SecurityEntity.SE_MACHINE_ACCOUNT_NAME => "SeMachineAccountPrivilege",
                SecurityEntity.SE_MANAGE_VOLUME_NAME => "SeManageVolumePrivilege",
                SecurityEntity.SE_PROF_SINGLE_PROCESS_NAME => "SeProfileSingleProcessPrivilege",
                SecurityEntity.SE_RELABEL_NAME => "SeRelabelPrivilege",
                SecurityEntity.SE_REMOTE_SHUTDOWN_NAME => "SeRemoteShutdownPrivilege",
                SecurityEntity.SE_RESTORE_NAME => "SeRestorePrivilege",
                SecurityEntity.SE_SECURITY_NAME => "SeSecurityPrivilege",
                SecurityEntity.SE_SHUTDOWN_NAME => "SeShutdownPrivilege",
                SecurityEntity.SE_SYNC_AGENT_NAME => "SeSyncAgentPrivilege",
                SecurityEntity.SE_SYSTEM_ENVIRONMENT_NAME => "SeSystemEnvironmentPrivilege",
                SecurityEntity.SE_SYSTEM_PROFILE_NAME => "SeSystemProfilePrivilege",
                SecurityEntity.SE_SYSTEMTIME_NAME => "SeSystemtimePrivilege",
                SecurityEntity.SE_TAKE_OWNERSHIP_NAME => "SeTakeOwnershipPrivilege",
                SecurityEntity.SE_TCB_NAME => "SeTcbPrivilege",
                SecurityEntity.SE_TIME_ZONE_NAME => "SeTimeZonePrivilege",
                SecurityEntity.SE_TRUSTED_CREDMAN_ACCESS_NAME => "SeTrustedCredManAccessPrivilege",
                SecurityEntity.SE_UNDOCK_NAME => "SeUndockPrivilege",
                SecurityEntity.SE_UNSOLICITED_INPUT_NAME => "SeUnsolicitedInputPrivilege",
                _ => throw new ArgumentOutOfRangeException(typeof(SecurityEntity).Name),
            };
        }
    }
}