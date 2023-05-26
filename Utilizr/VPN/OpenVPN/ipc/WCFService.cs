#if !NETCOREAPP



using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using Utilizr.Logging;

namespace Utilizr.OpenVPN.ipc
{
    [ServiceContract]
    public interface IWCFService
    {
        [OperationContract]
        int RunOpenVPN(string host, string caFile, string mode = "udp", int port = 1194, int pingTimeout = 30);

        [OperationContract]
        int RunOpenVPNWithConfigFile(string host, string configFile, string mode = "udp", int port = 1194);

        [OperationContract]
        void StopOpenVPN();
    }

    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple
        )]
    public class WCFService: IWCFService
    {
        public const string ADDRESS = "net.pipe://localhost/jdivpn/jdivpnservice";
        

        public int RunOpenVPN(string host, string caFile, string mode = "udp", int port = 1194, int pingTimeout = 30)
        {
            try
            {
                Log.Info("OPEN_VPN_SERVICE", $"Client requested start OpenVPN connection with host={host}, mode={mode}, pingTimeout={pingTimeout}");
                return ProcessManager.LaunchOpenVPN(host, caFile, mode, pingTimeout);
            }
            catch (Exception ex)
            {
                Log.Exception("OPEN_VPN_SERVICE", ex);
                throw;
            }
            
        }

        public int RunOpenVPNWithConfigFile(string host, string configFile, string mode = "udp", int port = 1194)
        {
            try
            {
                Log.Info("OPEN_VPN_SERVICE", $"Client requested start OpenVPN connection with host={host}, configFile={configFile}");
                return ProcessManager.LaunchOpenVPN(host, configFile);
            }
            catch (Exception ex)
            {
                Log.Exception("OPEN_VPN_SERVICE", ex);
                throw;
            }

        }

        public void StopOpenVPN()
        {
            try
            {
                Log.Info("OPEN_VPN_SERVICE", "Client requested stop all OpenVPN Connections");
                ProcessManager.StopAllOpenVPNProcesses();
            }
            catch (Exception ex)
            {
                Log.Exception("OPEN_VPN_SERVICE", ex);
                throw;
            }
        }

        public static IWCFService CreateClient()
        {
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.SendTimeout = TimeSpan.MaxValue;
            binding.MaxReceivedMessageSize = int.MaxValue;
            EndpointAddress ep = new EndpointAddress(ADDRESS);
            IWCFService client = ChannelFactory<IWCFService>.CreateChannel(binding, ep);
            return client;
        }
    }
}
#endif