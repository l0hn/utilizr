#pragma once
#include <Windows.h>
#include <stdio.h>
#define export __declspec(dllexport)

DEFINE_GUID(WFPKS_FILTER_GUID, 0x3b2c7dfd, 0x7dd9, 0x4193, 0xad, 0x0c, 0x6a, 0xf7, 0x49, 0xa1, 0x4a, 0xa5);

extern "C" {
	__declspec(dllexport) DWORD KillswitchEngage2(WFPKS_ADDR_AND_MASK* remoteAddresses, int addrCount, WFPKS_ADDR_AND_MASK* localAddresses, int localAddrCount, ULONG tapAdapterIndex, const wchar_t* ovpnBinaryPath, BOOL persistReboot, const wchar_t* displayName)
	{
		return WfpksEnable2(remoteAddresses, addrCount, localAddresses, localAddrCount, tapAdapterIndex, ovpnBinaryPath, persistReboot, displayName);
	}

	__declspec(dllexport) DWORD KillswitchDisengage() {
		return WfpksDisable();
	}

	__declspec(dllexport) BOOL KillswitchIsEngaged() {
		return WfpksIsEnabled();
	}

	__declspec(dllexport) DWORD CreateIkevVpnDevice(LPCWSTR deviceName, LPCWSTR connectionHostname) {
		return CreateVpnDevice(deviceName, connectionHostname);
	}

	__declspec(dllexport) DWORD AbortIkevVpn() {
		return AbortDialAttempt();
	}

	__declspec(dllexport) DWORD ResetAbortIkevVpn() {
		return ResetAbortDial();
	}

	__declspec(dllexport) DWORD ConnectIkevVpn(
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

	__declspec(dllexport) DWORD DisconnectIkevVpn(LPCWSTR deviceName) {
		return DisconnectVpnDevice(deviceName);
	}

	__declspec(dllexport) DWORD GetIkevVpnStatistics(LPCWSTR deviceName, VpnDeviceStats* returnStats) {
		return GetVpnDeviceStatistics(deviceName, returnStats);
	}

	__declspec(dllexport) DWORD RaslibSetLogCallback(PLogCallback logCallback)
	{
		return SetLogCallback(logCallback);
	}
}