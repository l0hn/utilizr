namespace Utilizr.Vpn
{

    public delegate void ConnectionStateHandler(object sender, string host, Exception? error, object? userContext=null);
    public delegate void UsageHandler(object sender, string host, BandwidthUsage usage, object? userContext = null);

    public interface IVpnController: IDisposable
    {
        event ConnectionStateHandler Connecting;

        event ConnectionStateHandler Connected;

        event ConnectionStateHandler Disconnecting;

        event ConnectionStateHandler Disconnected;

        event ConnectionStateHandler ConnectError;

        event EventHandler DurationUpdate;

        event EventHandler<BandwidthUsage> BandwidthUpdated;

        event EventHandler TapDriverInstallationRequired;

        Task ConnectAsync(IConnectionStartParams startParams);

        Task DisconnectAsync();

        Task AbortAsync();

        string CurrentServer();
        object CurrentContext();

        bool IsConnected { get; }

        /// <summary>
        /// Will prevent ConnectError events from being raised
        /// </summary>
        bool SupressErrors { get; set; }

        /// <summary>
        /// Raises the last ConnectError event
        /// NOTE: calling RaiseLastError() will turn off ErrorSupression
        /// </summary>
        void RaiseLastError();

        Exception? LastError { get; }

        TimeSpan ConnectedDuration { get; }

        BandwidthUsage Usage { get; }

        ConnectionState ConnectionState { get; }

        /// <summary>
        /// Should return the protocols available on this platform
        /// </summary>
        /// <returns></returns>
        IEnumerable<ConnectionType> GetAvailableProtocols();

        ConnectionType CurrentConnectionType { get; }
    }
    
    public interface IConnectionStartParams
    {
        ConnectionType ConnectionType { get; }
        string Hostname { get; set; }
        object Context { get; set; }
    }

    public class OpenVpnConnectionStartParams: IConnectionStartParams
    {
        public ConnectionType ConnectionType => ConnectionType.OPEN_VPN;
        public string Protocol { get; set; } = "udp";
        public int OpenVpnPort { get; set; } = 1194;
        public string Hostname { get; set; }
        public object Context { get; set; } 

        public OpenVpnConnectionStartParams(string hostname, object context)
        {
            Hostname = hostname;
            Context = context;
        }
    }

    public class RasConnectionStartParams : IConnectionStartParams
    {
        public ConnectionType ConnectionType { get; }
        public string Hostname { get; set; }
        public object Context { get; set; }

        public RasConnectionStartParams(string hostname, ConnectionType connectionType, object context = null)
        {
            Hostname = hostname;
            ConnectionType = connectionType;
            Context = context;
        }
    }

    public enum ConnectionType
    {
        PPTP = 1,
        L2TP_IPSEC = 2,
        IKEV2 = 3,
        CISCO_IPSEC = 4,
        SSTP = 5,
        OPEN_VPN = 6,
    }

    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Disconnecting
    }
}
