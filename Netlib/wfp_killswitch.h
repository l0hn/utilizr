

#ifndef WFP_KILLSWITCH_LIBRARY_H
#define WFP_KILLSWITCH_LIBRARY_H
//#define _WIN32_WINNT 0x0600
#include <winsock2.h>


#include <windows.h>

typedef struct WFPKS_ADDR_AND_MASK_
{
	const char* szIpAddr;
	const char* szMask;
} WFPKS_ADDR_AND_MASK;

[[deprecated]]
DWORD WfpksEnable(ULONG networkAdapterIndex, UINT16 port, BOOL persistReboot);
DWORD WfpksEnable2(WFPKS_ADDR_AND_MASK* remoteAddresses, int addrCount, WFPKS_ADDR_AND_MASK* localAddresses, int localAddrCount, ULONG tapAdapterIndex, const wchar_t* ovpnBinaryPath, BOOL persistReboot, const wchar_t* displayName);
DWORD WfpksDisable();
BOOL WfpksIsEnabled();

#endif