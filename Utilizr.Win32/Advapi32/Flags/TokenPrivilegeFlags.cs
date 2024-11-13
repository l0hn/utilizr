using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win32.Advapi32.Flags
{
    //https://learn.microsoft.com/en-us/windows/win32/secauthz/access-rights-for-access-token-objects

    [Flags]
    public enum TOKEN_PRIVILEGE_FLAGS : uint
    {
        TOKEN_ASSIGN_PRIMARY =                                  0x1,
        TOKEN_DUPLICATE =                                       0x2,
        TOKEN_IMPERSONATE =                                     0x4,
        TOKEN_QUERY =                                           0x8,
        TOKEN_QUERY_SOURCE =                                    0x10,
        TOKEN_ADJUST_PRIVILEGES =                               0x20,
        TOKEN_ADJUST_GROUPS =                                   0x40,
        TOKEN_ADJUST_DEFAULT =                                  0x80,
        TOKEN_ADJUST_SESSIONID =                                0x100,
        TOKEN_STANDARD_RIGHTS_READ =                            0x2000,
        TOKEN_STANDARD_RIGHTS_REQUIRED =                        0xF0000,
        TOKEN_READ = TOKEN_STANDARD_RIGHTS_REQUIRED 
            | TOKEN_QUERY,
        TOKEN_ALL_ACCESS = TOKEN_STANDARD_RIGHTS_REQUIRED
            | TOKEN_ASSIGN_PRIMARY
            | TOKEN_DUPLICATE
            | TOKEN_IMPERSONATE
            | TOKEN_QUERY
            | TOKEN_QUERY_SOURCE
            | TOKEN_ADJUST_PRIVILEGES
            | TOKEN_ADJUST_GROUPS
            | TOKEN_ADJUST_DEFAULT
            | TOKEN_ADJUST_SESSIONID
    }
}
