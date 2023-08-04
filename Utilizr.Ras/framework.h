#pragma once

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files
#include <windows.h>
#include "Raslib.h"


extern "C" {

	__declspec(dllexport) DWORD RaslibCreateIkevVpnDevice(LPCWSTR deviceName, LPCWSTR connectionHostname) {
		return CreateVpnDevice(deviceName, connectionHostname);
	}

	__declspec(dllexport) DWORD RaslibAbortIkevVpn() {
		return AbortDialAttempt();
	}

	__declspec(dllexport) DWORD RaslibResetAbortIkevVpn() {
		return ResetAbortDial();
	}

	__declspec(dllexport) DWORD RaslibConnectIkevVpn(
		LPCWSTR deviceName,
		LPCWSTR connectionHostname,
		LPCWSTR username,
		LPCWSTR password,
		DialDelegateFuncType completeCallback,
		DialErrorFuncType errorCallback,
		DialDelegateFuncType abortCallback
	) {
		return ConnectVpnDevice(deviceName, connectionHostname, username, password, completeCallback, errorCallback, abortCallback);
	}

	__declspec(dllexport) DWORD RaslibDisconnectIkevVpn(LPCWSTR deviceName) {
		return DisconnectVpnDevice(deviceName);
	}

	__declspec(dllexport) DWORD RaslibGetIkevVpnStatistics(LPCWSTR deviceName, VpnDeviceStats* returnStats) {
		return GetVpnDeviceStatistics(deviceName, returnStats);
	}

	__declspec(dllexport) DWORD RaslibSetLogCallback(PLogCallback logCallback)
	{
		return SetLogCallback(logCallback);
	}
}
