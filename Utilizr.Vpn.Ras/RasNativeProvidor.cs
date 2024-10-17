using System.ComponentModel;
using System.ServiceProcess;
using Utilizr.Async;
using Utilizr.Extensions;
using Utilizr.Globalisation;
using Utilizr.Logging;

namespace Utilizr.Vpn.Ras
{
    public class RasNativeProvider : IVpnProvider
    {
        private const string LOG_CAT = "ras_provider";
        
        public BandwidthUsage Usage { get; private set; } = new BandwidthUsage();
        public TimeSpan ConnectedDuration { get; private set; } = TimeSpan.Zero;

        public bool IsConnected
        {
            get
            {
                try
                {
                    IkevVpnStats stats = _rasDialer.GetStats(_deviceName);
                    return stats.Status == IkevVpnStatsStatus.CONNECTED;
                }
                catch (Exception e)
                {
                    Log.Exception(LOG_CAT, e);
                    return false;
                }
            }
            private set { IsConnected = value; }
        }

        public string? CurrentServer
        {
            get
            {
                if (_currentServer != null)
                {
                    return _currentServer;
                }

                if (IsConnected)
                {
                    try
                    {
                        IkevVpnStats stats = _rasDialer.GetStats(_deviceName);
                        return _currentServer = stats.Hostname;
                    }
                    catch (Win32Exception e)
                    {
                        Log.Exception(LOG_CAT, e);
                        return null;
                    }
                }

                return null;
            }
            private set { _currentServer = value; }
        }

        public event ConnectionStateHandler Connecting;
        public event ConnectionStateHandler Connected;
        public event ConnectionStateHandler Disconnecting;
        public event ConnectionStateHandler Disconnected;
        public event ConnectionStateHandler ConnectError;
        public event EventHandler<BandwidthUsage> BandwidthUsageUpdated;
        public event EventHandler TapDriverInstallationRequired; //not used

        private string _currentServer;
        private UserPassHandler _userPassHandler;
        private string _deviceName;
        private object _context;
        private IkevVpn _rasDialer;
        private ManualResetEvent _rasDialerDone = new ManualResetEvent(false);
        private ManualResetEvent _connectDone = new ManualResetEvent(false);
        private Task _checkStatusTask;

        public RasNativeProvider(string deviceName)
        {
            _deviceName = deviceName;
        }

        public void Initialize(UserPassHandler userPass)
        {
            _rasDialer = new IkevVpn();
            _rasDialer.DialComplete += RasDialerOnDialComplete;
            _rasDialer.DialAborted += RasDialerOnDialAborted;
            _rasDialer.DialError += RasDialerOnDialError;
            _userPassHandler = userPass;

            _checkStatusTask = new Task(CheckStatusThread, TaskCreationOptions.LongRunning);
            _checkStatusTask.Start();

            if (IsConnected && CurrentServer != null)
            {
                _connectDone.Set();
            }
        }

        private void RasDialerOnDialError(uint error)
        {
            try
            {
                Log.Error($"rasdialer error callback {error}", error);

                // https://docs.microsoft.com/en-us/windows/win32/eaphost/eap-related-error-and-information-constants
                const uint EAP_E_USER_NAME_PASSWORD_REJECTED = 0x80420112;
                // https://docs.microsoft.com/en-us/windows/win32/rras/routing-and-remote-access-error-codes
                const uint ERROR_AUTHENTICATION_FAILURE = 691;

                if (error == ERROR_AUTHENTICATION_FAILURE ||
                    error == EAP_E_USER_NAME_PASSWORD_REJECTED)
                {
                    OnConnectError(new IkevVpnAuthenticationException(L._("Authentication failure during connection attempt.")), _context);
                }
                else
                {
                    OnConnectError(new Win32Exception((int)error), _context);
                }
                OnDisconnected(_currentServer, _context);

            }
            catch (Exception e)
            {
                Log.Exception(LOG_CAT, e);
            }
            finally
            {
                _rasDialerDone.Set();
            }
        }

        private void RasDialerOnDialAborted()
        {
            try
            {
                Log.Info(LOG_CAT, $"rasdialer disconnected callback");
                OnDisconnected(_currentServer, _context);
            }
            catch (Exception e)
            {
                Log.Exception(LOG_CAT, e);
            }
        }

        private void RasDialerOnDialComplete()
        {
            try
            {
                Log.Info(LOG_CAT, $"rasdialer complete callback");
                OnConnected(_context);
            }
            catch (Exception e)
            {
                Log.Exception(LOG_CAT, e);
            }
            finally
            {
                _rasDialerDone.Set();
            }
        }

        public void CreateDevice(string connectionHostname)
        {
            _rasDialer.CreateDevice(_deviceName, connectionHostname);
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                OnDisconnecting(_currentServer, _context);
                _rasDialer.Disconnect(_deviceName);
                Sleeper.Sleep(Time.Time.SECOND);
                OnDisconnected(_currentServer, _context);
            }
            else
            {
                OnDisconnected(_currentServer, _context);
            }
            _rasDialerDone.Set();
        }

        public async void Abort()
        {
            OnDisconnecting(_currentServer, _context);
            _rasDialerDone.Set();
            _rasDialer.Abort();

            await Task.Delay(Time.Time.SECOND * 2);
            Disconnect();
        }

        public Task Connect(IConnectionStartParams startParams)
        {
            return Task.Run(() =>
            {
                var rasParams = (RasConnectionStartParams)startParams;

                ConnectedDuration = TimeSpan.Zero;
                Usage.Reset();

                OnConnecting(startParams.Context, rasParams.Hostname);

                try
                {
                    _context = startParams.Context;
                    _currentServer = rasParams.Hostname;

                    var credentials = _userPassHandler(ConnectionType.IKEV2);

                    _rasDialerDone.Reset();
                    _rasDialer.ResetAbort();

                    RasmanRunning();
                    Log.Info(LOG_CAT, $"rasdialer connecting to {rasParams.Hostname}");

                    _rasDialer.Connect(
                        _deviceName,
                        rasParams.Hostname,
                        credentials.Username,
                        credentials.Password.ToUnsecureString()
                    );

                    Log.Info(LOG_CAT, "waiting for rasDialer complete..");
                    _rasDialerDone.WaitOne();

                    Log.Info(LOG_CAT, "rasdialer completed");
                }
                catch (Exception e)
                {
                    Log.Exception(LOG_CAT, e);
                    OnConnectError(e, _context);
                    OnDisconnected(_currentServer, _context);
                    _rasDialerDone.Set();
                }
            });
        }

        /// <summary>
        /// Ensure the Remote Access Connection Manager service is running before we attempt to connect
        /// </summary>
        void RasmanRunning()
        {
            const string service = "rasman";
            try
            {
                using (var controller = new ServiceController(service))
                {
                    if (controller.Status == ServiceControllerStatus.Running)
                        return;

                    Log.Info(LOG_CAT, $"{service} not running, attempting to start");

                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(120));

                    Log.Info(LOG_CAT, $"{service} now running");
                }
            }
            catch (System.ServiceProcess.TimeoutException tEx)
            {
                Log.Exception(LOG_CAT, tEx, $"Timeout starting {service}, connection attempt will almost certainly fail.");
            }
            catch (Exception ex)
            {
                Log.Exception(
                    LOG_CAT,
                    ex,
                    $"Failure ensuring {service} is running. Connection attempt will almost certainly fail if it's not running."
                );
            }
        }

        public IEnumerable<ConnectionType> GetAvailableProtocols()
        {
            return new[] { ConnectionType.IKEV2 };
        }

        private async void CheckStatusThread()
        {
            while (true)
            {
                _connectDone.WaitOne();

                try
                {
                    IkevVpnStats stats = _rasDialer.GetStats(_deviceName);

                    if (stats.Status == IkevVpnStatsStatus.DISCONNECTED)
                    {
                        _rasDialerDone.Set();
                        OnDisconnected(_currentServer, _context);
                        continue;
                    }

                    _currentServer = stats.Hostname;
                    Usage.Update(stats.BytesTransmitted, stats.BytesReceived);
                    ConnectedDuration = TimeSpan.FromMilliseconds(stats.ConnectDuration);

                    OnBandwidthUpdated(Usage);
                }
                catch (Exception e)
                {
                    Log.Exception("ras_provider", e);
                    OnDisconnected(_currentServer, _context);
                }

                await Task.Delay(Time.Time.SECOND);
            }
        }

        protected virtual void OnConnecting(object context, string server)
        {
            Connecting?.Invoke(this, server, null, context);
        }

        protected virtual void OnConnected(object context)
        {
            _connectDone.Set();
            Connected?.Invoke(this, CurrentServer!, null, context);
        }

        protected virtual void OnConnectError(Exception error, object context)
        {
            ConnectError?.Invoke(this, CurrentServer!, error, context);
        }

        protected virtual void OnDisconnected(string server, object context)
        {
            _connectDone.Reset();
            Disconnected?.Invoke(this, server, null, context);
        }

        protected virtual void OnDisconnecting(string server, object context)
        {
            Disconnecting?.Invoke(this, server, null, context);
        }

        protected virtual void OnBandwidthUpdated(BandwidthUsage usage)
        {
            BandwidthUsageUpdated?.Invoke(this, usage);
        }
    }

    public class IkevVpnAuthenticationException : Exception
    {
        public IkevVpnAuthenticationException(string message) : base(message)
        {

        }
    }
}
