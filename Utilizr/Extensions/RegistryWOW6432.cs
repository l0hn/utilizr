using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Utilizr.Info;
using System.ComponentModel;
using System.Reflection;
using Utilizr.Logging;
using System.IO;

namespace Utilizr.Windows
{
	#if !MONO

    /// <summary>
    /// An extension class to allow a registry key to allow it to get the
    /// registry in the 32 bit (Wow6432Node) or 64 bit regular registry key
    /// </summary>
    public static class RegistryWOW6432
    {
        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        public static extern int RegOpenKeyEx(UIntPtr hKey, string subKey, int ulOptions, int samDesired, out IntPtr phkResult);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        public static extern int RegCreateKeyEx(
            UIntPtr hKey,
            string lpSubKey,
            int Reserved,
            string lpClass,
            RegOption dwOptions,
            RegSAM samDesired,
            UIntPtr lpSecurityAttributes,
            out IntPtr phkResult,
            out RegResult lpdwDisposition);

        [Flags]
        public enum RegOption
        {
            NonVolatile = 0x0,
            Volatile = 0x1,
            CreateLink = 0x2,
            BackupRestore = 0x4,
            OpenLink = 0x8
        }

        [Flags]
        public enum RegSAM
        {
            None = 0,

            /// <summary>
            /// Required to query the values of a registry key.
            /// </summary>
            QueryValue = 0x0001,

            /// <summary>
            /// Required to create, delete, or set a registry value.
            /// </summary>
            SetValue = 0x0002,

            /// <summary>
            /// Required to create a subkey of a registry key.
            /// </summary>
            CreateSubKey = 0x0004,

            /// <summary>
            /// Required to enumerate the subkeys of a registry key.
            /// </summary>
            EnumerateSubKeys = 0x0008,

            /// <summary>
            /// Required to request change notifications for a registry key or for subkeys of a registry key.
            /// </summary>
            Notify = 0x0010,

            /// <summary>
            /// Reserved for system use.
            /// </summary>
            CreateLink = 0x0020,

            /// <summary>
            /// Indicates that an application on 64-bit Windows should operate on the 32-bit registry view.
            /// This flag is ignored by 32-bit Windows. Not supported in Windows 2000.
            /// </summary>
            WOW64_32Key = 0x0200,

            /// <summary>
            /// Indicates that an application on 64-bit Windows should operate on the 64-bit registry view.
            /// This flag is ignored by 32-bit Windows. Not supported in Windows 2000.
            /// </summary>
            WOW64_64Key = 0x0100,

            /// <summary>
            /// Combines the Read, QueryValues, EnumerateSubKeys, and Notify values.
            /// </summary>
            Read = 0x00020019,

            /// <summary>
            /// Combines the Write, SetValue, and CreateSubKey access rights.
            /// </summary>
            Write = 0x00020006,

            /// <summary>
            /// Equivalent to Read.
            /// </summary>
            Execute = 0x00020019,

            /// <summary>
            /// Combines the QueryValues, SetValue, CreateSubKey, EnumerateSubKeys, Notify, and CreateLink access rights.
            /// </summary>
            AllAccess = 0x000f003f
        }

        public enum RegResult
        {
            /// <summary>
            /// The key did not exist and was created.
            /// </summary>
            CreatedNewKey = 0x00000001,

            /// <summary>
            /// The key existed and was simply opened without being changed.
            /// </summary>
            OpenedExistingKey = 0x00000002
        }

        public static UIntPtr HKEY_CLASSES_ROOT = new UIntPtr(0x80000000u);
        public static UIntPtr HKEY_CURRENT_USER = new UIntPtr(0x80000001u);
        public static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(unchecked(0x80000002u));
        public static UIntPtr HKEY_USERS = new UIntPtr(0x80000003u);
        public static UIntPtr HKEY_PERFORMANCE_DATA = new UIntPtr(0x80000004u);
        public static UIntPtr HKEY_CURRENT_CONFIG = new UIntPtr(0x80000005u);
        public static UIntPtr HKEY_DYN_DATA = new UIntPtr(0x80000006u);

        /// <summary>
        /// Open subkey, explicitly setting 32 or 64 bit view and read/write permissions.
        /// </summary>
        public static RegistryKey OpenSubKeyWow6432(this RegistryKey key, string subKeyName, bool writable, bool in32BitView)
        {
            var options = RegSAM.Read;

            if (writable)
                options |= RegSAM.Write;

            options |= in32BitView 
                ? RegSAM.WOW64_32Key 
                : RegSAM.WOW64_64Key;

            return OpenSubKeyWow6432(key, subKeyName, options);
        }

        /// <summary>
        /// Open subkey, but all desired access values will need to be set, including whether 32 or 64 bit view.
        /// </summary>
        public static RegistryKey OpenSubKeyWow6432(this RegistryKey key, string subKeyName, RegSAM options)
        {
#if NETCOREAPP

            // has builtin support for registry redirection, and avoids needing /unsafe compiler switch to remove build error
            var correctViewHive = GetRegHiveFromPath(key.Name, (options & RegSAM.WOW64_32Key) == RegSAM.WOW64_32Key);

            // key may not be only the hive, don't remove remaining path, e.g. HKCU\SID
            if (correctViewHive.Name.Length < key.Name.Length)
            {
                var pathHiveRemoved = key.Name.Substring(correctViewHive.Name.Length);
                subKeyName = Path.Combine(pathHiveRemoved, subKeyName).TrimStart('\\','/');
            }

            var subkey = correctViewHive.OpenSubKey(subKeyName, (options & RegSAM.Write) == RegSAM.Write);
            return subkey;
            
            #else

            UIntPtr keyHandle = GetRegistryKeyHandle(key);
            if (keyHandle == UIntPtr.Zero)
                return null;

            IntPtr subKeyHandle;
            int result = RegOpenKeyEx(keyHandle, subKeyName, 0, (int)options, out subKeyHandle);
            string fullRegKeyPath = Path.Combine(key.Name, subKeyName);

            if (result != 0)
            {
                bool in32BitView = (options & RegSAM.WOW64_32Key) == RegSAM.WOW64_32Key;

                var ex = new Exception(
                    $"{nameof(RegOpenKeyEx)} failed, returned {result} when trying to access {fullRegKeyPath} in {(in32BitView ? 32 : 64)} bit view.",
                    new Win32Exception(result)
                );

                Log.Exception(LoggingLevel.DEBUG, nameof(RegistryWOW6432), ex);
                return null;
            }

            return PointerToRegistryKey(subKeyHandle, options, fullRegKeyPath);
            #endif
        }

        /// <summary>
        /// Create subkey, explicitly setting 32 or 64 bit view and read/write permissions on the created subkey.
        /// </summary>
        public static RegistryKey CreateSubKeyWow6432(this RegistryKey key, string subKeyName, bool writable, bool in32BitView)
        {
            var options = RegSAM.QueryValue;

            if (writable)
                options |= RegSAM.Write;

            options |= in32BitView
                ? RegSAM.WOW64_32Key
                : RegSAM.WOW64_64Key;

            return CreateSubKeyWow6432(key, subKeyName, options);
        }


        /// <summary>
        /// Create subkey, but all desired access values will need to be set, including whether 32 or 64 bit view.
        /// </summary>
        public static RegistryKey CreateSubKeyWow6432(this RegistryKey key, string subKeyName, RegSAM options)
        {
#if NETCOREAPP
            // has builtin support for registry redirection, and avoids needing /unsafe compiler switch to remove build error

            var correctViewHive = GetRegHiveFromPath(key.Name, (options & RegSAM.WOW64_32Key) == RegSAM.WOW64_32Key);
            var subkey = correctViewHive.CreateSubKey(subKeyName, (options & RegSAM.Write) == RegSAM.Write);
            return subkey;

            #else
            UIntPtr keyHandle = GetRegistryKeyHandle(key);
            if (keyHandle == UIntPtr.Zero)
                return null;

            IntPtr subKeyHandle;
            RegResult tmp;

            int result = RegCreateKeyEx(
                keyHandle, 
                subKeyName, 
                0,
                null, 
                RegOption.NonVolatile,
                RegSAM.QueryValue | options, 
                UIntPtr.Zero,
                out subKeyHandle,
                out tmp
            );

            if (result != 0)
            {
                bool in32BitView = (options & RegSAM.WOW64_32Key) == RegSAM.WOW64_32Key;

                var ex = new Exception(
                    $"{nameof(RegCreateKeyEx)} failed, returned {result} when trying to create {subKeyName} on {key.Name} in {(in32BitView ? 32 : 64)} bit view.",
                    new Win32Exception(result)
                );

                Log.Exception(LoggingLevel.DEBUG, nameof(RegistryWOW6432), ex);
                return null;
            }

            return PointerToRegistryKey(subKeyHandle, options, Path.Combine(key.Name, subKeyName));
#endif
        }


        static UIntPtr GetRegHiveFromString(string path)
        {
            if (path.StartsWith(@"HKEY_CLASSES_ROOT", StringComparison.OrdinalIgnoreCase))
                return HKEY_CLASSES_ROOT;

            if (path.StartsWith(@"HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase))
                return HKEY_CURRENT_USER;

            if (path.StartsWith(@"HKEY_LOCAL_MACHINE", StringComparison.OrdinalIgnoreCase))
                return HKEY_LOCAL_MACHINE;

            if (path.StartsWith(@"HKEY_USERS", StringComparison.OrdinalIgnoreCase))
                return HKEY_USERS;

            if (path.StartsWith(@"HKEY_CURRENT_CONFIG", StringComparison.OrdinalIgnoreCase))
                return HKEY_CURRENT_CONFIG;

            if (path.StartsWith(@"HKEY_DYN_DATA", StringComparison.OrdinalIgnoreCase))
                return HKEY_DYN_DATA;

            if (path.StartsWith(@"HKEY_PERFORMANCE_DATA", StringComparison.OrdinalIgnoreCase))
                return HKEY_PERFORMANCE_DATA;

            return UIntPtr.Zero;
        }

#if NETCOREAPP
        static RegistryKey GetRegHiveFromPath(string path, bool is32Bit)
        {
            var hive = RegistryHive.LocalMachine;
            if (path.StartsWith(@"HKEY_CLASSES_ROOT", StringComparison.OrdinalIgnoreCase))
                hive = RegistryHive.ClassesRoot;
            else if (path.StartsWith(@"HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase))
                hive = RegistryHive.CurrentUser;
            else if (path.StartsWith(@"HKEY_USERS", StringComparison.OrdinalIgnoreCase))
                hive = RegistryHive.Users;
            else if (path.StartsWith(@"HKEY_CURRENT_CONFIG", StringComparison.OrdinalIgnoreCase))
                hive = RegistryHive.CurrentConfig;
            if (path.StartsWith(@"HKEY_PERFORMANCE_DATA", StringComparison.OrdinalIgnoreCase))
                hive = RegistryHive.PerformanceData;

            var view = is32Bit
                ? RegistryView.Registry32
                : RegistryView.Registry64;
            return RegistryKey.OpenBaseKey(hive, view);
        }
#endif

#if !NETCOREAPP
        static unsafe UIntPtr GetRegistryKeyHandle(RegistryKey key)
        {
            if (key == null)
                return UIntPtr.Zero;

            Type registryKeyType = typeof(RegistryKey);
            var fieldInfo = registryKeyType.GetField("hkey", BindingFlags.NonPublic | BindingFlags.Instance);

            SafeHandle handle = (SafeHandle)fieldInfo.GetValue(key);
            IntPtr h = handle.DangerousGetHandle();

            // Vitally important to return an UIntPtr. Normal keys should be fine, but all hives
            // defined between 0x80000000 and 0x80000006. Will overflow and can never open.

            return (UIntPtr)h.ToPointer();
        }
#endif


#if !NETCOREAPP
        static public RegistryKey PointerToRegistryKey(IntPtr hKey, RegSAM options, string fullRegistryPath)
        {
            // The irony is strong with this one. All this faff because we cannot pass in whether
            // we want a 32 bit of 64 bit view of the registry which is fixed in .NET 4.0+. Using 
            // reflection to create RegistryKey from the handle we get from using the Win32 API. Well,
            // the constructor is different for .NET 4.0+, it takes that enum that we don't have in
            // earlier frameworks which is the whole reason for this in the first place! Since .NET 4.0
            // always uses CLR v4, just checking Environment.Version to invoke ctors with different args.

            bool hasWriteableAccess = (options & RegSAM.Write) == RegSAM.Write;
            bool in32BitMode = (options & RegSAM.WOW64_32Key) == RegSAM.WOW64_32Key;
            bool invokeNet4Construtor = Environment.Version.Major >= 4;

            try
            {
                var safeRegHandleCtorModifiers = invokeNet4Construtor
                    ? BindingFlags.Instance | BindingFlags.Public // .NET 4.0+ is public
                    : BindingFlags.Instance | BindingFlags.NonPublic; // lower is internal
                var safeRegHandleType = Type.GetType("Microsoft.Win32.SafeHandles.SafeRegistryHandle");
                var safeRegHandleCtorTypes = new Type[] { typeof(IntPtr), typeof(bool) };
                var safeRegHandleCtorInfo = safeRegHandleType.GetConstructor(safeRegHandleCtorModifiers, null, safeRegHandleCtorTypes, null);
                var safeRegHandle = safeRegHandleCtorInfo.Invoke(new object[] { hKey, false });

                RegistryKey result = null;
                var registryKeyType = typeof(RegistryKey);
                var regKeyCtorModifiers = BindingFlags.Instance | BindingFlags.NonPublic;

                if (invokeNet4Construtor)
                {
                    //ctor signature: private RegistryKey(SafeRegistryHandle  hkey, bool writable, RegistryView view)
                    var registryViewEnumType = Type.GetType("Microsoft.Win32.RegistryView");
                    string enumValue = in32BitMode
                        ? "Registry32"
                        : "Registry64";
                    var registryView = Enum.Parse(registryViewEnumType, enumValue);

                    var registryKeyCtorTypesNet4 = new Type[] { safeRegHandleType, typeof(bool), registryViewEnumType };
                    var registryKeyCtorInfoNet4 = registryKeyType.GetConstructor(regKeyCtorModifiers, null, registryKeyCtorTypesNet4, null);
                    result = (RegistryKey)registryKeyCtorInfoNet4.Invoke(new object[] { safeRegHandle, hasWriteableAccess, registryView });
                }
                else
                {
                    // ctor signature: private RegistryKey(SafeRegistryHandle  hkey, bool writable)
                    var registryKeyCtorTypesNet3 = new Type[] { safeRegHandleType, typeof(bool) };
                    var registryKeyCtorInfoNet3 = registryKeyType.GetConstructor(regKeyCtorModifiers, null, registryKeyCtorTypesNet3, null);
                    result = (RegistryKey)registryKeyCtorInfoNet3.Invoke(new object[] { safeRegHandle, hasWriteableAccess });
                }

                // Same for .NET 3.0 and 4.0+
                // Really important to set keyName, will be used when returning key
                var keyNameField = registryKeyType.GetField("keyName", BindingFlags.NonPublic | BindingFlags.Instance);
                keyNameField.SetValue(result, fullRegistryPath);

                return result;
            }
            catch(Exception ex)
            {
                Log.Exception(ex, $"Failed to create RegistryKey using reflection, .NET 4.0+ Version = {invokeNet4Construtor}");
                throw;
            }
        }
#endif
    }
#endif
        }