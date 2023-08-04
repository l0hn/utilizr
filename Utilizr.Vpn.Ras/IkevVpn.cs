using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Utilizr.Logging;

namespace Utilizr.Vpn.Ras
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RaslibLogCallbackDelegate([MarshalAs(UnmanagedType.LPStr)] string message);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void CallbackDelegate();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ErrorCallbackDelegate(UInt32 error);

    public class IkevVpn : IDisposable
    {
        [DllImport("Utilizr.Ras.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint RaslibSetLogCallback(
            IntPtr logCallback
        );

        [DllImport("Utilizr.Ras.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint RaslibCreateIkevVpnDevice(
            [MarshalAs(UnmanagedType.LPWStr), In] string deviceName,
            [MarshalAs(UnmanagedType.LPWStr), In] string connectionHostname
        );

        [DllImport("Utilizr.Ras.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RaslibAbortIkevVpn();

        [DllImport("Utilizr.Ras.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RaslibResetAbortIkevVpn();

        [DllImport("Utilizr.Ras.dll", CallingConvention = CallingConvention.Cdecl), MethodImpl(MethodImplOptions.InternalCall)]
        public static extern uint RaslibConnectIkevVpn(
            [MarshalAs(UnmanagedType.LPWStr), In] string deviceName,
            [MarshalAs(UnmanagedType.LPWStr), In] string connectionHostname,
            [MarshalAs(UnmanagedType.LPWStr), In] string username,
            [MarshalAs(UnmanagedType.LPWStr), In] string password,
            IntPtr completeCallback,
            IntPtr errorCallback,
            IntPtr abortCallback
            );

        [DllImport("Utilizr.Ras.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint RaslibDisconnectIkevVpn([MarshalAs(UnmanagedType.LPWStr), In] string deviceName);

        [DllImport("Utilizr.Ras.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint RaslibGetIkevVpnStatistics([MarshalAs(UnmanagedType.LPWStr), In] string deviceName, IntPtr stats);


        private GCHandle _completeHandle;
        private GCHandle _errorHandle;
        private GCHandle _abortHandle;
        private GCHandle _logHandle;

        private CallbackDelegate _completeCallback;
        private ErrorCallbackDelegate _errorCallback;
        private CallbackDelegate _abortCallback;
        private RaslibLogCallbackDelegate _logCallback;

        public event CallbackDelegate DialComplete;
        public event CallbackDelegate DialAborted;
        public event ErrorCallbackDelegate DialError;

        public IkevVpn()
        {
            _completeCallback = new CallbackDelegate(CompleteCallback);
            _errorCallback = new ErrorCallbackDelegate(ErrorCallback);
            _abortCallback = new CallbackDelegate(AbortCallback);
            _logCallback = new RaslibLogCallbackDelegate(RasLibLogCallback);

            _completeHandle = GCHandle.Alloc(_completeCallback);
            _errorHandle = GCHandle.Alloc(_errorCallback);
            _abortHandle = GCHandle.Alloc(_abortCallback);
            _logHandle = GCHandle.Alloc(_logCallback);

            RaslibSetLogCallback(Marshal.GetFunctionPointerForDelegate(_logCallback));
        }

        private void RasLibLogCallback(string message)
        {
            try
            {
                Log.Info("ikev_vpn", message);
            }
            catch (Exception e)
            {
            }
        }

        public bool CreateDevice(string deviceName, string connectionHostname)
        {
            uint result = RaslibCreateIkevVpnDevice(deviceName, connectionHostname);
            if (result == 0)
            {
                return true;
            }
            else
            {
                throw new Win32Exception((int)result);
            }
        }

        public void Abort()
        {
            RaslibAbortIkevVpn();
        }

        public void ResetAbort()
        {
            RaslibResetAbortIkevVpn();
        }

        private void CompleteCallback()
        {
            OnDialComplete();
        }

        private void ErrorCallback(uint error)
        {
            OnDialError(error);
        }

        private void AbortCallback()
        {
            OnDialAborted();
        }

        public bool Connect(
            string deviceName,
            string connectionHostname,
            string username,
            string password
        )
        {
            uint result = RaslibConnectIkevVpn(
                deviceName,
                connectionHostname,
                username,
                password,
                Marshal.GetFunctionPointerForDelegate(_completeCallback),
                Marshal.GetFunctionPointerForDelegate(_errorCallback),
                Marshal.GetFunctionPointerForDelegate(_abortCallback)
            );

            if (result == 0)
            {
                return true;
            }
            else
            {
                throw new Win32Exception((int)result);
            }
        }

        public bool Disconnect(string deviceName)
        {
            uint result = RaslibDisconnectIkevVpn(deviceName);
            if (result == 0)
            {
                return true;
            }
            else
            {
                throw new Win32Exception((int)result);
            }
        }

        public IkevVpnStats GetStats(string deviceName)
        {
            IkevVpnStats stats;
            unsafe
            {
                IntPtr pStats = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IkevVpnStats)));
                uint result = RaslibGetIkevVpnStatistics(deviceName, pStats);

                if (result == 0)
                {
                    stats = (IkevVpnStats)Marshal.PtrToStructure(pStats, typeof(IkevVpnStats));
                }
                else
                {

                    Marshal.FreeHGlobal(pStats);
                    throw new Win32Exception((int)result);
                }
            }

            return stats;
        }

        protected virtual void OnDialComplete()
        {
            DialComplete?.Invoke();
        }

        protected virtual void OnDialError(uint error)
        {
            DialError?.Invoke(error);
        }

        protected virtual void OnDialAborted()
        {
            DialAborted?.Invoke();
        }

        public void Dispose()
        {
            _abortHandle.Free();
            _errorHandle.Free();
            _completeHandle.Free();
            _logHandle.Free();
        }
    }

    public enum IkevVpnStatsStatus
    {
        DISCONNECTED = 0,
        CONNECTED = 1
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct IkevVpnStats
    {
        public IkevVpnStatsStatus Status;
        public Int64 BytesTransmitted;
        public Int64 BytesReceived;
        public Int64 Bps;
        public Int64 ConnectDuration;
        [MarshalAs(UnmanagedType.LPWStr, SizeConst = 129)]
        public string Hostname;
    }
}
