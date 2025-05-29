using Utilizr.Extensions;
using Utilizr.Logging;
using Utilizr.Util;

namespace Utilizr.Vpn
{

    public class VpnController : IVpnController
    {
        const string LOG_CAT = "vpn-controller";

        //keep track of the provider last used to connect
        private IVpnProvider CurrentProvider;

        private IVpnProvider[] Providers;

        private object _userContext;

        private Timer _updateTimer;

        public event EventHandler TapDriverInstallationRequired;
        public event EventHandler<BandwidthUsage> BandwidthUpdated;

        private UserPassHandler _userPassHandler;
        private bool _initializeDone;

        public Exception? LastError { get; private set; }

        public ConnectionType CurrentConnectionType { get; private set; }

        public bool SupressErrors { get; set; }

        public VpnController(UserPassHandler authenticationHandler, bool initializeNow = true, params IVpnProvider[] providers)
        {
            _userPassHandler = authenticationHandler;
            Providers = providers;

            if (initializeNow)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            if (_initializeDone)
            {
                return;
            }

            foreach (var item in Providers)
            {
                //set up each included provider
                item.Initialize(_userPassHandler);
                item.Connected += (s, h, e, c) => OnConnected(c);
                item.Connecting += (s, h, e, c) => OnConnecting(h, c);
                item.ConnectError += (s, h, e, c) => OnConnectionError(e, c);
                item.Disconnected += (s, h, e, c) => OnDisconnected(h, c, e);
                item.Disconnecting += (s, h, e, c) => OnDisconnecting();
                item.TapDriverInstallationRequired += (s, e) => TapDriverInstallationRequired?.Invoke(s, e);
                item.BandwidthUsageUpdated += (sender, usage) => OnBandwidthUpdated(usage);
            }

            _updateTimer = new Timer((o) =>
            {
                if (IsConnected)
                {
                    OnDurationUpdated();
                }
            }, null, 1000, 1000);

            if (IsConnected)
                ConnectionState = ConnectionState.Connected;

            _initializeDone = true;
        }


        #region IVpnController implementation
        public event ConnectionStateHandler Connecting;
        public event ConnectionStateHandler Connected;
        public event ConnectionStateHandler Disconnecting;
        public event ConnectionStateHandler Disconnected;
        public event ConnectionStateHandler ConnectError;
        public event EventHandler DurationUpdate;

        public Task<string> ConnectAsync(IConnectionStartParams startParams)
        {
            return Task.Run(() => Connect(startParams));
        }

        async Task<string> Connect(IConnectionStartParams startParams)
        {
            var connectedCountry = string.Empty;
            try
            {
                LastError = null;
                Log.Info(LOG_CAT, $"Connecting to {startParams.Hostname}");
                _userContext = startParams.Context;

                if (!GetAvailableProtocols().Contains(startParams.ConnectionType))
                {
                    throw new NotSupportedException($"{startParams.ConnectionType} is not available on this platform");
                }

                var provider = Providers.FirstOrDefault(a => a.GetAvailableProtocols().Contains(startParams.ConnectionType));
                if (provider == null)
                {
                    throw new NotSupportedException($"{startParams.ConnectionType} provider was not found on this platform");
                }


                var supressOldVal = SupressErrors;
                try
                {
                    if (IsConnected)
                    {
                        SupressErrors = true;
                        CurrentProvider?.Disconnect();
                    }
                }
                catch (Exception e)
                {
                    Log.Exception(LOG_CAT, e);
                }
                finally
                {
                    SupressErrors = supressOldVal;
                }

                CurrentProvider = provider;

                connectedCountry = await provider.Connect(startParams);
                CurrentConnectionType = startParams.ConnectionType;
            }
            catch (Exception ex)
            {
                Log.Exception(LOG_CAT, ex);
                throw;
            }
            if (LastError != null)
            {
                throw LastError;
            }

            return connectedCountry;
        }


        private void Hangup()
        {
            foreach (var item in Providers)
            {
                item.Disconnect();
            }
        }

        void Abort()
        {
            foreach (var item in Providers)
            {
                item.Abort();
            }
        }

        Task HangupActiveConnectionAsync()
        {
            return Task.Run(Hangup);
        }


        Task AbortActiveConnectionAsync()
        {
            return Task.Run(Abort);
        }

        public Task DisconnectAsync()
        {
            return HangupActiveConnectionAsync();
        }


        public Task AbortAsync()
        {
            return AbortActiveConnectionAsync();
        }

        public string? CurrentServer() 
        {
            return CurrentProvider?.CurrentServer;
        }

        public object CurrentContext()
        {
            return _userContext;
        }

        public bool IsConnected
        {
            get
            {
                if (CurrentProvider != null)
                {
                    return CurrentProvider.IsConnected;
                }

                foreach (var provider in Providers)
                {
                    if (provider.IsConnected)
                    {
                        CurrentProvider = provider;
                        return true;
                    }
                }

                return false;
            }
            set { IsConnected = value; }
        }

        public void RaiseLastError()
        {
            SupressErrors = false;
            if (LastError != null)
            {
                OnConnectionError(LastError, _userContext);
            }
        }

        public TimeSpan ConnectedDuration => CurrentProvider?.ConnectedDuration ?? TimeSpan.Zero;

        public BandwidthUsage Usage => CurrentProvider?.Usage ?? new BandwidthUsage();

        public ConnectionState ConnectionState { get; private set; }

        public IEnumerable<ConnectionType> GetAvailableProtocols()
        {
            return Providers.SelectMany(a => a.GetAvailableProtocols()).Distinct();
        }

        protected virtual void OnConnected(object userContext)
        {
            if (string.IsNullOrEmpty(CurrentServer()))
                return;

            Log.Info(LOG_CAT, $"Connected to server: {CurrentServer()}: STACK: {Stack.CallerInfo()}");
            ConnectionState = ConnectionState.Connected;
            Connected?.Invoke(this, CurrentServer()!, null, userContext);
        }

        protected virtual void OnDisconnected(string host, object? userContext = null, Exception? error = null)
        {
            if (SupressErrors)
            {
                return;
            }
            if (host.IsNullOrEmpty())
            {
                return;
            }
            
            Log.Info(LOG_CAT, $"Disconnected from server: {host}: STACK: {Stack.CallerInfo()}");
            ConnectionState = ConnectionState.Disconnected;
            Disconnected?.Invoke(this, host, error, userContext);
        }

        protected virtual void OnConnecting(string host, object userContext)
        {
            if (host.IsNullOrEmpty())
            {
                return;
            }
            Log.Info(LOG_CAT, $"Connecting to server: {host}: STACK: {Stack.CallerInfo()}");
            ConnectionState = ConnectionState.Connecting;
            Connecting?.Invoke(this, host, null, userContext);
        }

        protected virtual void OnConnectionError(Exception? error, object userContext)
        {
            LastError = error;
            if (SupressErrors)
            {
                return;
            }
            ConnectError?.Invoke(this, CurrentServer()!, error, userContext);
        }

        protected virtual void OnDisconnecting()
        {
            if (CurrentServer() == null)
            {
                return;
            }
            ConnectionState = ConnectionState.Disconnecting;
            Disconnecting?.Invoke(this, CurrentServer()!, null);
        }

        protected virtual void OnDurationUpdated()
        {
            DurationUpdate?.Invoke(this, new EventArgs());
        }

        protected virtual void OnBandwidthUpdated(BandwidthUsage? usage)
        {
            BandwidthUpdated?.Invoke(this, usage);
        }
        #endregion

        public void Dispose()
        {
            _updateTimer?.Dispose();
            foreach (var item in Providers)
            {
                item.Disconnect();
            }

        }
    }

}
