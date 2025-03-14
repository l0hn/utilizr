﻿using System;

namespace Utilizr.Win32.Kernel32.Flags
{
    [Flags]
    public enum ThreadAccess : int
    {
        TERMINATE =                     (0x0001),
        SUSPEND_RESUME =                (0x0002),
        GET_CONTEXT =                   (0x0008),
        SET_CONTEXT =                   (0x0010),
        SET_INFORMATION =               (0x0020),
        QUERY_INFORMATION =             (0x0040),
        SET_THREAD_TOKEN =              (0x0080),
        IMPERSONATE =                   (0x0100),
        DIRECT_IMPERSONATION =          (0x0200),
        THREAD_ALL_ACCESS =             (0x1F03FF),
    }
}
