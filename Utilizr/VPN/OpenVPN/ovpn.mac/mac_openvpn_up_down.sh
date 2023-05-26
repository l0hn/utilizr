#!/bin/bash

PATH=/usr/sbin:$PATH

PRIMARY_INTERFACE=`echo 'show State:/Network/Global/IPv4' | scutil | grep PrimaryInterface | sed -e 's/.*PrimaryInterface : //'`

if [[ "${PRIMARY_INTERFACE}" == "" ]]; then
    if [ "$1" == "up" ] ; then
    	#not much we can do without the primary interface, bail
    	echo "Could not find primary interface"
    	exit 1
    fi
fi

echo "Found primary interface ${PRIMARY_INTERFACE}"

PRIMARY_SERVICE=`echo 'show State:/Network/Global/IPv4' | scutil | grep PrimaryService | sed -e 's/.*PrimaryService : //'`
echo "Found primary service ${PRIMARY_SERVICE}"

BACKUP_IPV6_AUTOMATIC="State:/Network/SSBrand/Backup/IPv6/Auto"

INTERFACE_NAME="$(echo "show Setup:/Network/Service/${PRIMARY_SERVICE}" | scutil | grep UserDefinedName | sed -e 's/.*UserDefinedName : //')"

echo "Found interface name ${INTERFACE_NAME}"

if [ "$1" == "up" ]  ; then

	echo "Executing openvpn-UP"

 	DOMAIN_NAME="SSBrand-client"
    FOREIGN_OPTIONS=`env | grep -E '^foreign_option_' | sort | sed -e 's/foreign_option_.*=//'`

#extract foreign options returned by VPN server
    while read -r option
    do
        case ${option} in
            *DOMAIN*)
                DOMAIN_NAME=${option//dhcp-option DOMAIN /}
                ;;
            *DNS*)
                VPN_DNS=${option//dhcp-option DNS /}
                ;;
        esac
    done <<< "${FOREIGN_OPTIONS}"

    echo "Parsed domain: $DOMAIN_NAME"
    echo "Parsed VPN DNS IP: $VPN_DNS"

#craete a new entry in systemconfiguration with our VPN settings
    scutil <<_EOF
        d.init
        d.add ServerAddresses * ${VPN_DNS}
        d.add DomainName "${DOMAIN_NAME}"
        d.add NetworkInterfaceID "${PRIMARY_SERVICE}"
        d.add IsFromScript "true"

        set State:/Network/SSBrand/DNS
_EOF

	echo "Wrote State:/Network/SSBrand/DNS"

	echo "show Setup:/Network/Service/${PRIMARY_SERVICE}/DNS" | scutil | grep IsFromScript >/dev/null
	setupExists=$?

	echo "show State:/Network/Service/${PRIMARY_SERVICE}/DNS" | scutil | grep IsFromScript >/dev/null
	stateExists=$?

#look to see if ipv6 currently set to "Automatic" and disable 
#if so, ignore other ipv6 settings as too complicated to restore easily

	networksetup -getinfo ${INTERFACE_NAME} | grep "IPv6: Automatic" >/dev/null
	ipv6IsAuto=$?

	if [ $setupExists != 0 ] ; then
		echo "Creating Setup Configuration"
		#setup values for app are not set - create them

		echo "Copying existing Setup"
		#copy current DNS setup settings under our own temporary key
		   	scutil <<_EOF
		        d.init
		    	get Setup:/Network/Service/${PRIMARY_SERVICE}/DNS
		        set State:/Network/SSBrand/Backup/DNS/Setup

_EOF

		echo "Copying new Setup over existing"
		#copy our newly created VPN DNS settings over to the primary service setup
		   	scutil <<_EOF
			 	d.init
			    get State:/Network/SSBrand/DNS
			    set Setup:/Network/Service/${PRIMARY_SERVICE}/DNS
_EOF

	fi

	if [ $stateExists != 0 ] ; then
		echo "Creating State Configuration"
		#state values for app are not set - create them

		echo "Copying existing State"
		#copy current DNS active settings under our own temporary key
		    scutil <<_EOF
		    d.init
		    get State:/Network/Service/${PRIMARY_SERVICE}/DNS
		    set State:/Network/SSBrand/Backup/DNS/State
_EOF
			
		echo "Copying new State over existing"
		#activate the newly created VPN DNS settings 
		    scutil <<_EOF
		    d.init
		    get State:/Network/SSBrand/DNS
		    set State:/Network/Service/${PRIMARY_SERVICE}/DNS
_EOF

	fi

	if [ $ipv6IsAuto == 0 ] ; then
		echo "Found ipv6 set to Automatic"

		echo "Create entry to record current ipv6 state"
		    scutil <<_EOF
		    d.init
		    set ${BACKUP_IPV6_AUTOMATIC}
_EOF
		
		echo "Disabling ipv6"
		networksetup -setv6off ${INTERFACE_NAME}

	fi

elif [ "$1" == "down" ] ; then

	echo "Executing openvpn-DOWN"

	echo "show State:/Network/SSBrand/Backup/DNS/Setup" | scutil | grep dictionary >/dev/null
	originalSetup=$?

	echo "show State:/Network/SSBrand/Backup/DNS/State" | scutil | grep dictionary >/dev/null
	originalState=$?

	echo "show ${BACKUP_IPV6_AUTOMATIC}" | scutil | grep dictionary >/dev/null
	hadAutomaticIPv6=$?

	previousService=`echo "show State:/Network/SSBrand/DNS" | scutil | grep NetworkInterfaceID | sed -e 's/.*NetworkInterfaceID : //'`
	

	if [ $originalSetup = 0 ] ; then
		echo "Found original Setup, restoring"
		#found original Setup config, write back to primary service
		scutil <<_EOF
	    d.init
	    get State:/Network/SSBrand/Backup/DNS/Setup
	    set Setup:/Network/Service/${previousService}/DNS
_EOF
	else
		echo "No original Setup found"
	fi


	if [ $originalState = 0 ] ; then
		echo "Found original State, restoring"
		#found original State config, write back to primary service
		scutil <<_EOF
	    d.init
	    get State:/Network/SSBrand/Backup/DNS/State
	    set State:/Network/Service/${previousService}/DNS
_EOF
	else 
		echo "No original State found"
	fi

	if [ $hadAutomaticIPv6 = 0 ] ; then
		echo "Found auto ipv6 flag State, re-enabling"
		networksetup -setv6Automatic ${INTERFACE_NAME}
	else 
		echo "No previous ivp6 state found"
	fi

#clean out all items under our keys
    scutil <<_EOF
        remove State:/Network/SSBrand/Backup/DNS/Setup
        remove State:/Network/SSBrand/Backup/DNS/State
        remove ${BACKUP_IPV6_AUTOMATIC}
        remove State:/Network/SSBrand/DNS

        quit
_EOF

fi
