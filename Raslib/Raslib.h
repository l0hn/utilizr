#ifndef RASLIB_LIBRARY_H
#define RASLIB_LIBRARY_H
#include <Windows.h>
#endif

typedef struct _VpnDeviceStats
{
	INT     Status; // 0 = disconnected, 1 = connected
	INT64   BytesTransmitted;
	INT64   BytesReceived;
	INT64   Bps;
	INT64   ConnectDuration;
	LPWSTR  Hostname;

} VpnDeviceStats;

typedef void(_cdecl *DialDelegateFuncType)();
typedef void(_cdecl *DialErrorFuncType)(DWORD error);
typedef void(_cdecl* PLogCallback)(const char* message);

extern DWORD SetLogCallback(PLogCallback logCallback);
extern DWORD CreateVpnDevice(LPCWSTR deviceName, LPCWSTR connectionHostname);
extern DWORD AbortDialAttempt();
extern DWORD ResetAbortDial();
extern DWORD ConnectVpnDevice(
	LPCWSTR deviceName,
	LPCWSTR connectionHostname,
	LPCWSTR username,
	LPCWSTR password,
	DialDelegateFuncType completeCallback,
	DialErrorFuncType errorCallback,
	DialDelegateFuncType abortCallback
);
extern DWORD DisconnectVpnDevice(LPCWSTR deviceName);
extern DWORD GetVpnDeviceStatistics(LPCWSTR deviceName, VpnDeviceStats* returnStats);