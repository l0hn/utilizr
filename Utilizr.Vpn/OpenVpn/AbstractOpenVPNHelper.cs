using Utilizr.Extensions;
using Utilizr.Globalisation;
using Utilizr.Logging;

namespace Utilizr.Vpn.OpenVpn
{
    public abstract class AbstractOpenVPNHelper
    {
        const int CONNECT_TIMEOUT = 60000;
        private AutoResetEvent _connectingDone;
        private bool _abortTimeout;

        public delegate void ConnectWorkCallback(CallbackHandlerArgs args, Exception? error);
        public delegate void KillConnectionCallback(Exception? error);

        public event EventHandler<BandwidthUsage> BandwidthUpdate;
        public event EventHandler<StateArgs> StateChanged;
        public event EventHandler<PasswordArgs> AuthenticationFailed;
        public event EventHandler<MessageArgs> FatalError;
        public event EventHandler ManagementDisconnected;
        public event EventHandler TapDriverInstallationRequired;
        public event EventHandler ConnectedWithErrors;

        private UserPassHandler _userPassHandler;

        private ManagementClient _lastManagementClient;

        public BandwidthUsage LastBandwidthUsage { get; protected set; }

        public TimeSpan ConnectedDuration
        {
            get
            {
                if (!IsConnected)
                {
                    return TimeSpan.Zero;
                }
                return DateTime.UtcNow - _connectionStartedTime;
            }
        }

        private OVPNState _lastState = 0;

        public bool IsConnected
        {
            get
            {
                if (_lastManagementClient == null)
                    return false;

                if (_lastManagementClient.IsWaitingForHoldRelease)
                {
                    return false;
                }

                return _lastState == OVPNState.CONNECTED;
            }
        }

        public bool IsConnecting
        {
            get
            {
                if (_lastManagementClient == null)
                    return false;

                if (_lastManagementClient.IsWaitingForHoldRelease)
                    return false;

                return _lastState == OVPNState.CONNECTING ||
                       _lastState == OVPNState.RESOLVE ||
                       _lastState == OVPNState.WAIT ||
                       _lastState == OVPNState.AUTH ||
                       _lastState == OVPNState.GET_CONFIG ||
                       _lastState == OVPNState.ADD_ROUTES ||
                       _lastState == OVPNState.ASSIGN_IP;
            }
        }

        //private ManagementClient _managementClient;
        private DateTime _connectionStartedTime;


        protected abstract void ConnectWorkWithCA(string host, string caFilePath, string mode, int port, ConnectWorkCallback callback);
        protected abstract void ConnectWorkWithConfig(string host, string configFile, string protocol, int port, string managementPwd, ConnectWorkCallback callback);
        protected abstract void KillAllOpenVPNConnectionsWork(KillConnectionCallback callback);

        protected AbstractOpenVPNHelper()
        {
            _connectingDone = new AutoResetEvent(false);
            LastBandwidthUsage = new BandwidthUsage();
        }

        public void SetAuthHandler(UserPassHandler handler)
        {
            _userPassHandler = handler;
        }

        public void ConnectWithConfigFile(string host, string configFile, string protocol = "udp", int port = 1194, string managementPwd = null, ConnectWorkCallback callback = null)
        {
            try
            {
                if (!File.Exists(configFile))
                    throw new FileNotFoundException("The OpenVPN config file provided does not exist");

                ConnectWorkWithConfig(
                    host,
                    configFile,
                    protocol,
                    port,
                    managementPwd,
                    (args, error) => ConnectWorkCallbackHandler(args, error, callback));
            }
            catch (Exception ex)
            {
                Log.Exception("OPEN_VPN_HELPER", ex);
                throw;
            }
        }

        private void ConnectWorkCallbackHandler(CallbackHandlerArgs args, Exception ex, ConnectWorkCallback callback = null)
        {
            LastBandwidthUsage.Reset();
            if (args.Port > -1 && ex == null)
            {
                SetupManagementClient(args.Port, args.ManagementPwd);
            }

            callback?.Invoke(args, ex);


            if (ex != null)
            {
                OnFatalError(new MessageArgs { Message = L._("The OpenVPN client could not be started."), Error = ex });
                Log.Exception("OPEN_VPN_HELPER", ex);
            }

            if (args.Port < 0)
                Log.Exception("OPEN_VPN_HELPER", new Exception($"Invalid port ({args.Port}) returned when starting OVPN process."));
        }

        public void Connect(string host, string caFilePath, string mode = "udp", int port = 1194, ConnectWorkCallback callback = null)
        {
            try
            {

                if (!File.Exists(caFilePath))
                {
                    throw new FileNotFoundException("The OpenVPN certificate provided does not exist.");
                }

                ConnectWorkWithCA(
                    host,
                    caFilePath,
                    mode,
                    port,
                    (mPort, ex) => ConnectWorkCallbackHandler(mPort, ex, callback)
                );
            }
            catch (Exception ex)
            {
                Log.Exception("OPEN_VPN_HELPER", ex);
                throw;
            }
        }

        public string LastInterface { get; private set; }

        protected void SetupManagementClient(int port, string mangementPwd)
        {
            var _managementClient = new ManagementClient(port, mangementPwd);
            _lastManagementClient = _managementClient;
            _managementClient.BandwidthUpdate += (s, e) => OnBandwidthUpdate(e);
            _managementClient.StateChanged += (s, e) => OnStateChanged(e);
            _managementClient.Log += (sender, args) =>
            {
                Log.Info("OPEN_VPN", args.LogMessage);
                if (args.LogMessage.ToLower().Contains("inactivity timeout (--ping-exit)"))
                {
                    OnFatalError(new MessageArgs() { Message = L._("The connection timed out.") });
                }
                if (args.LogMessage.ToLower().Contains("authenticate/decrypt packet error: cipher final failed"))
                {
                    OnFatalError(new MessageArgs() { Message = L._("OpenVPN encryption method failed.") });
                    KillAllOpenVPNConnections();
                }
                if (args.LogMessage.ToLower().Contains("opened utun device"))
                {
                    var message = args.LogMessage.ToLower().Replace("opened utun device", "");
                    LastInterface = message.Trim();
                }
            };
            _managementClient.Fatal += OnManagementClientFatal;
            _managementClient.VerificationFailed += (sender, args) =>
            {
                Log.Error("OPEN_VPN", args.VerificationFailureMessage);
                OnAuthenticationFailed(args);
                KillAllOpenVPNConnections();
            };
            _managementClient.UsernamePasswordRequired += (sender, args) =>
            {
                Log.Info("OPEN_VPN", "username / pwd required.");
                try
                {
                    var passArgs = _userPassHandler.Invoke(ConnectionType.OPEN_VPN);
                    args.Username = passArgs.Username;
                    args.Password = passArgs.Password.ToUnsecureString();
                }
                catch (Exception ex)
                {
                    OnManagementClientFatal(this, new MessageArgs() { Message = L._("Failed to retrieve login credentials for this user ({0})", ex.Message) });

                    throw;
                }
            };
            _managementClient.Info += (sender, args) =>
            {
                Log.Info("OPEN_VPN", args.Message);

            };
            _managementClient.Echo += (sender, args) =>
            {
                Log.Info("OPEN_VPN", $"{args.DateTime}: {args.Command}");
            };

            _managementClient.Disconnected += (sender, args) => OnManagementDisconnected();
            _managementClient.Success += (sender, args) =>
            {
                Log.Info("OPEN_VPN", $"Success: {args.Message}");
            };

            _managementClient.Connect(ar =>
            {
                Log.Info("OPEN_VPN", "management client connect called back");
                try
                {
                    // AsyncHelper.EndExecute(ar);
                }
                catch (Exception ex)
                {
                    Log.Exception("OPEN_VPN", ex);
                    OnFatalError(new MessageArgs()
                    {
                        Message = L._("OpenVPN Interface Error: {0}", ex.Message)
                    });
                    return;
                }

                if (_managementClient == null)
                {
                    OnFatalError(new MessageArgs() { Message = "THIS SHOULDN'T HAPPEN" });
                    return;
                }

                Log.Info("OPEN_VPN", $"waiting for hold release = {_managementClient.IsWaitingForHoldRelease}");
                if (_managementClient.IsWaitingForHoldRelease)
                {
                    Log.Info("OPEN_VPN", "releasing hold");
                    ReleaseHoldAndSetupOptions(_managementClient);
                }
                else
                {
                    _managementClient.WaitingForHoldRelease += ManagementClientOnWaitingForHoldRelease;
                    //double checking incase we missed the wait for release hold message while subscribing to the event
                    Log.Info("OPEN_VPN", $"post-event subdcribe waiting for hold release = {_managementClient.IsWaitingForHoldRelease}");
                    if (_managementClient.IsWaitingForHoldRelease)
                    {
                        //remove the event handler
                        _managementClient.WaitingForHoldRelease -= ManagementClientOnWaitingForHoldRelease;
                        ReleaseHoldAndSetupOptions(_managementClient);
                    }
                }

                _managementClient.ConnectedWithErrors += (sender, args) =>
                {
                    OnConnectedWithErrors();
                };
            });
        }

        void ReleaseHoldAndSetupOptions(ManagementClient _managementClient)
        {
            Task.Run(() =>
            {
                Log.Info("OPEN_VPN", "setting state to on");
                _managementClient.SetState(true);
                Log.Info("OPEN_VPN", "setting byte count to 1");
                _managementClient.SetByteCount(1);
                Log.Info("OPEN_VPN", "setting log to on");
                _managementClient.SetLog(true);
                Log.Info("OPEN_VPN", "releasing hold");
                _managementClient.ReleaseHold();
                _abortTimeout = false;

                //manually timeout the connect attempt as openvpn does not seem to have a built-in timeout?
                var timeoutError = !_connectingDone.WaitOne(CONNECT_TIMEOUT, false);
                timeoutError = timeoutError && !_abortTimeout && !IsConnected;

                if (timeoutError)
                {
                    KillAllOpenVPNConnections();
                    Log.Info("OPEN_VPN", $"Timeout: No connection established after {CONNECT_TIMEOUT}ms");
                    OnFatalError(new MessageArgs() { Message = L._("The connection attempt timed out.") });
                }
            });
        }

        protected virtual void OnManagementClientFatal(object sender, MessageArgs args)
        {
            Log.Error("OPEN_VPN", args.Message);

            if (args.Message.ToLowerInvariant().Contains("there are no tap-windows nor wintun adapters on this system"))
            {
                OnTapDriverInstallationRequired();
                return;
            }
            args.Message = L._("OpenVPN Interface Error: {0}", args.Message);
            OnFatalError(args);
        }

        private void ManagementClientOnWaitingForHoldRelease(object sender, EventArgs eventArgs)
        {
            Log.Info("OPEN_VPN", "Waiting for hold release");
            var _managementClient = sender as ManagementClient;
            _managementClient.WaitingForHoldRelease -= ManagementClientOnWaitingForHoldRelease;
            ReleaseHoldAndSetupOptions(_managementClient);
        }

        public void KillAllOpenVPNConnections(KillConnectionCallback callback = null)
        {
            _lastState = OVPNState.EXITING;
            LastInterface = null;

            try
            {
                _abortTimeout = true;
                _lastManagementClient?.Dispose();
                _connectingDone.Set();
                _connectingDone.Reset();
                KillAllOpenVPNConnectionsWork((ex) => callback?.Invoke(ex));

            }
            catch (Exception ex)
            {
                Log.Exception("OPEN_VPN_HELPER", ex);
            }
        }

        protected virtual void OnBandwidthUpdate(ByteCountArgs args)
        {
            LastBandwidthUsage.Update(args.BytesOut, args.BytesIn);
            BandwidthUpdate?.Invoke(this, LastBandwidthUsage);
        }

        protected virtual void OnStateChanged(StateArgs args)
        {
            _lastState = args.State;
            Log.Info("OPEN_VPN", $"State changed to {args.State}");
            if (args.State == OVPNState.CONNECTED)
            {
                _connectionStartedTime = DateTime.UtcNow;
                _connectingDone.Set();
            }
            StateChanged?.Invoke(this, args);
        }

        protected virtual void OnAuthenticationFailed(PasswordArgs args)
        {
            AuthenticationFailed?.Invoke(this, args);
        }

        protected virtual void OnFatalError(MessageArgs args)
        {
            KillAllOpenVPNConnections();
            FatalError?.Invoke(this, args);
        }

        protected virtual void OnManagementDisconnected()
        {
            _lastManagementClient?.Dispose();
            _lastManagementClient = null;
            _lastState = OVPNState.EXITING;
            ManagementDisconnected?.Invoke(this, new EventArgs());
        }

        protected virtual void OnTapDriverInstallationRequired()
        {
            TapDriverInstallationRequired?.Invoke(this, new EventArgs());
        }

        protected virtual void OnConnectedWithErrors()
        {
            ConnectedWithErrors?.Invoke(this, EventArgs.Empty);
        }
    }

    public class OpenVpnException : Exception
    {
        public OpenVpnException(string message)
            : base(message)
        {

        }
    }

    public class OpenVpnAuthenticationException : OpenVpnException
    {
        public OpenVpnAuthenticationException(string message) : base(message)
        {
        }
    }

    public class CallbackHandlerArgs
    {
        public int Port { get; set; }
        public string ManagementPwd { get; set; }
    }
}

