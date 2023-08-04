if exist ipv6_enabled.flag (
    echo %date% %time% Re-enabling IPv6 on %1 >> .\logs\ovpn-ipv6.log
    C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe -Command "& {Enable-NetAdapterBinding -InterfaceAlias '%1' -ComponentID ms_tcpip6}" >> .\logs\ovpn-ipv6.log
    echo %date% %time% Removing re-enable IPv6 flag for %1 >> .\logs\ovpn-ipv6.log
    del ipv6_enabled.flag
) else (
    echo %date% %time% IPv6 was already disabled on %1, not re-enabling
)
exit 0