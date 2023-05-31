//using GetText;
using System;
using Utilizr.Globalisation;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Utilizr.Async;
using Utilizr.Info;
using Utilizr.Logging;
using Utilizr.OpenVPN;
using System.Threading.Tasks;

namespace Utilizr.VPN.Providers
{
	public class OpenVPNProvider : IVPNProvider
	{
		public delegate string CertificateHandler();

	    public delegate string ConfigFileHandler();

		private AutoResetEvent _openVpnDialerDone;

	    private OpenVpnProviderConfig _providerConfig;

		string _currentServer;
		object _currentContext;
        private string _lastOpenVPNServer;

		public OpenVPNProvider(OpenVpnProviderConfig config, AbstractOpenVPNHelper helper)
		{
		    _providerConfig = config;
			_openVpnHelper = helper;
			_openVpnDialerDone = new AutoResetEvent(false);
		}


		//OPEN VPN
		private AbstractOpenVPNHelper _openVpnHelper;

		public TimeSpan ConnectedDuration => _openVpnHelper.ConnectedDuration;

		public bool IsConnected => _openVpnHelper.IsConnected;
        public bool IsConnecting => _openVpnHelper.IsConnecting;

		public BandwidthUsage Usage => _openVpnHelper.LastBandwidthUsage;

		public string CurrentServer => _currentServer;
		public object CurrentUserContext => _currentContext;

        public void Connect(IConnectionStartParams startParams)
		{
			_currentServer = startParams.Hostname;
            _currentContext = startParams.Context;
            
			var ar = ConnectOpenVPNAsync(startParams);
			//AsyncHelper.EndExecute(ar);
		}

		public void Disconnect()
		{
			if (IsConnected || IsConnecting)
			{
				OnDisconnecting(_currentContext);
				_openVpnHelper.KillAllOpenVPNConnections();
			    OnDisconnected(_currentServer, _currentContext);
			}
			else
			{
                _openVpnHelper.KillAllOpenVPNConnections();
            }
		}

        public void Abort()
        {
            OnDisconnecting(_currentContext);
            _openVpnHelper.KillAllOpenVPNConnections();
            OnDisconnected(_currentServer, _currentContext);
        }

        protected void ForceDisconnect()
		{
			_openVpnHelper.KillAllOpenVPNConnections();

			OnDisconnected(_currentServer, _currentContext);
		}

		private Exception StoredException;

		protected AbstractOpenVPNHelper Helper => _openVpnHelper;

		public void Initialize(UserPassHandler userPass)
		{
			_openVpnHelper.SetAuthHandler(userPass);

			_openVpnHelper.AuthenticationFailed += (sender, args) =>
			{
				OnConnectionError(new OpenVPNAuthenticationException(args.VerificationFailureMessage), _currentContext);
				OnDisconnected(_currentServer, _currentContext);
			};
			_openVpnHelper.StateChanged += (sender, args) =>
			{
				switch (args.State)
				{
					case OVPNState.CONNECTING:
                    case OVPNState.RESOLVE:
						OnConnecting(_currentServer, _currentContext);
						break;
					case OVPNState.CONNECTED:
						OnConnected(_currentContext);
						_openVpnDialerDone.Set();
						break;
					case OVPNState.EXITING:
						OnDisconnected(_lastOpenVPNServer ?? _currentServer, _currentContext);
						_openVpnDialerDone.Set();
						break;
				}
			};
			_openVpnHelper.FatalError += (sender, args) =>
			{
				OnConnectionError(new OpenVPNException(args.Message), _currentContext);
				OnDisconnected(_lastOpenVPNServer ?? _currentServer, _currentContext, args.Error);
				_openVpnDialerDone.Set();
			};
			_openVpnHelper.ManagementDisconnected += (sender, args) =>
			{
			    OnDisconnected(_lastOpenVPNServer ?? _currentServer, _currentContext);
			    _openVpnDialerDone.Set();
			};
			_openVpnHelper.TapDriverInstallationRequired += (sender, args) =>
			{
				//raise an event for a ui popup to confirm tap installation
				OnConnectionError(new OpenVPNException(L._("TAP Windows drivers need to be installed on this system")), _currentContext);
				OnTapDriverInstallRequired();
				_openVpnHelper.KillAllOpenVPNConnections();

				OnDisconnected(_currentServer, _currentContext);

				_openVpnDialerDone.Set();
			};
		    _openVpnHelper.BandwidthUpdate += (sender, usage) =>
		    {
		        OnBandwidthUpdated(usage);
		    };
		    _openVpnHelper.ConnectedWithErrors += (sender, args) =>
		    {
                OnConnectionError(new ConnectedWithErrorsException(), _currentContext);
		    };
		}

		protected void OnTapDriverInstallRequired()
		{
			TapDriverInstallationRequired?.Invoke(this, new EventArgs());
		}

		public event EventHandler TapDriverInstallationRequired;


		public IEnumerable<ConnectionType> GetAvailableProtocols()
		{
			var available = new List<ConnectionType>();
			available.Add(ConnectionType.OPEN_VPN);

			return available;
		}


		private void ConnectOpenVPN(IConnectionStartParams startParams)
		{
		    OpenVpnConnectionStartParams ovpnParams = (OpenVpnConnectionStartParams) startParams;

			StoredException = null;

			Disconnect();

			OnConnecting(startParams.Hostname, startParams.Context);
			try
			{
				_openVpnDialerDone.Reset();
			    if (_providerConfig.UseFile)
                    _openVpnHelper.ConnectWithConfigFile(startParams.Hostname, _providerConfig.ConfigFileHandler(), ovpnParams.Protocol, ovpnParams.OpenVpnPort, Guid.NewGuid().ToString());
			    else
                    _openVpnHelper.Connect(startParams.Hostname, _providerConfig.CertificateHandler(), ovpnParams.Protocol, ovpnParams.OpenVpnPort);
				
				_openVpnDialerDone.WaitOne();
			}
			catch (Exception ex)
			{
				//if (StoredException != null)
				//{
				//    new System.Aggreg
				//    StoredException.InnerException = ex;
				//}
				OnConnectionError(ex, startParams.Context);
				OnDisconnected(_lastOpenVPNServer ?? _currentServer, _currentContext);
				_openVpnDialerDone.Set();
			}
			_lastOpenVPNServer = startParams.Hostname;
		}

		public event ConnectionStateHandler Connecting;
		public event ConnectionStateHandler Connected;
		public event ConnectionStateHandler Disconnecting;
		public event ConnectionStateHandler Disconnected;
		public event ConnectionStateHandler ConnectError;
	    public event EventHandler<BandwidthUsage> BandwidthUsageUpdated;

		private IAsyncResult ConnectOpenVPNAsync(IConnectionStartParams startParams, AsyncCallback callback = null)
		{
			return Task.Run(() => ConnectOpenVPN(startParams));
		}

		protected virtual void OnConnectionError(Exception error, object userContext)
		{
			ConnectError?.Invoke(this, _currentServer, error, userContext);
		}

		protected virtual void OnDisconnected(string host, object userContext, Exception error = null)
		{
			Disconnected?.Invoke(this, host, error, userContext);
        }

		protected virtual void OnConnected(object userContext)
		{
			Connected?.Invoke(this, _currentServer, null, userContext);
        }

		protected virtual void OnConnecting(string host, object userContext)
		{
			Connecting?.Invoke(this, host, null, userContext);
        }

		protected virtual void OnDisconnecting(object userContext)
		{
			Disconnecting?.Invoke(this, _currentServer, null);
        }

	    protected virtual void OnBandwidthUpdated(BandwidthUsage usage)
	    {
	        BandwidthUsageUpdated?.Invoke(this, usage);
	    }
	}

    public class OpenVpnProviderConfig
    {
        public OpenVPNProvider.ConfigFileHandler ConfigFileHandler { get; private set; }
        public OpenVPNProvider.CertificateHandler CertificateHandler { get; private set; }

        public bool UseFile { get; private set; }

        public static OpenVpnProviderConfig FromConfigFileHandler(OpenVPNProvider.ConfigFileHandler configHandler)
        {
            return new OpenVpnProviderConfig()
            {
                ConfigFileHandler = configHandler,
                UseFile = true
            };
        }

        public static OpenVpnProviderConfig FromCertificateHandler(OpenVPNProvider.CertificateHandler certHandler)
        {
            return new OpenVpnProviderConfig()
            {
                CertificateHandler = certHandler ,
                UseFile = false
            };
        }

        private OpenVpnProviderConfig()
        {
            
        }
    }

    public class ConnectedWithErrorsException : Exception
    {

    }
}
