using Utilizr.Util;

namespace Utilizr.Vpn
{
    public interface IVpnProvider
    {
        void Initialize(UserPassHandler userPass);

        void Disconnect();
        /// <summary>
        /// Same as <see cref="Disconnect"/> but will always fire disconnecting / disconnected events.
        /// </summary>
        void Abort();
        Task Connect(IConnectionStartParams startParams);
        BandwidthUsage Usage { get; }
        TimeSpan ConnectedDuration { get; }
        bool IsConnected { get; }

        string? CurrentServer { get; }

        event ConnectionStateHandler Connecting;
        event ConnectionStateHandler Connected;
        event ConnectionStateHandler Disconnecting;
        event ConnectionStateHandler Disconnected;
        event ConnectionStateHandler ConnectError;
        event EventHandler<BandwidthUsage> BandwidthUsageUpdated;
        event EventHandler TapDriverInstallationRequired;

        IEnumerable<ConnectionType> GetAvailableProtocols();
    }
}
