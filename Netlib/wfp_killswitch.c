#define _WINSOCK_DEPRECATED_NO_WARNINGS
#include "wfp_killswitch.h"
#include <windows.h>
#include <guiddef.h>
#include <fwpmu.h>
#include <stdio.h>
#include <initguid.h>
#include <conio.h>
#include <ws2def.h>
#include <ws2ipdef.h>
#include <iphlpapi.h>
#include <WS2tcpip.h>
#if __MINGW
#include "wfpm_defines.h"
#endif





const GUID GUID_NULL = { 0, 0, 0, { 0, 0, 0, 0, 0, 0, 0, 0 } };

//openvpn filter (used by original WfpksEnable not by WfpksEnable2)
DEFINE_GUID(WFPKS_DEFAULT_FILTER_GUID, 0xcd69ac10, 0x275d, 0x43a0, 0xb3, 0x69, 0xa2, 0x50, 0xfb, 0x88, 0x67, 0x69);
#ifndef WFPKS_FILTER_GUID
#define WFPKS_FILTER_GUID WFPKS_DEFAULT_FILTER_GUID
#endif

//ikev filters
DEFINE_GUID(WFPKS_DEFAULT_BLOCKALL_FILTER_GUID, 0x68a634d6, 0xee7b, 0x43be, 0x85, 0x96, 0x7e, 0x66, 0x5b, 0x91, 0xe5, 0x50);
DEFINE_GUID(WFPKS_DEFAULT_ALLOW_IP_RANGE_FILTER_GUID, 0xb984250c, 0x303b, 0x4d45, 0xb3, 0x0a, 0x29, 0xcd, 0x72, 0x4a, 0x32, 0xeb);
DEFINE_GUID(WFPKS_DEFAULT_ALLOW_PORT_OUT_FILTER_GUID, 0x182cf284, 0xd352, 0x4642, 0x97, 0x77, 0x4a, 0xb1, 0xed, 0x63, 0x97, 0xe8);
DEFINE_GUID(WFPKS_DEFAULT_ALLOW_IP_FILTER_GUID, 0x4a662297, 0x0732, 0x4447, 0x9f, 0xdd, 0x97, 0x8e, 0x21, 0xbe, 0xa7, 0x1d);
DEFINE_GUID(WFPKS_DEFAULT_ALLOW_IP_LOCAL_FILTER_GUID, 0xc352c8f7, 0x1c3e, 0x457f, 0x99, 0x2c, 0xbd, 0x16, 0x02, 0x3b, 0xf6, 0xa4);
DEFINE_GUID(WFPKS_DEFAULT_SUBLAYER_GUID, 0x11466786, 0xe3fe, 0x4af2, 0x94, 0x44, 0xea, 0xe7, 0xb3, 0xf3, 0xcd, 0x25);

//ipv6 filters
DEFINE_GUID(WFPKS_DEFAULT_BLOCKALL_V6_FILTER_GUID, 0x42d15e5e, 0x9d38, 0x41ea, 0xa0, 0x43, 0x91, 0xcb, 0x25, 0x8a, 0x9f, 0x4e);
DEFINE_GUID(WFPKS_DEFAULT_ALLOW_V6_LINK_LOCAL_GUID, 0x45ae7951, 0x6cc7, 0x47e6, 0xae, 0xa2, 0x4b, 0x8d, 0xe1, 0xa6, 0x24, 0x36);
DEFINE_GUID(WFPKS_DEFAULT_ALLOW_V6_LOOPBACK_GUID, 0xc81d01d1, 0x8a99, 0x46d6, 0xad, 0xac, 0xa9, 0x2d, 0x2e, 0xff, 0x49, 0xda);
DEFINE_GUID(WFPKS_DEFAULT_ALLOW_V6_MULTICAST_GUID, 0x079c0fe3, 0x9137, 0x4820, 0xb8, 0x81, 0x53, 0x42, 0x96, 0xf7, 0x97, 0xbc);


#ifndef WFPKS_BLOCKAALL_FILTER_GUID
#define WFPKS_BLOCKALL_FILTER_GUID WFPKS_DEFAULT_BLOCKALL_FILTER_GUID
#define WFPKS_ALLOW_IP_RANGE_FILTER_GUID WFPKS_DEFAULT_ALLOW_IP_RANGE_FILTER_GUID
#define WFPKS_ALLOW_PORT_OUT_FILTER_GUID WFPKS_DEFAULT_ALLOW_PORT_OUT_FILTER_GUID
#define WFPKS_ALLOW_IP_FILTER_GUID WFPKS_DEFAULT_ALLOW_IP_FILTER_GUID
#define WFPKS_ALLOW_IP_LOCAL_FILTER_GUID WFPKS_DEFAULT_ALLOW_IP_LOCAL_FILTER_GUID
#define WFPKS_SUBLAYER_GUID WFPKS_DEFAULT_SUBLAYER_GUID

#define WFPKS_BLOCKALL_V6_FILTER_GUID WFPKS_DEFAULT_BLOCKALL_V6_FILTER_GUID
#define WFPKS_ALLOW_V6_LINK_LOCAL_GUID WFPKS_DEFAULT_ALLOW_V6_LINK_LOCAL_GUID
#define WFPKS_ALLOW_V6_LOOPBACK_GUID WFPKS_DEFAULT_ALLOW_V6_LOOPBACK_GUID
#define WFPKS_ALLOW_V6_MULTICAST_GUID WFPKS_DEFAULT_ALLOW_V6_MULTICAST_GUID
#endif



void debugPrint(const char* fmt, ...) {
#ifndef _DEBUG
	return;
#endif
	va_list argptr;
	va_start(argptr, fmt);
	printf(fmt, argptr);
	va_end(argptr);
}

BOOL WfpksIsEnabled() {
	FWPM_FILTER0* fwpmFilter = NULL;
	HANDLE engineHandle = NULL;
	DWORD result = ERROR_SUCCESS;

	debugPrint("opening engine\n");
	result = FwpmEngineOpen0(NULL, RPC_C_AUTHN_DEFAULT, NULL, NULL, &engineHandle);

	if (result == ERROR_SUCCESS) {
		debugPrint("getting filter\n");
		result = FwpmFilterGetByKey0(engineHandle, &WFPKS_FILTER_GUID, &fwpmFilter);

		if (result == FWP_E_FILTER_NOT_FOUND)
		{
			debugPrint("getting ikev filter\n");
			result = FwpmFilterGetByKey0(engineHandle, &WFPKS_BLOCKALL_FILTER_GUID, &fwpmFilter);
		}
	}

	if (engineHandle != NULL) {
		debugPrint("closing engine\n");
		FwpmEngineClose0(engineHandle);
	}

	if (fwpmFilter != NULL) {
		debugPrint("freeing filter\n");
		FwpmFreeMemory0((void**)&fwpmFilter);
	}

	if (result == ERROR_SUCCESS) {
		return TRUE;
	}
	return FALSE;
}

DWORD WfpksEnable(ULONG networkAdapterIndex, UINT16 port, BOOL persistReboot) {
	WfpksDisable();

	DWORD result = ERROR_SUCCESS;
	HANDLE engineHandle = NULL;
	FWPM_FILTER0* fwpmFilter = NULL;
	FWPM_FILTER_CONDITION0 fwpmFilterConditions[10];
	NET_LUID networkAdapterLuid;
	UINT64 filterId;
	FWPM_ACTION0 action;
	UINT32 numFilterConditions = 0;

	fwpmFilter = (FWPM_FILTER0*)calloc(1, sizeof(FWPM_FILTER0));

	//get the luid
	if (networkAdapterIndex != 999999) {
		result = ConvertInterfaceIndexToLuid(networkAdapterIndex, &networkAdapterLuid);
	}

	//create the conditions
	if (result == ERROR_SUCCESS) {
		//set the filter properties
		fwpmFilter->filterKey = WFPKS_FILTER_GUID;
		fwpmFilter->displayData.name = const_cast<wchar_t*>(L"WFP KILLSWITCH");
		fwpmFilter->displayData.description = const_cast<wchar_t*>(L"Prevents IP leaks when unexpectedly disconnected from OpenVPN");

		action.type = FWP_ACTION_BLOCK;
		action.filterType = WFPKS_FILTER_GUID;
		fwpmFilter->action = action;

		if (persistReboot == TRUE) {
			fwpmFilter->flags |= FWPM_FILTER_FLAG_PERSISTENT;
		}

		fwpmFilter->layerKey = FWPM_LAYER_OUTBOUND_TRANSPORT_V4;
		fwpmFilter->subLayerKey = IID_NULL;

		//always allow loopback adapter
		fwpmFilterConditions[numFilterConditions].fieldKey = FWPM_CONDITION_FLAGS;
		fwpmFilterConditions[numFilterConditions].matchType = FWP_MATCH_FLAGS_NONE_SET;
		fwpmFilterConditions[numFilterConditions].conditionValue.type = FWP_UINT32;
		fwpmFilterConditions[numFilterConditions].conditionValue.uint32 = FWP_CONDITION_FLAG_IS_LOOPBACK;
		numFilterConditions++;

		//always allow the tap interface
		if (networkAdapterIndex != 999999) {
			fwpmFilterConditions[numFilterConditions].fieldKey = FWPM_CONDITION_IP_LOCAL_INTERFACE;
			fwpmFilterConditions[numFilterConditions].matchType = FWP_MATCH_NOT_EQUAL;
			fwpmFilterConditions[numFilterConditions].conditionValue.type = FWP_UINT64;
			fwpmFilterConditions[numFilterConditions].conditionValue.uint64 = (UINT64*)&networkAdapterLuid;
			numFilterConditions++;
		}

		// always allow the openvpn port
		if (port > 0)
		{
			fwpmFilterConditions[numFilterConditions].fieldKey = FWPM_CONDITION_IP_REMOTE_PORT;
			fwpmFilterConditions[numFilterConditions].matchType = FWP_MATCH_NOT_EQUAL;
			fwpmFilterConditions[numFilterConditions].conditionValue.type = FWP_UINT16;
			fwpmFilterConditions[numFilterConditions].conditionValue.uint16 = port;
			numFilterConditions++;
		}

		fwpmFilter->numFilterConditions = numFilterConditions;
		fwpmFilter->filterCondition = fwpmFilterConditions;
	}

	if (result == ERROR_SUCCESS) {
		result = FwpmEngineOpen0(NULL, RPC_C_AUTHN_DEFAULT, NULL, NULL, &engineHandle);
	}

	if (result == ERROR_SUCCESS) {
		result = FwpmFilterAdd0(engineHandle, fwpmFilter, NULL, &filterId);
	}

	//cleanup
	if (engineHandle != NULL) {
		FwpmEngineClose0(engineHandle);
	}

	free(fwpmFilter);

	if (result == ERROR_SUCCESS)
	{
		debugPrint("successfully added filter\n");
	}

	return result;
}

DWORD WfpksEnable2(WFPKS_ADDR_AND_MASK* remoteAddresses, int addrCount, WFPKS_ADDR_AND_MASK* localAddresses, int localAddrCount, ULONG tapAdapterIndex, const wchar_t* ovpnBinaryPath, BOOL persistReboot, const wchar_t* displayName)
{
	WfpksDisable();
	DWORD result = ERROR_SUCCESS;
	HANDLE engineHandle = NULL;
	FWPM_FILTER0* fwpmBlockAllFilter = NULL;
	FWPM_FILTER0* fwpmIpRangeFilter = NULL;
	FWPM_FILTER0* fwpmPortOutFilter = NULL;
	FWPM_FILTER0* fwpmIpAddrFilter = NULL;
	FWPM_FILTER0* fwpmIpAddrLocalFilter = NULL;

	FWPM_FILTER0* fwpmBlockAllV6Filter = NULL;
	FWPM_FILTER0* fwpmAllowMulticastV6 = NULL;
	FWPM_FILTER0* fwpmAllowLinkLocalV6 = NULL;
	FWPM_FILTER0* fwpmAllowLoopbackV6 = NULL;

	FWPM_FILTER_CONDITION0 blockAllConditions[999];
	FWPM_FILTER_CONDITION0 allowIpRangeConditions[999];
	FWPM_FILTER_CONDITION0 allowPortOutConditions[999];
	FWPM_FILTER_CONDITION0* allowIpConditions = NULL;
	FWPM_FILTER_CONDITION0 allowIpLocalConditions[999];

	FWPM_FILTER_CONDITION0 allowLinkLocalConditionsV6[999];
	FWPM_FILTER_CONDITION0 allowMulticastConditionsV6[999];
	FWPM_FILTER_CONDITION0 allowLoopbackConditionsV6[999];
	FWPM_FILTER_CONDITION0 blockAllConditionsV6[999];

	UINT32 numBlockConditions = 0;
	UINT32 numIpRangeConditions = 0;
	UINT32 numPortOutConditions = 0;
	UINT32 numIpConditions = 0;
	UINT32 numIpLocalConditions = 0;

	UINT32 numLinkLocalConditionsV6 = 0;
	UINT32 numMulticastConditionsV6 = 0;
	UINT32 numLoopbackConditionsV6 = 0;
	UINT32 numBlockConditionsV6 = 0;

	NET_LUID adapterLuid;
	UINT64 filterId;
	FWP_V4_ADDR_AND_MASK* customIpsAndMasks = NULL;
	FWP_V4_ADDR_AND_MASK* localAddrAndMasks = NULL;

	FWP_V6_ADDR_AND_MASK* loopbackv6IpAndMask = NULL;

	FWP_BYTE_BLOB* ovpnBlob = NULL;


	//IPV4
	//create the main block layer to block all
	FWPM_ACTION0 blockAction;
	blockAction.type = FWP_ACTION_BLOCK;
	blockAction.filterType = WFPKS_BLOCKALL_FILTER_GUID;

	fwpmBlockAllFilter = (FWPM_FILTER0*)calloc(1, sizeof(FWPM_FILTER0));

	UINT64 blockWeight = 0;

	fwpmBlockAllFilter->action = blockAction;
	fwpmBlockAllFilter->filterKey = WFPKS_BLOCKALL_FILTER_GUID;
	fwpmBlockAllFilter->subLayerKey = WFPKS_SUBLAYER_GUID;
	fwpmBlockAllFilter->layerKey = FWPM_LAYER_ALE_AUTH_CONNECT_V4;
	fwpmBlockAllFilter->weight.type = FWP_UINT64;
	fwpmBlockAllFilter->weight.uint64 = &blockWeight;
	fwpmBlockAllFilter->displayData.name = const_cast<wchar_t*>(displayName);
	fwpmBlockAllFilter->displayData.description = const_cast<wchar_t*>(L"Prevents IP leaks when unexpectedly disconnected");

	if (persistReboot)
	{
		fwpmBlockAllFilter->flags |= FWPM_FILTER_FLAG_PERSISTENT;
	}

	blockAllConditions[numBlockConditions].fieldKey = FWPM_CONDITION_FLAGS;
	blockAllConditions[numBlockConditions].matchType = FWP_MATCH_FLAGS_NONE_SET;
	blockAllConditions[numBlockConditions].conditionValue.type = FWP_UINT32;
	blockAllConditions[numBlockConditions].conditionValue.uint32 = FWP_CONDITION_FLAG_IS_LOOPBACK;
	numBlockConditions++;

	//always allow the tap interface
	if (tapAdapterIndex > 0 && tapAdapterIndex < 999999)
	{
		result = ConvertInterfaceIndexToLuid(tapAdapterIndex, &adapterLuid);
		if (result == ERROR_SUCCESS) {
			blockAllConditions[numBlockConditions].fieldKey = FWPM_CONDITION_IP_LOCAL_INTERFACE;
			blockAllConditions[numBlockConditions].matchType = FWP_MATCH_NOT_EQUAL;
			blockAllConditions[numBlockConditions].conditionValue.type = FWP_UINT64;
			blockAllConditions[numBlockConditions].conditionValue.uint64 = (UINT64*)&adapterLuid;
			numBlockConditions++;
		}
	}

	//allow the ovpn binary
	if (wcslen(ovpnBinaryPath) > 0)
	{
		DWORD appIdResult = FwpmGetAppIdFromFileName0(ovpnBinaryPath, &ovpnBlob);
		if (appIdResult == ERROR_SUCCESS)
		{
			blockAllConditions[numBlockConditions].fieldKey = FWPM_CONDITION_ALE_APP_ID;
			blockAllConditions[numBlockConditions].matchType = FWP_MATCH_NOT_EQUAL;
			blockAllConditions[numBlockConditions].conditionValue.type = FWP_BYTE_BLOB_TYPE;
			blockAllConditions[numBlockConditions].conditionValue.byteBlob = ovpnBlob;
			numBlockConditions++;
		}
	}

	fwpmBlockAllFilter->numFilterConditions = numBlockConditions;
	fwpmBlockAllFilter->filterCondition = blockAllConditions;

	//inbound ports
	fwpmIpRangeFilter = (FWPM_FILTER0*)calloc(1, sizeof(FWPM_FILTER0));

	fwpmIpRangeFilter->action.type = FWP_ACTION_PERMIT;
	fwpmIpRangeFilter->action.filterType = WFPKS_ALLOW_IP_RANGE_FILTER_GUID;
	fwpmIpRangeFilter->filterKey = WFPKS_ALLOW_IP_RANGE_FILTER_GUID;
	fwpmIpRangeFilter->layerKey = FWPM_LAYER_ALE_AUTH_CONNECT_V4;
	fwpmIpRangeFilter->subLayerKey = WFPKS_SUBLAYER_GUID;
	fwpmIpRangeFilter->weight.type = FWP_UINT8;
	fwpmIpRangeFilter->weight.uint8 = 2;
	fwpmIpRangeFilter->displayData.name = const_cast<wchar_t*>(displayName);
	if (persistReboot)
	{
		fwpmIpRangeFilter->flags |= FWPM_FILTER_FLAG_PERSISTENT;
	}

	FWP_RANGE0 range1;
	range1.valueLow.type = FWP_UINT32;
	range1.valueLow.uint32 = htonl(inet_addr("224.0.0.0"));
	range1.valueHigh.type = FWP_UINT32;
	range1.valueHigh.uint32 = htonl(inet_addr("239.255.255.255"));

	allowIpRangeConditions[numIpRangeConditions].fieldKey = FWPM_CONDITION_IP_REMOTE_ADDRESS;
	allowIpRangeConditions[numIpRangeConditions].matchType = FWP_MATCH_RANGE;
	allowIpRangeConditions[numIpRangeConditions].conditionValue.type = FWP_RANGE_TYPE;
	allowIpRangeConditions[numIpRangeConditions].conditionValue.rangeValue = &range1;
	numIpRangeConditions++;

	fwpmIpRangeFilter->filterCondition = allowIpRangeConditions;
	fwpmIpRangeFilter->numFilterConditions = numIpRangeConditions;

	//outbound ports
	fwpmPortOutFilter = (FWPM_FILTER0*)calloc(1, sizeof(FWPM_FILTER0));

	FWPM_ACTION0 allowOutPortAction;
	allowOutPortAction.type = FWP_ACTION_PERMIT;
	allowOutPortAction.filterType = WFPKS_ALLOW_PORT_OUT_FILTER_GUID;

	fwpmPortOutFilter->action = allowOutPortAction;
	fwpmPortOutFilter->filterKey = WFPKS_ALLOW_PORT_OUT_FILTER_GUID;
	fwpmPortOutFilter->layerKey = FWPM_LAYER_ALE_AUTH_CONNECT_V4;
	fwpmPortOutFilter->subLayerKey = WFPKS_SUBLAYER_GUID;
	fwpmPortOutFilter->weight.type = FWP_UINT8;
	fwpmPortOutFilter->weight.uint8 = 3;
	fwpmPortOutFilter->displayData.name = const_cast<wchar_t*>(displayName);
	if (persistReboot)
	{
		fwpmPortOutFilter->flags |= FWPM_FILTER_FLAG_PERSISTENT;
	}

	//ikev ports
	UINT16 ports[] = { 67, 68, 500, 4500, 1900, 5350, 5351, 5353 };

	for (UINT16 port : ports)
	{
		allowPortOutConditions[numPortOutConditions].fieldKey = FWPM_CONDITION_IP_REMOTE_PORT;
		allowPortOutConditions[numPortOutConditions].matchType = FWP_MATCH_EQUAL;
		allowPortOutConditions[numPortOutConditions].conditionValue.type = FWP_UINT16;
		allowPortOutConditions[numPortOutConditions].conditionValue.uint16 = port;
		numPortOutConditions++;
	}

	fwpmPortOutFilter->filterCondition = allowPortOutConditions;
	fwpmPortOutFilter->numFilterConditions = numPortOutConditions;

	//create a sublayer to allow the remote ip
	fwpmIpAddrFilter = (FWPM_FILTER0*)calloc(1, sizeof(FWPM_FILTER0));

	FWPM_ACTION0 allowIpAction;
	allowIpAction.type = FWP_ACTION_PERMIT;
	allowIpAction.filterType = WFPKS_ALLOW_IP_FILTER_GUID;

	fwpmIpAddrFilter->action = allowIpAction;
	fwpmIpAddrFilter->filterKey = WFPKS_ALLOW_IP_FILTER_GUID;
	fwpmIpAddrFilter->layerKey = FWPM_LAYER_ALE_AUTH_CONNECT_V4;
	fwpmIpAddrFilter->subLayerKey = WFPKS_SUBLAYER_GUID;
	fwpmIpAddrFilter->weight.type = FWP_UINT8;
	fwpmIpAddrFilter->weight.uint8 = 3;
	fwpmIpAddrFilter->displayData.name = const_cast<wchar_t*>(displayName);

	if (persistReboot)
	{
		fwpmIpAddrFilter->flags |= FWPM_FILTER_FLAG_PERSISTENT;
	}

	customIpsAndMasks = (FWP_V4_ADDR_AND_MASK*)calloc(addrCount, sizeof(FWP_V4_ADDR_AND_MASK));
	allowIpConditions = (FWPM_FILTER_CONDITION0*)calloc(addrCount, sizeof(FWPM_FILTER_CONDITION0));

	for (int i = 0; i < addrCount; i++)
	{
		customIpsAndMasks[i].addr = htonl(inet_addr(remoteAddresses[i].szIpAddr));
		customIpsAndMasks[i].mask = htonl(inet_addr(remoteAddresses[i].szMask));

		allowIpConditions[numIpConditions].fieldKey = FWPM_CONDITION_IP_REMOTE_ADDRESS;
		allowIpConditions[numIpConditions].matchType = FWP_MATCH_EQUAL;
		allowIpConditions[numIpConditions].conditionValue.type = FWP_V4_ADDR_MASK;
		allowIpConditions[numIpConditions].conditionValue.v4AddrMask = &customIpsAndMasks[i];
		numIpConditions++;
	}

	fwpmIpAddrFilter->numFilterConditions = numIpConditions;
	fwpmIpAddrFilter->filterCondition = allowIpConditions;

	//local ip addresses
	fwpmIpAddrLocalFilter = (FWPM_FILTER0*)calloc(1, sizeof(FWPM_FILTER0));

	fwpmIpAddrLocalFilter->action.type = FWP_ACTION_PERMIT;
	fwpmIpAddrLocalFilter->action.filterType = WFPKS_ALLOW_IP_LOCAL_FILTER_GUID;
	fwpmIpAddrLocalFilter->filterKey = WFPKS_ALLOW_IP_LOCAL_FILTER_GUID;
	fwpmIpAddrLocalFilter->layerKey = FWPM_LAYER_ALE_AUTH_CONNECT_V4;
	fwpmIpAddrLocalFilter->subLayerKey = WFPKS_SUBLAYER_GUID;
	fwpmIpAddrLocalFilter->weight.type = FWP_UINT8;
	fwpmIpAddrLocalFilter->weight.uint8 = 4;
	fwpmIpAddrLocalFilter->displayData.name = const_cast<wchar_t*>(displayName);
	if (persistReboot)
	{
		fwpmIpAddrLocalFilter->flags |= FWPM_FILTER_FLAG_PERSISTENT;
	}


	localAddrAndMasks = (FWP_V4_ADDR_AND_MASK*)calloc(localAddrCount, sizeof(FWP_V4_ADDR_AND_MASK));

	for (size_t i = 0; i < localAddrCount; i++)
	{
		localAddrAndMasks[i].addr = htonl(inet_addr(localAddresses[i].szIpAddr));
		localAddrAndMasks[i].mask = htonl(inet_addr(localAddresses[i].szMask));

		allowIpLocalConditions[numIpLocalConditions].fieldKey = FWPM_CONDITION_IP_LOCAL_ADDRESS;
		allowIpLocalConditions[numIpLocalConditions].matchType = FWP_MATCH_EQUAL;
		allowIpLocalConditions[numIpLocalConditions].conditionValue.type = FWP_V4_ADDR_MASK;
		allowIpLocalConditions[numIpLocalConditions].conditionValue.v4AddrMask = &localAddrAndMasks[i];
		numIpLocalConditions++;
	}

	fwpmIpAddrLocalFilter->numFilterConditions = numIpLocalConditions;
	fwpmIpAddrLocalFilter->filterCondition = allowIpLocalConditions;
	//end ipv4 rules

	//ipv6 rules
	//block all V6
	FWPM_ACTION0 blockActionV6;
	blockActionV6.type = FWP_ACTION_BLOCK;
	blockActionV6.filterType = WFPKS_BLOCKALL_V6_FILTER_GUID;

	fwpmBlockAllV6Filter = (FWPM_FILTER0*)calloc(1, sizeof(FWPM_FILTER0));
	UINT64 blockAllV6Weight = 0;

	fwpmBlockAllV6Filter->action = blockActionV6;
	fwpmBlockAllV6Filter->filterKey = WFPKS_BLOCKALL_V6_FILTER_GUID;
	fwpmBlockAllV6Filter->subLayerKey = WFPKS_SUBLAYER_GUID;
	fwpmBlockAllV6Filter->layerKey = FWPM_LAYER_ALE_AUTH_CONNECT_V6;
	fwpmBlockAllV6Filter->weight.type = FWP_UINT64;
	fwpmBlockAllV6Filter->weight.uint64 = &blockAllV6Weight;
	fwpmBlockAllV6Filter->displayData.name = const_cast<wchar_t*>(displayName);
	fwpmBlockAllV6Filter->displayData.description = const_cast<wchar_t*>(L"Prevents IP leaks when unexpectedly disconnected");

	if (persistReboot)
	{
		fwpmBlockAllV6Filter->flags |= FWPM_FILTER_FLAG_PERSISTENT;
	}

	blockAllConditionsV6[numBlockConditionsV6].fieldKey = FWPM_CONDITION_FLAGS;
	blockAllConditionsV6[numBlockConditionsV6].matchType = FWP_MATCH_FLAGS_NONE_SET;
	blockAllConditionsV6[numBlockConditionsV6].conditionValue.type = FWP_UINT32;
	blockAllConditionsV6[numBlockConditionsV6].conditionValue.uint32 = FWP_CONDITION_FLAG_IS_LOOPBACK;
	numBlockConditionsV6++;

	//always allow the tap interface
	if (tapAdapterIndex > 0 && tapAdapterIndex < 999999)
	{
		result = ConvertInterfaceIndexToLuid(tapAdapterIndex, &adapterLuid);
		if (result == ERROR_SUCCESS) {
			blockAllConditionsV6[numBlockConditionsV6].fieldKey = FWPM_CONDITION_IP_LOCAL_INTERFACE;
			blockAllConditionsV6[numBlockConditionsV6].matchType = FWP_MATCH_NOT_EQUAL;
			blockAllConditionsV6[numBlockConditionsV6].conditionValue.type = FWP_UINT64;
			blockAllConditionsV6[numBlockConditionsV6].conditionValue.uint64 = (UINT64*)&adapterLuid;
			numBlockConditionsV6++;
		}
	}

	fwpmBlockAllV6Filter->numFilterConditions = numBlockConditionsV6;
	fwpmBlockAllV6Filter->filterCondition = blockAllConditionsV6;
	//end block all V6

	//allow loopback v6
	fwpmAllowLoopbackV6 = (FWPM_FILTER0*)calloc(1, sizeof(FWPM_FILTER0));

	fwpmAllowLoopbackV6->action.type = FWP_ACTION_PERMIT;
	fwpmAllowLoopbackV6->action.filterType = WFPKS_ALLOW_V6_LOOPBACK_GUID;
	fwpmAllowLoopbackV6->filterKey = WFPKS_ALLOW_V6_LOOPBACK_GUID;
	fwpmAllowLoopbackV6->layerKey = FWPM_LAYER_ALE_AUTH_CONNECT_V6;
	fwpmAllowLoopbackV6->subLayerKey = WFPKS_SUBLAYER_GUID;
	fwpmAllowLoopbackV6->weight.type = FWP_UINT8;
	fwpmAllowLoopbackV6->weight.uint8 = 4;
	fwpmAllowLoopbackV6->displayData.name = const_cast<wchar_t*>(displayName);
	if (persistReboot)
	{
		fwpmAllowLoopbackV6->flags |= FWPM_FILTER_FLAG_PERSISTENT;
	}

	loopbackv6IpAndMask = (FWP_V6_ADDR_AND_MASK*)calloc(1, sizeof(FWP_V6_ADDR_AND_MASK));

	inet_pton(AF_INET6, const_cast<char*>("::1"), &loopbackv6IpAndMask->addr);
	loopbackv6IpAndMask->prefixLength = 128;

	allowLoopbackConditionsV6[numLoopbackConditionsV6].fieldKey = FWPM_CONDITION_IP_REMOTE_ADDRESS;
	allowLoopbackConditionsV6[numLoopbackConditionsV6].matchType = FWP_MATCH_EQUAL;
	allowLoopbackConditionsV6[numLoopbackConditionsV6].conditionValue.type = FWP_V6_ADDR_MASK;
	allowLoopbackConditionsV6[numLoopbackConditionsV6].conditionValue.v6AddrMask = loopbackv6IpAndMask;
	numLoopbackConditionsV6++;

	fwpmAllowLoopbackV6->numFilterConditions = numLoopbackConditionsV6;
	fwpmAllowLoopbackV6->filterCondition = allowLoopbackConditionsV6;

	//end allow loopback v6

	//allow multicast v6
	fwpmAllowMulticastV6 = (FWPM_FILTER0*)calloc(1, sizeof(FWPM_FILTER0));

	fwpmAllowMulticastV6->action.type = FWP_ACTION_PERMIT;
	fwpmAllowMulticastV6->action.filterType = WFPKS_ALLOW_V6_MULTICAST_GUID;
	fwpmAllowMulticastV6->filterKey = WFPKS_ALLOW_V6_MULTICAST_GUID;
	fwpmAllowMulticastV6->layerKey = FWPM_LAYER_ALE_AUTH_CONNECT_V6;
	fwpmAllowMulticastV6->subLayerKey = WFPKS_SUBLAYER_GUID;
	fwpmAllowMulticastV6->weight.type = FWP_UINT8;
	fwpmAllowMulticastV6->weight.uint8 = 4;
	fwpmAllowMulticastV6->displayData.name = const_cast<wchar_t*>(displayName);
	if (persistReboot)
	{
		fwpmAllowMulticastV6->flags |= FWPM_FILTER_FLAG_PERSISTENT;
	}

	FWP_RANGE0 multiCastRange;
	multiCastRange.valueHigh.type = FWP_BYTE_ARRAY16_TYPE;
	multiCastRange.valueLow.type = FWP_BYTE_ARRAY16_TYPE;
	FWP_BYTE_ARRAY16 multicastHigh, multicastLow;
	inet_pton(AF_INET6, const_cast<char*>("ff00::"), &multicastLow);
	inet_pton(AF_INET6, const_cast<char*>("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"), &multicastHigh);
	multiCastRange.valueHigh.byteArray16 = &multicastHigh;
	multiCastRange.valueLow.byteArray16 = &multicastLow;

	allowMulticastConditionsV6[numMulticastConditionsV6].fieldKey = FWPM_CONDITION_IP_REMOTE_ADDRESS;
	allowMulticastConditionsV6[numMulticastConditionsV6].matchType = FWP_MATCH_RANGE;
	allowMulticastConditionsV6[numMulticastConditionsV6].conditionValue.type = FWP_RANGE_TYPE;
	allowMulticastConditionsV6[numMulticastConditionsV6].conditionValue.rangeValue = &multiCastRange;
	numMulticastConditionsV6++;

	fwpmAllowMulticastV6->numFilterConditions = numMulticastConditionsV6;
	fwpmAllowMulticastV6->filterCondition = allowMulticastConditionsV6;

	//end allow multicast v6

	//allow link local v6
	fwpmAllowLinkLocalV6 = (FWPM_FILTER0*)calloc(1, sizeof(FWPM_FILTER0));

	fwpmAllowLinkLocalV6->action.type = FWP_ACTION_PERMIT;
	fwpmAllowLinkLocalV6->action.filterType = WFPKS_ALLOW_V6_LINK_LOCAL_GUID;
	fwpmAllowLinkLocalV6->layerKey = FWPM_LAYER_ALE_AUTH_CONNECT_V6;
	fwpmAllowLinkLocalV6->filterKey = WFPKS_ALLOW_V6_LINK_LOCAL_GUID;
	fwpmAllowLinkLocalV6->subLayerKey = WFPKS_SUBLAYER_GUID;
	fwpmAllowLinkLocalV6->weight.type = FWP_UINT8;
	fwpmAllowLinkLocalV6->weight.uint8 = 4;
	fwpmAllowLinkLocalV6->displayData.name = const_cast<wchar_t*>(displayName);

	if (persistReboot)
	{
		fwpmAllowLinkLocalV6->flags |= FWPM_FILTER_FLAG_PERSISTENT;
	}

	FWP_RANGE0 linklocalRange;
	linklocalRange.valueHigh.type = FWP_BYTE_ARRAY16_TYPE;
	linklocalRange.valueLow.type = FWP_BYTE_ARRAY16_TYPE;
	FWP_BYTE_ARRAY16 linklocalHigh, linklocalLow;
	inet_pton(AF_INET6, const_cast<char*>("fe80::"), &linklocalLow);
	inet_pton(AF_INET6, const_cast<char*>("fe80::ffff:ffff:ffff:ffff"), &linklocalHigh);
	linklocalRange.valueHigh.byteArray16 = &linklocalHigh;
	linklocalRange.valueLow.byteArray16 = &linklocalLow;


	allowLinkLocalConditionsV6[numLinkLocalConditionsV6].fieldKey = FWPM_CONDITION_IP_REMOTE_ADDRESS;
	allowLinkLocalConditionsV6[numLinkLocalConditionsV6].matchType = FWP_MATCH_RANGE;
	allowLinkLocalConditionsV6[numLinkLocalConditionsV6].conditionValue.type = FWP_RANGE_TYPE;
	allowLinkLocalConditionsV6[numLinkLocalConditionsV6].conditionValue.rangeValue = &linklocalRange;
	numLinkLocalConditionsV6++;

	fwpmAllowLinkLocalV6->numFilterConditions = numLinkLocalConditionsV6;
	fwpmAllowLinkLocalV6->filterCondition = allowLinkLocalConditionsV6;

	//end allow link local v6

	//end ipv6 rules

	//add the layers to WFP
	if (result == ERROR_SUCCESS)
	{
		result = FwpmEngineOpen0(NULL, RPC_C_AUTHN_DEFAULT, NULL, NULL, &engineHandle);
	}

	FWPM_SUBLAYER0 fwpSubLayer;
	memset(&fwpSubLayer, 0, sizeof(fwpSubLayer));

	if (result == ERROR_SUCCESS)
	{
		fwpSubLayer.subLayerKey = WFPKS_SUBLAYER_GUID;
		fwpSubLayer.displayData.name = const_cast<wchar_t*>(displayName);
		fwpSubLayer.displayData.description = const_cast<wchar_t*>(L"UltraVPN Filter Sublayer");
		fwpSubLayer.flags = 0;
		fwpSubLayer.weight = 0x100;
		if (persistReboot)
		{
			fwpSubLayer.flags |= FWPM_FILTER_FLAG_PERSISTENT;
		}

		result = FwpmSubLayerAdd0(engineHandle, &fwpSubLayer, NULL);

		if (result == FWP_E_ALREADY_EXISTS)
		{
			result = FwpmSubLayerDeleteByKey0(engineHandle, &WFPKS_SUBLAYER_GUID);
			if (result == ERROR_SUCCESS)
			{
				result = FwpmSubLayerAdd0(engineHandle, &fwpSubLayer, NULL);
			}
		}
	}

	if (result == ERROR_SUCCESS)
	{
		result = FwpmFilterAdd0(engineHandle, fwpmBlockAllFilter, NULL, &filterId);
	}

	if (result == ERROR_SUCCESS)
	{
		result = FwpmFilterAdd0(engineHandle, fwpmIpRangeFilter, NULL, &filterId);
	}

	if (result == ERROR_SUCCESS)
	{
		result = FwpmFilterAdd0(engineHandle, fwpmPortOutFilter, NULL, &filterId);
	}

	if (result == ERROR_SUCCESS)
	{
		result = FwpmFilterAdd0(engineHandle, fwpmIpAddrFilter, NULL, &filterId);
	}

	if (result == ERROR_SUCCESS)
	{
		result = FwpmFilterAdd0(engineHandle, fwpmIpAddrLocalFilter, NULL, &filterId);
	}
	//v6

	if (result == ERROR_SUCCESS)
	{
		result = FwpmFilterAdd0(engineHandle, fwpmBlockAllV6Filter, NULL, &filterId);
	}

	if (result == ERROR_SUCCESS)
	{
		result = FwpmFilterAdd0(engineHandle, fwpmAllowLinkLocalV6, NULL, &filterId);
	}

	if (result == ERROR_SUCCESS)
	{
		result = FwpmFilterAdd0(engineHandle, fwpmAllowLoopbackV6, NULL, &filterId);
	}

	if (result == ERROR_SUCCESS)
	{
		result = FwpmFilterAdd0(engineHandle, fwpmAllowMulticastV6, NULL, &filterId);
	}


	//cleanup
	if (engineHandle != NULL)
		FwpmEngineClose0(engineHandle);

	if (fwpmBlockAllFilter != NULL)
		free(fwpmBlockAllFilter);

	if (fwpmIpRangeFilter != NULL)
		free(fwpmIpRangeFilter);

	if (fwpmPortOutFilter != NULL)
		free(fwpmPortOutFilter);

	if (fwpmIpAddrFilter != NULL)
		free(fwpmIpAddrFilter);

	if (customIpsAndMasks != NULL)
		free(customIpsAndMasks);

	if (localAddrAndMasks != NULL)
		free(localAddrAndMasks);

	if (ovpnBlob != NULL)
		FwpmFreeMemory0((void**)&ovpnBlob);

	if (allowIpConditions != NULL)
		free(allowIpConditions);

	//v6

	if (fwpmBlockAllV6Filter != NULL)
		free(fwpmBlockAllV6Filter);

	if (fwpmAllowLoopbackV6 != NULL)
		free(fwpmAllowLoopbackV6);

	if (fwpmAllowLinkLocalV6 != NULL)
		free(fwpmAllowLinkLocalV6);

	if (fwpmAllowMulticastV6 != NULL)
		free(fwpmAllowMulticastV6);

	if (loopbackv6IpAndMask != NULL)
		free(loopbackv6IpAndMask);

	if (result == ERROR_SUCCESS)
	{
		debugPrint("successfully added filter\n");
	}
	else
	{
		debugPrint("failed to add filter\n");
	}

	return result;
}

DWORD WfpksDisable() {

	HANDLE engineHandle = NULL;
	DWORD result = ERROR_SUCCESS;

	// if (WfpksIsEnabled() == FALSE) {
	//     return ERROR_SUCCESS;
	// }

	result = FwpmEngineOpen0(NULL, RPC_C_AUTHN_DEFAULT, NULL, NULL, &engineHandle);

	if (result == ERROR_SUCCESS) {
		result = FwpmFilterDeleteByKey0(engineHandle, &WFPKS_FILTER_GUID);
		result = FwpmFilterDeleteByKey0(engineHandle, &WFPKS_BLOCKALL_FILTER_GUID);
		result = FwpmFilterDeleteByKey0(engineHandle, &WFPKS_ALLOW_IP_RANGE_FILTER_GUID);
		result = FwpmFilterDeleteByKey0(engineHandle, &WFPKS_ALLOW_PORT_OUT_FILTER_GUID);
		result = FwpmFilterDeleteByKey0(engineHandle, &WFPKS_ALLOW_IP_FILTER_GUID);
		result = FwpmFilterDeleteByKey0(engineHandle, &WFPKS_ALLOW_IP_LOCAL_FILTER_GUID);

		//ipv6
		result = FwpmFilterDeleteByKey0(engineHandle, &WFPKS_BLOCKALL_V6_FILTER_GUID);
		result = FwpmFilterDeleteByKey0(engineHandle, &WFPKS_ALLOW_V6_LINK_LOCAL_GUID);
		result = FwpmFilterDeleteByKey0(engineHandle, &WFPKS_ALLOW_V6_LOOPBACK_GUID);
		result = FwpmFilterDeleteByKey0(engineHandle, &WFPKS_ALLOW_V6_MULTICAST_GUID);
	}

	if (engineHandle != NULL) {
		FwpmEngineClose0(engineHandle);
	}

	return result;
}

