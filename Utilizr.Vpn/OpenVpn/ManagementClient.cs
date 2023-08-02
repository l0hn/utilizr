using Utilizr.Async;
using Utilizr.Extensions;
using Utilizr.Globalisation;
using Utilizr.Network;

namespace Utilizr.Vpn.OpenVpn
{
    public class ManagementClient: IDisposable
    {

        private static string _flushStr = "".PadLeft(1024, '\0');

        TelnetClient _telnetClient;
        
        public event EventHandler<EchoArgs> Echo;
        public event EventHandler<MessageArgs> Info;
        public event EventHandler<LogArgs> Log;
        public event EventHandler<MessageArgs> Fatal;
        public event EventHandler ConnectedWithErrors;
        public event EventHandler<ByteCountArgs> BandwidthUpdate;
        public event EventHandler WaitingForHoldRelease;
        public event EventHandler HoldReleaseSucceeded;
        public event EventHandler<PasswordArgs> UsernamePasswordRequired;
        public event EventHandler<PasswordArgs> PrivateKeyRequired;
        public event EventHandler<PasswordArgs> VerificationFailed;
        public event EventHandler<StateArgs> StateChanged;
        public event EventHandler Disconnected;
        public event EventHandler<MessageArgs> Success;

        public bool IsWaitingForHoldRelease { get; set; }
        /// <summary>
        /// Last bandwidth update time recieved from the management interface
        /// </summary>
        public DateTime LastBandwidthUpdate { get; private set; }

        public bool ReceivedExitNotification { get; private set; }

        public ManagementClient(int port, string managementPwd)
        {
            
            _telnetClient = new TelnetClient("localhost", port, 60000, 60000);
            _telnetClient.MagicPhrases.Add("ENTER PASSWORD:");

            _telnetClient.Router.AddHandler("ENTER PASSWORD:", args =>
            {
                _telnetClient.Send(managementPwd);
            }, startswith:false, endswith:true);
            _telnetClient.Router.AddHandler(">ECHO:", args =>
            {
                if (_isDisposing)
                    return;
                Echo?.Invoke(this, EchoArgs.FromString(trimType(args.Message)));
            });
            _telnetClient.Router.AddHandler(">LOG:", args =>
            {
                if (_isDisposing)
                    return;
                Log?.Invoke(this, LogArgs.FromString(trimType(args.Message)));
            });
            _telnetClient.Router.AddHandler(">INFO:", args =>
            {
                if (_isDisposing)
                    return;
                Info?.Invoke(this, new MessageArgs() {Message = args.Message});
            });
            _telnetClient.Router.AddHandler(">FATAL:", args =>
            {
                if (_isDisposing)
                    return;
                Fatal?.Invoke(this, new MessageArgs() { Message = trimType(trimType(args.Message))});
            });
            _telnetClient.Router.AddHandler(">BYTECOUNT:", args =>
            {
                if (_isDisposing)
                    return;
                LastBandwidthUpdate = DateTime.UtcNow;
                BandwidthUpdate?.Invoke(this, ByteCountArgs.FromString(trimType(args.Message)));
            });
            _telnetClient.Router.AddHandler(">HOLD:", args =>
            {
                if (_isDisposing)
                    return;
                IsWaitingForHoldRelease = true;
                WaitingForHoldRelease?.Invoke(this, new EventArgs());
            });
            _telnetClient.Router.AddHandler("SUCCESS:", args => HandleSuccessMessage(trimType(args.Message)));
            _telnetClient.Router.AddHandler(">PASSWORD:", args => HandlePasswordMessage(trimType(args.Message)));
            _telnetClient.Router.AddHandler(">STATE:", args =>
            {
                var stateArgs = StateArgs.FromString(trimType(args.Message));
                if (_isDisposing)
                {
                    if (stateArgs.State == OVPNState.EXITING)
                    {
                        ReceivedExitNotification = true;
                    }
                    return;
                }

                if (stateArgs.State == OVPNState.RECONNECTING)
                {
                    _telnetClient.Disconnect();
                }
                
                if (stateArgs.State == OVPNState.CONNECTED && args.Message.Contains(",ERROR,"))
                {
                    _telnetClient.Disconnect();
                    ConnectedWithErrors?.Invoke(this, EventArgs.Empty);
                    return;
                }
                
                StateChanged?.Invoke(this, StateArgs.FromString(trimType(args.Message)));
            });
            
            _telnetClient.Disconnected += (sender, args) =>
            {
                if (_isDisposing)
                    return;

                ReceivedExitNotification = true;
                Disconnected?.Invoke(this, new EventArgs());
            };
        }

        public IAsyncResult Connect(AsyncCallback callback = null)
        {
            return _telnetClient.Connect();
        }

        void HandleSuccessMessage(string message)
        {
            switch (message.ToLowerInvariant())
            {
                case "hold release succeeded":
                    IsWaitingForHoldRelease = false;
                    HoldReleaseSucceeded?.Invoke(this, new EventArgs());
                    break;
            }
            Success?.Invoke(this, new MessageArgs() { Message = message });
        }

        void HandlePasswordMessage(string message)
        {
            var passwordArgs = PasswordArgs.FromString(message);
            if (passwordArgs.NeedUsernamePassword)
            {
                OnNeedUsernameAndPassword(passwordArgs);
                return;
            }
            if (passwordArgs.NeedPrivateKey)
            {
                OnNeedPrivateKey(passwordArgs);
            }
            if (passwordArgs.VerificationFailureMessage.IsNotNullOrEmpty())
            {
                VerificationFailed?.Invoke(this, passwordArgs);
            }
        }

        string Escape(string argument)
        {
            return argument.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace(" ", "\\ ");
        }

        /// <summary>
        /// The bytecount command is used to request real-time notification of OpenVPN bandwidth usage.
        /// </summary>
        /// <param name="bytecount">
        /// 0 -- turn off bytecount notifications
        /// n>0 -- setup automatic notifications of bandwaidth usage once every n seconds
        /// </param>
        public void SetByteCount(int seconds)
        {
            _telnetClient.Send($"bytecount {seconds}");
        }

        /// <summary>
        /// The echo capability is used to allow GUI-specific parameters to be either embedded in the 
        /// OpenVPN config file or pushed to an OpenVPN client from a server.
        /// </summary>
        /// <param name="on"></param>
        public void SetEcho(bool on)
        {
            _telnetClient.Send("echo " + (on ? "on" : "off"));
        }

        /// <summary>
        /// leave hold state and start OpenVPN, but do not alter the current hold flag setting.
        /// </summary>
        public void ReleaseHold()
        {
            _telnetClient.Send("hold release");
        }

        /// <summary>
        /// turn on/off hold flag so that future restarts will hold / not hold.
        /// </summary>
        /// <param name="on"></param>
        public void SetHold(bool on)
        {
            _telnetClient.Send("hold " + (on ? "on" : "off"));
        }

        /// <summary>
        /// Enable / disable real-time output of log messages.
        /// </summary>
        /// <param name="on"></param>
        public void SetLog(bool on)
        {
            _telnetClient.Send("log " + (on ? "on" : "off"));
        }

        /// <summary>
        /// Tell OpenVPN your password
        /// </summary>
        /// <param name="pwd"></param>
        public void SetAuthPassword(string pwd)
        {
            _telnetClient.Send($"password Auth \"{Escape(pwd)}\"");
            Thread.Sleep(200);
            _telnetClient.Send(_flushStr);
        }

        /// <summary>
        /// Tell OpenVPN your username
        /// </summary>
        /// <param name="username"></param>
        public void SetAuthUsername(string username)
        {
            _telnetClient.Send($"username Auth \"{Escape(username)}\"");
            Thread.Sleep(200);
            _telnetClient.Send(_flushStr);
        }

        /// <summary>
        /// Tell OpenVPN your private key
        /// </summary>
        /// <param name="privateKey"></param>
        public void SetPrivateKey(string privateKey)
        {
            _telnetClient.Send($"password \"Private Key\" \"{Escape(privateKey)}\"");
        }

        /// <summary>
        /// Tell OpenVPN to give us real-time status updates
        /// </summary>
        /// <param name="on"></param>
        public void SetState(bool on)
        {
            _telnetClient.Send("state " + (on ? "on" : "off"));
        }

        /// <summary>
        /// Send OpenVPN the SITERM message to let it know we're disconnecting
        /// </summary>
        public void SigTerm()
        {
            _telnetClient.Send("signal SIGTERM");
        }

        string trimType(string message)
        {
            var i = message.IndexOf(':');
            return message.Substring(i+1).Trim();
        }
        
        protected virtual void OnNeedUsernameAndPassword(PasswordArgs args)
        {
            if (_isDisposing)
                return;
            UsernamePasswordRequired?.Invoke(this, args);
            SetAuthPassword(args.Password);
            SetAuthUsername(args.Username);
        }

        protected virtual void OnNeedPrivateKey(PasswordArgs args)
        {
            if (_isDisposing)
                return;
            PrivateKeyRequired?.Invoke(this, args);
            SetPrivateKey(args.PrivateKey);
        }

        private bool _isDisposing;

        public void Dispose()
        {
            if (_isDisposing)
            {
                return;
            }
            _isDisposing = true;
            SigTerm();
            var maxWait = 10000;
            var ellapsed = 0;
            var wait = 100;
            while (!ReceivedExitNotification && ellapsed < maxWait && _telnetClient.Connected)
            {
                Sleeper.Sleep(wait);
                ellapsed += wait;
            }
            _telnetClient.Dispose();
            Disconnected?.Invoke(this, EventArgs.Empty);
        }
    }

    public class MessageArgs : EventArgs
    {
        public string Message { get; set; }
        /// <summary>
        /// Null for no error.
        /// </summary>
        public Exception Error { get; set; }
    }

    public class ByteCountArgs : EventArgs
    {
        public long BytesIn { get; set; }
        public long BytesOut { get; set; }

        internal static ByteCountArgs FromString(string str)
        {
            var args = new ByteCountArgs();
            var parts = str.Split(',');
            args.BytesIn = long.Parse(parts[0]);
            args.BytesOut = long.Parse(parts[1]);
            return args;
        }
    }
    
    public class LogArgs : EventArgs
    {
        public DateTime DateTime { get; set; }
        public string LogMessage { get; set; }
        public LogType LogFlags { get; set; }


        internal static LogArgs FromString(string str)
        {
            var args = new LogArgs();
            var parts = str.Split(',');
            int unixTime = int.Parse(parts[0]);
            args.DateTime = unixTime.ToDateTime();
            foreach (var flags in parts[1])
            {
                switch (flags)
                {
                    case 'I':
                        args.LogFlags |= LogType.Informational;
                        break;
                    case 'F':
                        args.LogFlags |= LogType.FatalError;
                        break;
                    case 'N':
                        args.LogFlags |= LogType.NonFatalError;
                        break;
                    case 'W':
                        args.LogFlags |= LogType.Warning;
                        break;
                }
            }
            args.LogMessage = parts[2];
            return args;
        }
    }

    public class EchoArgs: EventArgs
    {
        public DateTime DateTime { get; set; }
        public string Command { get; set; }

        internal static EchoArgs FromString(string str)
        {
            var args = new EchoArgs();
            var parts = str.Split(',');
            var unixTime = int.Parse(parts[0]);
            args.DateTime = unixTime.ToDateTime();
            args.Command = parts[1];
            return args;
        }
    }

    public class StateArgs : EventArgs
    {
        public DateTime DateTime { get; set; }
        public OVPNState State { get; set; }
        public string Description { get; set; }
        public string LocalIP { get; set; }
        public string RemoteIP { get; set; }

        public static StateArgs FromString(string str)
        {
            var args = new StateArgs();
            var parts = str.Split(',');
            var unixTime = int.Parse(parts[0]);
            args.DateTime = unixTime.ToDateTime();
            args.State = (OVPNState)Enum.Parse(typeof(OVPNState), parts[1]);
            if (parts.Length >= 3)
                args.Description = parts[2];
            if (parts.Length >= 4)
                args.LocalIP = parts[3];
            if (parts.Length >= 5)
                args.RemoteIP = parts[4];
            return args;
        }
    }

    public class PasswordArgs : EventArgs
    {
        /// <summary>
        /// True if OpenVPN needs your username and password
        /// </summary>
        public bool NeedUsernamePassword { get; private set; }

        public bool NeedPrivateKey { get; private set; }
        /// <summary>
        /// Username to pass back to openvpn
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Password to pass back to openvpn
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Private key to pass back to openvpn
        /// </summary>
        public string PrivateKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string VerificationFailureMessage { get; private set; }

        internal static PasswordArgs FromString(string str)
        {
            str = str.TrimStart(':');
            var args = new PasswordArgs();
            var parts = str.Split(' ');
            if (str.StartsWith("need", StringComparison.OrdinalIgnoreCase))
            {
                switch (parts[1])
                {
                    case "'Auth'":
                        args.NeedUsernamePassword = true;
                        break;
                    case "'Private Key'":
                        args.NeedPrivateKey = true;
                        break;
                }
            }
            else if (str.StartsWith("verification failed", StringComparison.OrdinalIgnoreCase))
            {
                args.VerificationFailureMessage = L._("Authentication failure during connection attempt.");
            }

            return args;
        }
    }

    [Flags]
    public enum LogType
    {
        Informational = 1 >> 0,
        FatalError = 1 >> 2,
        NonFatalError = 1 >> 4,
        Warning = 1 >> 8,
        Debug = 1 >> 16
    }

    public enum OVPNState
    {
        RESOLVE,
        CONNECTING,
        WAIT,
        AUTH,
        GET_CONFIG,
        ASSIGN_IP,
        ADD_ROUTES,
        CONNECTED,
        RECONNECTING,
        EXITING,
        TCP_CONNECT,
    }
}
