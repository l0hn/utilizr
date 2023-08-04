#include "stdafx.h"
#include <Windows.h>
#include <Ras.h>
#include <RasError.h>
#include <stddef.h>
#include <strsafe.h>
#include <iostream>
#include "Raslib.h"
#pragma comment(lib, "Rasapi32.lib")

#define CELEMS(x) ((sizeof(x))/(sizeof(x[0])))



PLogCallback _logCallback = NULL;

void OutputTraceString(const char* lpszFormatString, ...)
{
	va_list argptr;
	va_start(argptr, lpszFormatString);
	if (_logCallback != NULL)
	{
		size_t sLen = vsnprintf(NULL, 0, lpszFormatString, argptr);
		char* logEntry = (char*)malloc((sLen + 1));
		vsnprintf(logEntry, sLen, lpszFormatString, argptr);
		_logCallback(const_cast<char*>(logEntry));
		free(logEntry);
	}
#ifndef _DEBUG
	va_end(argptr);
	return;
#endif
	printf(lpszFormatString, argptr);
	va_end(argptr);
}

DWORD SetLogCallback(PLogCallback logCallback)
{
	_logCallback = logCallback;
	_logCallback("raslib log callback added");
	return ERROR_SUCCESS;
}

DWORD CreateVpnDevice(LPCWSTR deviceName, LPCWSTR connectionHostname)
{
	DWORD rc;
	DWORD dwSize;
	DWORD result = ERROR_SUCCESS;
	DWORD dwNumEntries;
	LPRASDEVINFO lpRasDevInfo = NULL;
	RASENTRY* RasEntry = (RASENTRY*)HeapAlloc(GetProcessHeap(), HEAP_ZERO_MEMORY, sizeof(RASENTRY));

	// Call the method to get the size of memory needed to actually call it
	rc = RasGetEntryProperties(NULL, L"", NULL, &dwSize, NULL, NULL);

	// Expected error return code because we passed a null pointer
	if (rc == ERROR_BUFFER_TOO_SMALL)
	{
		// Resize to actual size needed to hold the information
		RasEntry = (RASENTRY*)HeapReAlloc(GetProcessHeap(), HEAP_ZERO_MEMORY, RasEntry, dwSize);
		// Set the size so the api knows how much memory / which version to use
		RasEntry->dwSize = dwSize;

		// Actually get the entry and hydrate RasEntry, if it doesn't exist populate it with default values
		rc = RasGetEntryProperties(NULL, L"", RasEntry, &dwSize, NULL, NULL);
	}


	if (rc != ERROR_SUCCESS)
	{
		OutputTraceString("RasGetEntryProperties returned error: 0x%.8X\n", rc);
		result = rc;
	}

	if (result == ERROR_SUCCESS)
	{
		// Validate the format of the connection entry name.
		rc = RasValidateEntryName(NULL, deviceName);
		if (rc == ERROR_INVALID_NAME)
		{
			OutputTraceString("RasValidateEntryName returned invalid name in CreateVpnDevice: 0x%.8X\n", rc);
			result = rc;
		}
	}

	if (rc == ERROR_SUCCESS || rc == ERROR_ALREADY_EXISTS)
	{
		// Get the default properties for the entry
		rc = RasGetEntryProperties(NULL, L"", RasEntry, &dwSize, NULL, NULL);
		if (rc != ERROR_SUCCESS)
		{
			OutputTraceString("RasGetEntryProperties failed in CreateVpnDevice: 0x%.8X\n", rc);
			result = rc;
		}
	}

	if (result == ERROR_SUCCESS)
	{
		//set hostname and vpn type
		StringCchCopy(RasEntry->szLocalPhoneNumber, CELEMS(RasEntry->szLocalPhoneNumber), connectionHostname);
		StringCchCopy(RasEntry->szDeviceType, CELEMS(RasEntry->szDeviceType), RASDT_Vpn);

		//set strategy to ikev only
		RasEntry->dwVpnStrategy = VS_Ikev2Only;

		dwSize = 0;

		// Call the method to get the size of memory needed to actually call it
		RasEnumDevices(NULL, &dwSize, &dwNumEntries);
		lpRasDevInfo = (LPRASDEVINFO)HeapAlloc(GetProcessHeap(), HEAP_ZERO_MEMORY, dwSize);

		if (lpRasDevInfo == NULL)
		{
			OutputTraceString("HeapAlloc failed in CreateVpnDevice (GetLastError = 0x%.8X)\n", GetLastError());
			result = rc;
		}
	}

	if (result == ERROR_SUCCESS)
	{
		lpRasDevInfo->dwSize = sizeof(RASDEVINFO);

		// Get all available device types as an enum
		rc = RasEnumDevices(lpRasDevInfo, &dwSize, &dwNumEntries);

		if (rc != ERROR_SUCCESS)
		{
			OutputTraceString("RasEnumDevices failed in CreateVpnDevice: 0x%.8X", rc);
			result = rc;
		}
	}

	if (result == ERROR_SUCCESS)
	{
		// Iterate in a separate variable so we can clear the memory correctly
		auto item = lpRasDevInfo;
		for (UINT i = 1; i < dwNumEntries; i++, item++)
		{
			// If it's an IKEv2 vpn device type, use it for the new entry
			if (lstrcmpi(item->szDeviceType, RASDT_Vpn) == 0 && wcsstr(item->szDeviceName, L"IKEv2") != 0)
			{
				StringCchCopy(RasEntry->szDeviceName, CELEMS(RasEntry->szDeviceName), item->szDeviceName);
				break;
			}
		}

		// Write the phonebook entry.
		rc = RasSetEntryProperties(NULL, deviceName, RasEntry, sizeof(RASENTRY), NULL, 0);
		if (rc != ERROR_SUCCESS)
		{
			OutputTraceString("RasSetEntryProperties failed in CreateVpnDevice: 0x%.8X\n", rc);
			result = rc;
		}
	}

	if (lpRasDevInfo)
	{
		HeapFree(GetProcessHeap(), 0, lpRasDevInfo);
	}
	
	if (RasEntry)
	{
		HeapFree(GetProcessHeap(), 0, RasEntry);
	}

	OutputTraceString("create vpn device result: 0x%.8X\n", result);

	return result;
}

BOOL AbortDial = false;
DialDelegateFuncType DialCompleteFunc = NULL;
DialDelegateFuncType DialAbortFunc = NULL;
DialErrorFuncType DialErrorFunc = NULL;

DWORD AbortDialAttempt()
{
	AbortDial = true;

	return 0;
}

DWORD ResetAbortDial()
{
	AbortDial = false;

	return 0;
}

void WINAPI RasDialFunc1(HRASCONN RasConn, UINT msg, RASCONNSTATE rasconnstate, DWORD error, DWORD extendedError)
{
	if (AbortDial)
	{
		RasHangUp(RasConn);
		AbortDial = false;
		if (DialAbortFunc != NULL)
		{
			DialAbortFunc();
		}
	}

	if (error != ERROR_SUCCESS)
	{
		RasHangUp(RasConn);
		if (DialErrorFunc != NULL)
		{
			DialErrorFunc(error);
		}
	}
	
	if (rasconnstate == RASCS_Connected)
	{
		if (DialCompleteFunc != NULL)
		{
			DialCompleteFunc();
		}
	}
}

DWORD ConnectVpnDevice(
	LPCWSTR deviceName,
	LPCWSTR connectionHostname,
	LPCWSTR username,
	LPCWSTR password,
	DialDelegateFuncType completeCallback,
	DialErrorFuncType errorCallback,
	DialDelegateFuncType abortCallback
)
{
	// Create / update a device to use and dial
	auto deviceCreated = CreateVpnDevice(deviceName, connectionHostname);
	if (deviceCreated != ERROR_SUCCESS)
	{
		return deviceCreated;
	}

	DialCompleteFunc = completeCallback;
	DialErrorFunc = errorCallback;
	DialAbortFunc = abortCallback;

	auto result = ERROR_SUCCESS;

	RASDIALPARAMS* DialParams = (RASDIALPARAMS*)HeapAlloc(GetProcessHeap(), HEAP_ZERO_MEMORY, sizeof(RASDIALPARAMS));
	HRASCONN RasConn = NULL;
	BOOL PasswordReturned = false;

	// Set the size so the api knows how much memory / which version to use
	DialParams->dwSize = sizeof(RASDIALPARAMS);

	// Set the entry name to be used to our new device
	StringCchCopy(DialParams->szEntryName, CELEMS(DialParams->szEntryName), deviceName);

	// Get the default dial params for our device
	result = RasGetEntryDialParams(NULL, DialParams, &PasswordReturned);

#if (WINVER >= 0x601)
	if (result == ERROR_INVALID_SIZE)
	{
		DialParams->dwSize = offsetof(RASDIALPARAMSW, dwIfIndex);
		result = RasGetEntryDialParams(NULL, DialParams, &PasswordReturned);
	}
#endif // (WINVER >= 0x601)

	if (result == ERROR_SUCCESS)
	{
		// Copy the credentials to the dial params
		StringCchCopy(DialParams->szUserName, CELEMS(DialParams->szUserName), username);
		StringCchCopy(DialParams->szPassword, CELEMS(DialParams->szPassword), password);

		// Dial the connection using our vpn device
		result = RasDial(NULL, NULL, DialParams, 1, RasDialFunc1, &RasConn);
		if (result != ERROR_SUCCESS)
		{
			RasHangUp(RasConn);
			OutputTraceString("RasDial failed in ConnectVpnDevice: 0x%.8X\n", result);
		}
	}
	else
	{
		OutputTraceString("RasGetEntryDialParams failed in ConnectVpnDevice: 0x%.8X\n", result);
	}

	HeapFree(GetProcessHeap(), 0, DialParams);

	return result;
}

DWORD DisconnectVpnDevice(LPCWSTR deviceName)
{
	DWORD dwSize = 0;
	DWORD rc = ERROR_SUCCESS;
	DWORD result = ERROR_SUCCESS;
	DWORD dwConnections = 0;
	LPRASCONN lpRasConn = NULL;

	// Call the method to get the size of memory needed to actually call it
	rc = RasEnumConnections(lpRasConn, &dwSize, &dwConnections);

	if (rc == ERROR_BUFFER_TOO_SMALL) 
	{
		// Allocate the memory needed for the array of RAS structure(s)
		lpRasConn = (LPRASCONN)HeapAlloc(GetProcessHeap(), HEAP_ZERO_MEMORY, dwSize);
		if (lpRasConn == NULL) 
		{
			OutputTraceString("HeapAlloc failed!\n");
			return 0;
		}
		// Set the size so the api knows how much memory / which version to use
		lpRasConn[0].dwSize = sizeof(RASCONN);

		// Call RasEnumConnections to enumerate active connections
		rc = RasEnumConnections(lpRasConn, &dwSize, &dwConnections);

		if (rc == ERROR_SUCCESS)
		{
			// Iterate in a separate variable so we can clear the memory correctly
			auto item = lpRasConn;
			for (UINT i = 0; i < dwConnections; i++, item++)
			{
				// Only hang up our own device
				if (lstrcmpi(item->szEntryName, deviceName) == 0)
				{
					// Try hanging up one time, each dial on an active connection requires an additional hangup
					rc = RasHangUp(item->hrasconn);

					// If we hung up correctly verify this by hanging up again expecting a non-zero response
					// Keep hanging up till we get a non-zero code or we give up
					auto attempts = 0;
					while (rc == 0 && attempts++ < 50)
					{
						rc = RasHangUp(item->hrasconn);
						Sleep(100);
					}

					if (rc == 0)
					{
						OutputTraceString("RasHangUp failed in DisconnectVpnDevice: 0x%.8X\n", rc);
						result = ERROR_HANGUP_FAILED;
					}
				}
			}
		}
		else
		{
			result = rc;
		}

		// Deallocate memory for the connection buffer
		HeapFree(GetProcessHeap(), 0, lpRasConn);
		lpRasConn = NULL;
	}
	else
	{
		result = rc;
	}

	return result;
}

DWORD GetVpnDeviceStatistics(LPCWSTR deviceName, VpnDeviceStats* returnStats)
{
	DWORD dwSize = 0;
	DWORD rc = ERROR_SUCCESS;
	DWORD result = ERROR_SUCCESS;
	DWORD dwConnections = 0;
	LPRASCONN lpRasConn = NULL;

	returnStats->Status = 0;
	returnStats->BytesTransmitted = 0;
	returnStats->BytesReceived = 0;
	returnStats->Bps = 0;
	returnStats->ConnectDuration = 0;
	returnStats->Hostname = NULL;

	RAS_STATS* stats = (RAS_STATS*)HeapAlloc(GetProcessHeap(), HEAP_ZERO_MEMORY, sizeof(RAS_STATS));
	stats->dwSize = sizeof(RAS_STATS);

	RASCONNSTATUS* status = (RASCONNSTATUS*)HeapAlloc(GetProcessHeap(), HEAP_ZERO_MEMORY, sizeof(RASCONNSTATUS));
	// Set the size so the api knows how much memory / which version to use
	status->dwSize = sizeof(RASCONNSTATUS);

	// Call the method to get the size of memory needed to actually call it
	rc = RasEnumConnections(lpRasConn, &dwSize, &dwConnections);

	if (rc == ERROR_BUFFER_TOO_SMALL) 
	{
		// Allocate the memory needed for the array of RAS structure(s)
		lpRasConn = (LPRASCONN)HeapAlloc(GetProcessHeap(), HEAP_ZERO_MEMORY, dwSize);
		if (lpRasConn == NULL) {
			OutputTraceString("HeapAlloc failed!\n");
			return 1;
		}
		// Set the size so the api knows how much memory / which version to use
		lpRasConn[0].dwSize = sizeof(RASCONN);

		// Call RasEnumConnections to enumerate active connections, might not return any if disconnected
		rc = RasEnumConnections(lpRasConn, &dwSize, &dwConnections);

		// Iterate in a separate variable so we can clear the memory correctly
		auto item = lpRasConn;
		for (UINT i = 0; i < dwConnections; i++, item++)
		{
			// Only check our own device
			if (lstrcmpi(item->szEntryName, deviceName) == 0)
			{
				// Get the connection status using the HRASCONN device connection
				rc = RasGetConnectStatus(item->hrasconn, status);
				if (rc != 0)
				{
					OutputTraceString("RasGetConnectStatus failed in VpnDeviceStats* GetVpnDeviceStatistics: 0x%.8X\n", rc);
					result = rc;
				}
				// If we get the status the connection is valid so get the stats
				if (rc == 0)
				{
					if (status->rasconnstate == RASCS_Connected)
					{
						returnStats->Status = 1;
					}

					returnStats->Hostname = status->szPhoneNumber;

					// Get the connection statistics using the HRASCONN device connection
					rc = RasGetConnectionStatistics(item->hrasconn, stats);
					if (rc != 0)
					{
						OutputTraceString("RasGetConnectionStatistics failed in VpnDeviceStats* GetVpnDeviceStatistics: 0x%.8X\n", rc);
						result = rc;
					}
					else
					{
						returnStats->BytesTransmitted = stats->dwBytesXmited;
						returnStats->BytesReceived = stats->dwBytesRcved;
						returnStats->Bps = stats->dwBps;
						returnStats->ConnectDuration = stats->dwConnectDuration;
					}
				}
			}
		}

		// Deallocate memory for the connection buffer
		HeapFree(GetProcessHeap(), 0, lpRasConn);
		lpRasConn = NULL;
	}

	return result;
}
