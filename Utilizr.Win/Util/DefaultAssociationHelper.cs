using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace Utilizr.Win.Util
{
    public static class DefaultAssociationHelper
    {
        public static string? GetAssociatedExecutable(string association, ASSOCIATIONTYPE type)
        {
            var progId = GetProgId(association, type);
            if (progId == null)
                return null;

            return GetExecutableFromProgId(progId);
        }

        public static string? GetProgId(string association, ASSOCIATIONTYPE type)
        {
            var reg = new ApplicationAssociationRegistration() as IApplicationAssociationRegistration;
            if (reg == null)
                return null;

            reg.QueryCurrentDefault(
                association,
                type,
                ASSOCIATIONLEVEL.AL_EFFECTIVE,
                out string progId
            );

            return progId;
        }

        public static string? GetExecutableFromProgId(string progId)
        {
            using var key = Registry.ClassesRoot.OpenSubKey($@"{progId}\shell\open\command");
            if (key == null)
                return null;

            var command = key.GetValue(null)?.ToString();
            if (string.IsNullOrWhiteSpace(command))
                return null;

            // Extract the executable path
            if (command.StartsWith("\""))
            {
                int end = command.IndexOf('"', 1);
                return command.Substring(1, end - 1);
            }
            else
            {
                int end = command.IndexOf(' ');
                return end > 0 ? command.Substring(0, end) : command;
            }
        }
    }

    #region COM Interop

    [ComImport]
    [Guid("4e530b0a-e611-4c77-a3ac-9031d022281b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IApplicationAssociationRegistration
    {
        int QueryCurrentDefault(
            [MarshalAs(UnmanagedType.LPWStr)] string pszQuery,
            ASSOCIATIONTYPE at,
            ASSOCIATIONLEVEL al,
            [MarshalAs(UnmanagedType.LPWStr)] out string ppszAssociation
        );
    }

    public enum ASSOCIATIONTYPE
    {
        AT_FILEEXTENSION,
        AT_URLPROTOCOL,
        AT_STARTMENUCLIENT,
        AT_MIMETYPE
    }

    public enum ASSOCIATIONLEVEL
    {
        AL_MACHINE,
        AL_EFFECTIVE,
        AL_USER
    }

    [ComImport]
    [Guid("591209c7-767b-42b2-9fba-44ee4615f2c7")]
    class ApplicationAssociationRegistration
    {
    }

    #endregion
}