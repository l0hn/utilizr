using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilizr.Win32.User32.Flags
{
    /// <summary>
    /// Specify one.
    /// </summary>
    public enum MonitorFromWindowFlags : uint
    {
        DEFAULT_TO_NULL =           0x00000000,
        DEFAULT_TO_PRIMARY =        0x00000001,
        DEFAULT_TO_NEAREST =        0x00000002,
    }
}
