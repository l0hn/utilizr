using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using DotRas;
using NUnit.Framework;
using Utilizr;
using Utilizr.Info;
using Utilizr.VPN;
using Utilizr.VPN.Win;
using Utilizr.IPC;
using Utilizr.OpenVPN.ipc;
using Utilizr.VPN.Win.Providers;
using System.Threading;
using System;
using System.Diagnostics;
using Utilizr.Async;
using Utilizr.VPN.Providers;

namespace Tests.VPN
{
    //TODO: Test userContext is correct for all events
    //TODO: Test that multiple disconnected events do not continue occuring after connecting to more than 1 server.

    /// <summary>
    /// NOTE: to run these tests you need to place a text file in the application dir named "vpn_test_credentials.txt"
    /// With 3 lines as follows:
    /// hostname
    /// username
    /// password
    /// (for obvious reasons no vpn credentials are stored in the repo)
    /// 
    /// For OpenVPN, place a valid certificate in the application dir named ca.crt
    /// For OpenVPN, plave a valid .ovpn config file in the application dir named test_config.ovpn
    /// </summary>
    [TestFixture(Category = "VPN")]
    public class VPNTests
    {
        private string hostname;
        private string username;
        private string password;
        private IVPNController controller;
        private AutoDialer autoDialer;

        [SetUp]
        public void Setup()
        {
            LoadCredentials();
        }

        [TearDown]
        public void TearDown()
        {
            autoDialer?.AbortAutoDial();
            controller.EndDisconnectAsync(controller.DisconnectAsync());
            controller.Dispose();
        }

        [Test]
        public void ConnectPPTPTest()
        {
            ConnectRas(ConnectionType.PPTP);
        }

        [Test]
        public void ConnectSSTPTest()
        {
            ConnectRas(ConnectionType.SSTP);
        }

        [Test]
        public void ConnectCiscoIPSecTest()
        {
            ConnectRas(ConnectionType.CISCO_IPSEC);
        }

        [Test]
        public void ConnectIKEV2Test()
        {
            ConnectRas(ConnectionType.IKEV2);
        }

        [Test]
        public void ConnectL2TPTest()
        {
            ConnectRas(ConnectionType.L2TP_IPSEC);
        }

        [Test]
        public void ConnectOpenVPNTest()
        {
            ///NOTE:
            ///Open vpn requires both user credentials file as mentioned above, and a valid certificate named 'ca.crt'
           
            //OpenVPN requires service 
            SingletonServiceHelper<WCFService, IWCFService>.StartServer(WCFService.ADDRESS);

            var connectedEventRaised = false;
            var connectingEventRaised = false;
            var disconnectingEventRaised = false;
            var disconnectedEventRaised = false;

            var providerConfig =
                OpenVpnProviderConfig.FromCertificateHandler(() => Path.Combine(AppInfo.AppDirectory, "ca.crt"));

            controller = new VPNController(
                 (connectionType) => new UserPass(username, password.ToSecureString()),
                 true,
                 new OpenVPNProvider(providerConfig, new WindowsOpenVPNHelper()),
                 new RasVPNProvider("Utilizr VPN Test", () => ""));

            controller.Connected += (sender, host, error, context) => connectedEventRaised = true;
            controller.Connecting += (sender, host, error, context) => connectingEventRaised = true;
            controller.Disconnecting += (sender, host, error, context) => disconnectingEventRaised = true;
            controller.Disconnected += (sender, host, error, context) => disconnectedEventRaised = true;

            var ar = controller.ConnectAsync(new OpenVpnConnectionStartParams(hostname, null));
            controller.EndConnectAsync(ar);

            Assert.True(controller.IsConnected);
            Assert.True(connectedEventRaised);
            Assert.True(connectingEventRaised);

            controller.EndDisconnectAsync(controller.DisconnectAsync());

            Assert.False(controller.IsConnected);
            Assert.True(disconnectingEventRaised);
            Assert.True(disconnectedEventRaised);

        }

        [Test]
        public void OpenVPNCorruptCertTest()
        {
            var corruptCert = "isuoiuansalisunfpasionaoisgn";
            var tempFile = Path.Combine(AppInfo.AppDirectory, "corrupt.ca");

            File.WriteAllText(tempFile, corruptCert);

            //OpenVPN requires service 
            SingletonServiceHelper<WCFService, IWCFService>.StartServer(WCFService.ADDRESS);

            var connectedEventRaised = false;
            var connectingEventRaised = false;
            var disconnectedEventRaised = false;
            var errorRaised = false;
            Exception exception = null;

            var providerConfig = OpenVpnProviderConfig.FromCertificateHandler(() => tempFile);

            controller = new VPNController(
                 (connectionType) => new UserPass(username, password.ToSecureString()),
                 true,
                 new OpenVPNProvider(providerConfig, new WindowsOpenVPNHelper()),
                 new RasVPNProvider("Utilizr VPN Test", () => ""));

            controller.Connected += (sender, host, error, context) => connectedEventRaised = true;
            controller.Connecting += (sender, host, error, context) => connectingEventRaised = true;
            controller.Disconnected += (sender, host, error, context) => disconnectedEventRaised = true;
            controller.ConnectError += (sender, host, error, context) => errorRaised = true;

            try
            {
                var ar = controller.ConnectAsync(new OpenVpnConnectionStartParams(hostname, null));
                controller.EndConnectAsync(ar);

            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsInstanceOf(typeof(OpenVPNException), exception);

            Assert.False(controller.IsConnected);
            Assert.True(errorRaised);
            Assert.True(connectingEventRaised);

            Assert.True(disconnectedEventRaised);
            File.Delete(tempFile);
            
        }

        [Test]
        public void OpenVPNNoCertTest()
        {
            ///NOTE:
            ///Open vpn requires both user credentials file as mentioned above, and a valid certificate named 'ca.crt'

            //OpenVPN requires service 
            SingletonServiceHelper<WCFService, IWCFService>.StartServer(WCFService.ADDRESS);

            Exception exception = null;

            var providerConfig = OpenVpnProviderConfig.FromCertificateHandler(() => "");

            controller = new VPNController(
                 (connectionType) => new UserPass(username, password.ToSecureString()),
                 true,
                 new OpenVPNProvider(providerConfig, new WindowsOpenVPNHelper()),
                 new RasVPNProvider("Utilizr VPN Test", () => ""));

            try
            {
                var ar = controller.ConnectAsync(new OpenVpnConnectionStartParams(hostname, null));
                controller.EndConnectAsync(ar);

            } catch (Exception e)
            {
                exception = e;
            }

            Assert.IsInstanceOf(typeof(FileNotFoundException), exception);
            Assert.False(controller.IsConnected);
      


        }

        [Test]
        public void OpenVPNConfigFileConnect()
        {
            SingletonServiceHelper<WCFService, IWCFService>.StartServer(WCFService.ADDRESS);

            var providerConfig = OpenVpnProviderConfig.FromConfigFileHandler(() => Path.Combine(AppInfo.AppDirectory, "test_config.ovpn"));
            controller = new VPNController(
                (connectionType) => new UserPass(username, password.ToSecureString()),
                true,
                new OpenVPNProvider(providerConfig, new WindowsOpenVPNHelper()));

            controller.BandwidthUpdated += (sender, usage) =>
            {
                Debug.WriteLine($"BANDWIDTH: {usage.RxBytesPerSecond:N0} {usage.TxBytesPerSecond:N0}");
            };

            var ar = controller.ConnectAsync(new OpenVpnConnectionStartParams(hostname, null));
            controller.EndConnectAsync(ar);
        }


        [Test]
        public void TestProtocolNotSupported()
        {

            controller = new VPNController(
                (connectionType) => new UserPass(username, password.ToSecureString()),
                true,
                new RasVPNProvider("Utilizr VPN Test", () => "")
                //don't add openvpn provider
                );
    
            Assert.Throws(typeof (System.NotSupportedException), () =>
            {
                var ar = controller.ConnectAsync(new OpenVpnConnectionStartParams(hostname, null));
                controller.EndConnectAsync(ar);
            });

        }


        private void ConnectRas(ConnectionType type)
        {
            var connectedEventRaised = false;
            var connectingEventRaised = false;
            var disconnectingEventRaised = false;
            var disconnectedEventRaised = false;

            controller = new VPNController(
                 (connectionType) => new UserPass(username, password.ToSecureString()),
                 true,
                 new RasVPNProvider("Utilizr VPN Test", ()=>""));

            controller.Connected += (sender, host, error, context) => connectedEventRaised = true;
            controller.Connecting += (sender, host, error, context) => connectingEventRaised = true;
            controller.Disconnecting += (sender, host, error, context) => disconnectingEventRaised = true;
            controller.Disconnected += (sender, host, error, context) => disconnectedEventRaised = true;

            var ar = controller.ConnectAsync(new OpenVpnConnectionStartParams(hostname, null));
            controller.EndConnectAsync(ar);

            Assert.True(controller.IsConnected);
            Assert.True(connectedEventRaised);
            Assert.True(connectingEventRaised);

            controller.EndDisconnectAsync(controller.DisconnectAsync());

            Assert.False(controller.IsConnected);
            Assert.True(disconnectingEventRaised);
            Assert.True(disconnectedEventRaised);
        }

        [Test]
        public void ErrorSupressionTest()
        {
            var connectedEventRaised = false;
            var connectingEventRaised = false;
            var disconnectingEventRaised = false;
            var disconnectedEventRaised = false;
            var errorEventRaised = false;

            var cont = new VPNController(
                (connectionType) => new UserPass("somemadeupuser", "somemadeuppassword".ToSecureString()),
                true,
                new RasVPNProvider("Utilizr VPN Test", () => ""));

            controller.Connected += (sender, host, error, context) => connectedEventRaised = true;
            controller.Connecting += (sender, host, error, context) => connectingEventRaised = true;
            controller.Disconnecting += (sender, host, error, context) => disconnectingEventRaised = true;
            controller.Disconnected += (sender, host, error, context) => disconnectedEventRaised = true;
            controller.ConnectError += (sender, host, error, context) => errorEventRaised = true;

            controller.SupressErrors = true;
            var ar = controller.ConnectAsync(new OpenVpnConnectionStartParams(hostname, null));
            Assert.Throws(typeof (RasDialException), () => controller.EndConnectAsync(ar));

            Assert.False(controller.IsConnected);
            Assert.False(connectedEventRaised);
            Assert.True(connectingEventRaised);

            controller.EndDisconnectAsync(controller.DisconnectAsync());

            Assert.False(controller.IsConnected);
            Assert.False(disconnectingEventRaised);
            Assert.False(disconnectedEventRaised);
            Assert.False(errorEventRaised);
            
            controller.RaiseLastError();
            Assert.True(errorEventRaised);
        }

        [Test]
        public void TestAutoDialerConnectSuccess()
        {
            var connectedEventRaised = false;
            var connectingEventRaised = false;
            var disconnectingEventRaised = false;
            var disconnectedEventRaised = false;
            var errorEventRaised = false;
            var dialerStepFailureCount = 0;

            controller = new VPNController(
                (connectionType) => new UserPass(username, password.ToSecureString()),
                true,
                new RasVPNProvider("Utilizr VPN Test", ()=>""));

            controller.Connected += (sender, host, error, context) => connectedEventRaised = true;
            controller.Connecting += (sender, host, error, context) => connectingEventRaised = true;
            controller.Disconnecting += (sender, host, error, context) => disconnectingEventRaised = true;
            controller.Disconnected += (sender, host, error, context) => disconnectedEventRaised = true;
            controller.ConnectError += (sender, host, error, context) => errorEventRaised = true;

            autoDialer = new AutoDialer(controller, new List<ConnectionType>() { ConnectionType.SSTP, ConnectionType.L2TP_IPSEC, ConnectionType.PPTP });
            autoDialer.DialStepFailed += (sender, host, error, context) => dialerStepFailureCount++;

            autoDialer.EndAutoDial(autoDialer.BeginAutoDial(new OpenVpnConnectionStartParams(hostname, null)));

            Assert.True(controller.IsConnected);
            Assert.True(connectedEventRaised);
            Assert.True(connectingEventRaised);

            controller.EndDisconnectAsync(controller.DisconnectAsync());

            Assert.False(controller.IsConnected);
            Assert.True(disconnectingEventRaised);
            Assert.True(disconnectedEventRaised);
            Assert.False(errorEventRaised);

            controller.RaiseLastError();
            Assert.False(errorEventRaised);

            Assert.AreEqual(2, dialerStepFailureCount);
        }

        [Test]
        public void TestAutoDialerFail()
        {
            var connectedEventRaised = false;
            var connectingEventRaised = false;
            var disconnectingEventRaised = false;
            var disconnectedEventRaised = false;
            var errorEventRaised = false;
            var dialerStepFailureCount = 0;

            controller = new VPNController(
                (connectionType) => new UserPass("somemadeupusername", "somenonextentpassword".ToSecureString()),
                true,
                new RasVPNProvider("Utilizr VPN Test", () => ""));

            controller.Connected += (sender, host, error, context) => connectedEventRaised = true;
            controller.Connecting += (sender, host, error, context) => connectingEventRaised = true;
            controller.Disconnecting += (sender, host, error, context) => disconnectingEventRaised = true;
            controller.Disconnected += (sender, host, error, context) => disconnectedEventRaised = true;
            controller.ConnectError += (sender, host, error, context) => errorEventRaised = true;

            var dialer = new AutoDialer(controller, new List<ConnectionType>() { ConnectionType.SSTP, ConnectionType.L2TP_IPSEC, ConnectionType.PPTP });
            dialer.DialStepFailed += (sender, host, error, context) => dialerStepFailureCount++;

            var ar = dialer.BeginAutoDial(new OpenVpnConnectionStartParams(hostname, null));
            Assert.Throws(typeof(RasDialException), () => dialer.EndAutoDial(ar));

            Assert.False(controller.IsConnected);
            Assert.False(connectedEventRaised);
            Assert.True(connectingEventRaised);

            controller.EndDisconnectAsync(controller.DisconnectAsync());

            Assert.False(controller.IsConnected);
            Assert.False(disconnectingEventRaised);
            Assert.False(disconnectedEventRaised);
            Assert.True(errorEventRaised);

            Assert.AreEqual(3, dialerStepFailureCount);
        }

        void LoadCredentials()
        {
            //Load hostname and credentials from file in appfolder
            var lines = File.ReadAllLines(Path.Combine(AppInfo.AppDirectory, "vpn_test_credentials.txt"));
            hostname = lines[0];
            username = lines[1];
            password = lines[2];
        }
    }
}
