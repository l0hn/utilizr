namespace Utilizr.Vpn
{
    public class AutoDialer
    {
        private bool _abort = false;
        private IVpnController _vpnController;
        public List<ConnectionType> ConnectionTypes { get; set; }

        public event ConnectionStateHandler DialStepFailed;

        public AutoDialer(IVpnController controller, List<ConnectionType>? connectionTypesAllowed = null)
        {
            _vpnController = controller;
            ConnectionTypes = connectionTypesAllowed ?? controller.GetAvailableProtocols().ToList();
        }

        public Task BeginAutoDial(IConnectionStartParams startParams)
        {
            return Task.Run(() => AutoDial(startParams));
        }
        
        public void AbortAutoDial()
        {
            _abort = true;
            _vpnController.DisconnectAsync();
        }

        async void AutoDial(IConnectionStartParams startParams)
        {
            _abort = false;
            _vpnController.SupressErrors = true;
            try
            {
                foreach (var connectionType in ConnectionTypes)
                {
                    if (_abort)
                        break;

                    try
                    {
                        await _vpnController.ConnectAsync(startParams);
                        if (_vpnController.IsConnected)
                        {
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!_abort)
                            OnDialStepFailed(startParams.Hostname, ex, startParams.Context);
                    }
                }
                if (!_abort)
                    _vpnController.RaiseLastError();
            }
            finally
            {
                _vpnController.SupressErrors = false;
                if (_abort)
                {
                    throw new OperationCanceledException("Operation was cancelled");
                }
                if (_vpnController.LastError != null)
                {
                    throw _vpnController.LastError;
                }
            }
        }

        protected virtual void OnDialStepFailed(string host, Exception ex, object? userContext=null)
        {
            DialStepFailed?.Invoke(this, host, ex, userContext);
        }
    }
}
