using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilizr.Network
{
    public delegate void ErrorHandler(object sender, Exception ex);

    /// <summary>
    /// Simple async telnet client
    /// </summary>
    public class TelnetClient : IDisposable
    {
        /// <summary>
        /// Fired when a line is recieved from the server.
        /// </summary>
        public event EventHandler<LineRecievedEventArgs>? LineReceieved;
        public event ErrorHandler? Error;
        public event EventHandler? Disconnected;

        private readonly TcpClient _tcpClient;
        private Stream? _tcpStream;
        private readonly ManualResetEvent _sendReady;
        private readonly Queue<string> _sendQueue;
        private readonly ManualResetEvent _receiveReady;
        private readonly Queue<string> _receiveQueue;
        private readonly object WRITE_LOCK = new();
        private readonly object READ_LOCK = new();
        private readonly string _host;
        private readonly int _port;
        private bool _connected = false;
        private bool _isDisposing;

        public bool Connected => _connected;
        public MessageRouter Router { get; private set; }

        /// <summary>
        /// Will cause a line received event if an exact match is received without a newline
        /// </summary>
        public List<string> MagicPhrases { get; }

        public TelnetClient(string host, int port, int sendTimeout = 0, int receiveTimeout = 0)
        {
            Router = new MessageRouter(this);
            _sendReady = new ManualResetEvent(false);
            _sendQueue = new Queue<string>();
            _receiveReady = new ManualResetEvent(false);
            _receiveQueue = new Queue<string>();
            _host = host;
            _port = port;
            _tcpClient = new TcpClient
            {
                SendTimeout = sendTimeout,
                ReceiveTimeout = receiveTimeout
            };
            MagicPhrases = new List<string>();
        }

        public Task Connect()
        {
            return Task.Run(() =>
            {
                _sendQueue.Clear();
                _receiveQueue.Clear();
                _tcpClient.Connect(_host, _port);
                _tcpStream = _tcpClient.GetStream();

                try
                {
                    Task.Factory.StartNew(SendLoop, TaskCreationOptions.LongRunning);
                    Task.Factory.StartNew(RecieveLoop, TaskCreationOptions.LongRunning);
                    Task.Factory.StartNew(RecieveProcessLoop, TaskCreationOptions.LongRunning);

                    _connected = true;
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            });
        }

        public void Disconnect()
        {
            _tcpClient.Close();
        }

        /// <summary>
        /// Queue a message to be sent
        /// </summary>
        /// <param name="line"></param>
        public void Send(string line)
        {
            lock (WRITE_LOCK)
            {
                _sendQueue.Enqueue(line);
                _sendReady.Set();
            }
        }

        void SendLoop()
        {
            while (_tcpClient.Connected)
            {
                _sendReady.WaitOne();
                lock (WRITE_LOCK)
                {
                    if (_sendQueue.Count <= 0)
                    {
                        _sendReady.Reset();
                        continue;
                    }
                    var nextLine = _sendQueue.Dequeue();
                    nextLine = nextLine.Trim('\r', '\n') + "\r\n";
                    var bytes = Encoding.UTF8.GetBytes(nextLine);
                    var ar = _tcpStream!.BeginWrite(bytes, 0, bytes.Length, null, null);
                    _tcpStream.EndWrite(ar);
                    _tcpStream.Flush();
                }
            }
        }

        void RecieveLoop()
        {
            try
            {
                int read;
                var buf = new byte[1 * 1024];
                string currentChunk = "";
                while (_tcpClient.Connected)
                {
                    read = _tcpStream!.EndRead(_tcpStream.BeginRead(buf, 0, buf.Length, null, null));
                    if (read <= 0)
                    {
                        continue;
                    }
                    currentChunk += Encoding.UTF8.GetString(buf, 0, read);

                    var i = 0;
                    var remainder = currentChunk.Length;
                    while ((i = currentChunk.IndexOf("\r\n")) > -1)
                    {
                        var line = currentChunk[..i];
                        lock (READ_LOCK)
                        {
                            _receiveQueue.Enqueue(line);
                            _receiveReady.Set();
                        }
                        remainder = currentChunk.Length - (i + "\r\n".Length);
                        currentChunk = currentChunk.Substring(currentChunk.Length - remainder, remainder);
                    }

                    //annoyginly we sometimes need to raise a line received event even if the message does not end with \r\n
                    if (MagicPhrases.Contains(currentChunk))
                    {
                        lock (READ_LOCK)
                        {
                            _receiveQueue.Enqueue(currentChunk);
                            _receiveReady.Set();
                        }
                        currentChunk = "";
                    }
                }
            }
            catch (Exception ex)
            {
                OnError(new Exception($"Terminal error, disconnecting... Unprocessed messages: send:{_sendQueue.Count}, recieve:{_receiveQueue.Count}", ex));
                throw;
            }
            finally
            {
                _sendReady.Set();
                _receiveReady.Set();
                OnDisconnected();
            }
        }

        void RecieveProcessLoop()
        {
            while (_tcpClient.Connected)
            {
                try
                {
                    _receiveReady.WaitOne();
                    string line = "";
                    lock (READ_LOCK)
                    {
                        if (_receiveQueue.Count == 0)
                        {
                            _receiveReady.Reset();
                            continue;
                        }
                        line = _receiveQueue.Dequeue();
                    }
                    if (string.IsNullOrEmpty(line))
                    {
                        //Log.Info("TELNET_READ_PROCESSING", "Skipping empty line");
                        continue;
                    }
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"processing {line}");
#endif 
                    Router.Process(new LineRecievedEventArgs(line));
                    OnLineRecieved(line);
                }
                catch (Exception ex)
                {
                    OnError(new Exception("TELNET_READ_PROCESSING", ex));
                }
            }
        }

        protected virtual void OnLineRecieved(string line)
        {
            var lineArgs = new LineRecievedEventArgs(line);
            LineReceieved?.Invoke(this, lineArgs);
            if (!string.IsNullOrEmpty(lineArgs.ResponseMessage))
            {
                Send(lineArgs.ResponseMessage);
            }
        }

        protected virtual void OnError(Exception ex)
        {
            Error?.Invoke(this, ex);
        }

        protected virtual void OnDisconnected()
        {
            _connected = false;
            Disconnected?.Invoke(this, new EventArgs());
        }

        public void Dispose()
        {
            if (_isDisposing)
                return;

            _isDisposing = true;

            try
            {
                _tcpClient?.Close();
                _tcpStream?.Dispose();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }

            GC.SuppressFinalize(this);
        }
    }

    public class LineRecievedEventArgs : EventArgs
    {
        /// <summary>
        /// Line recieved from the server
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Set this if you want to respond to the message
        /// </summary>
        public string? ResponseMessage { get; set; } = null;

        public LineRecievedEventArgs(string message)
        {
            Message = message;
        }

        internal void Reset()
        {
            ResponseMessage = null;
        }
    }

    public class MessageRouter
    {
        private readonly List<Handler> _handlers;
        private readonly TelnetClient _telnetClient;

        public MessageRouter(TelnetClient telnetClient)
        {
            _telnetClient = telnetClient;
            _handlers = new List<Handler>();
        }

        public void AddHandler(string stringToCompare,
            Action<LineRecievedEventArgs> handler,
            StringComparison comparison = StringComparison.OrdinalIgnoreCase,
            bool startswith = true,
            bool contains = false,
            bool endswith = false)
        {
            _handlers.Add(new Handler(comparison, stringToCompare, startswith, endswith, contains, handler));
        }

        public void Process(LineRecievedEventArgs args)
        {
            foreach (var handler in _handlers)
            {
                args.Reset();
                handler.ProcessMessage(args);

                if (string.IsNullOrEmpty(args.ResponseMessage))
                    continue;

                _telnetClient.Send(args.ResponseMessage);
            }
        }
    }

    internal class Handler
    {
        public bool StartsWith { get; set; }
        public bool Contains { get; set; }
        public bool EndsWith { get; set; }
        public StringComparison StringComparison { get; set; }
        public Action<LineRecievedEventArgs> Action { get; set; }
        public string StringToCompare { get; set; }

        internal Handler(
            StringComparison stringComparison,
            string stringToCompare,
            bool startswith,
            bool endswith,
            bool contains,
            Action<LineRecievedEventArgs> handler)
        {
            StringComparison = stringComparison;
            StringToCompare = stringToCompare;
            StartsWith = startswith; 
            EndsWith = endswith;
            Contains = contains;
            Action = handler;
        }

        public void ProcessMessage(LineRecievedEventArgs args)
        {
            var fire = (StartsWith && args.Message.StartsWith(StringToCompare, StringComparison)) ||
                       (EndsWith && args.Message.EndsWith(StringToCompare, StringComparison)) ||
                       (Contains && args.Message.IndexOf(StringToCompare, StringComparison) > -1);

            if (fire)
            {
                Action(args);
            }
        }
    }
}
