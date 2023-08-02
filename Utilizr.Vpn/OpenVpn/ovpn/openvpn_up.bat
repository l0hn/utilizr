C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe -Command "& { $enabled = Get-NetAdapterBinding '%1' -ComponentID ms_tcpip6 | select -expand Enabled; if($enabled -eq 'True') {exit 100} else {exit 101}}" >> .\logs\ovpn-ipv6.log

set IPv6_Enabled=%errorlevel%
echo %date% %time% IPv6 enabled=%IPv6_Enabled% on %1 >> .\logs\ovpn-ipv6.log

if %IPv6_Enabled% == 100 (
    echo %date% %time% Setting IPv6 flag to re-enable at a later time for %1 >> .\logs\ovpn-ipv6.log
    type nul > ipv6_enabled.flag
    echo %date% %time% Disabling IPv6 for leak protection on %1 >> .\logs\ovpn-ipv6.log
    C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe -Command "& {Disable-NetAdapterBinding -InterfaceAlias '%1' -ComponentID ms_tcpip6}" >> .\logs\ovpn-ipv6.log
)
exit 0