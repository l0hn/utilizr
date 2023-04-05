using System;
using System.Dynamic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Utilizr.Win
{
    // TODO: COMObject not needed for .net 5.0+, refactor all instances
    // Workaround until .NET 5.0 is out (should be fixed in that version)
    // A small wrapper around COM interop to make it more easy to use.
    // See https://github.com/dotnet/runtime/issues/12587#issuecomment-534611966
    public class COMObject : DynamicObject, IDisposable
    {
        public static COMObject CreateObject(string progID)
        {
            var progIDType = Type.GetTypeFromProgID(progID, true);
            if (progIDType == null)
                throw new ArgumentException(nameof(progIDType));

            return new COMObject(Activator.CreateInstance(progIDType));
        }

        public dynamic? Instance { get; private set; }

        public COMObject(object? instance)
        {
            Instance = instance;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder.Name == nameof(Instance))
            {
                result = Instance!;
                return true;
            }

            result = Instance!.GetType().InvokeMember(
                binder.Name,
                BindingFlags.GetProperty,
                Type.DefaultBinder,
                Instance,
                new object[] { }
            );
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
            Instance!.GetType().InvokeMember(
                binder.Name,
                BindingFlags.SetProperty,
                Type.DefaultBinder,
                Instance,
                new object?[] { WrapIfRequired(value) }
            );
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
        {
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] is COMObject co)
                        args[i] = co.Instance!;
                }
            }

            result = Instance!.GetType().InvokeMember(
                binder.Name,
                BindingFlags.InvokeMethod,
                Type.DefaultBinder,
                Instance,
                args
            );
            result = WrapIfRequired(result);
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
        {
            result = WrapIfRequired(
                Instance!.GetType()
                    .InvokeMember(
                        "Item",
                        BindingFlags.GetProperty,
                        Type.DefaultBinder,
                        Instance,
                        indexes
                    ));
            return true;
        }

        private static object? WrapIfRequired(object? obj)
        {
            return obj != null && obj.GetType().IsCOMObject
                ? new COMObject(obj)
                : obj;
        }

        public void Dispose()
        {
            // The RCW is a .NET object and cannot be released from the finalizer,
            // because it might not exist anymore.
            if (Instance != null)
            {
                Marshal.ReleaseComObject(Instance);
                Instance = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}