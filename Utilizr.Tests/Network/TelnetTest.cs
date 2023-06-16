using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Utilizr.Network;

namespace Utilizr.Tests.Network
{
    [TestFixture]
    public class TelnetTest
    {
        [Test]
        public async Task TestTelnet()
        {
            var serverDone = new ManualResetEvent(false);
            var clientDone = new ManualResetEvent(false);

            var messages = new[]
            {
                "test1",
                "test2",
                ">YOUR MUM?",
                "test3",
                "fh3489fh3895yt849gh45g894h9gh589h489h",
            };

            var routedMessages = new[]
            {
                "StaRTsWith: yo momma",
                "TEST: yo momme ENdSWIth",
                "TEST: CoNtAiNS"
            };

            var clientRecievedRoutedMessages = new List<string>();

            //create a server
            var port = NetUtil.GetAvailablePort(30000, 40000);
            var server = new TcpListener(new IPEndPoint(IPAddress.Any, port));
            var client = new TelnetClient("localhost", port);
            var clientRecievedMessages = new List<string>();
            var serverRecievedMessages = new List<string>();
            client.LineReceieved += (sender, args) =>
            {
                clientRecievedMessages.Add(args.Message);
                if (clientRecievedMessages.Count == messages.Length)
                {
                    clientDone.Set();
                }
                if (args.Message.StartsWith(">your mum", StringComparison.OrdinalIgnoreCase))
                {
                    args.ResponseMessage = "yes please";
                }
            };
            client.Router.AddHandler("startswith:", args =>
            {
                clientRecievedRoutedMessages.Add(args.Message);
            });
            client.Router.AddHandler("endswith", args =>
            {
                clientRecievedRoutedMessages.Add(args.Message);
            }, endswith:true);
            client.Router.AddHandler("contains", args =>
            {
                clientRecievedRoutedMessages.Add(args.Message);
            }, contains:true);
            server.Start();
            server.BeginAcceptTcpClient(ar =>
            {
                var c = server.EndAcceptTcpClient(ar);
                var serverReader = new StreamReader(c.GetStream());
                var serverWritter = new StreamWriter(c.GetStream());

                Task.Run(() =>
                {
                    while (true)
                    {
                        var message = serverReader.ReadLine();
                        serverRecievedMessages.Add(message);
                        //+1 as we're expecting a response to a message
                        if (serverRecievedMessages.Count == messages.Length+1)
                        {
                            serverDone.Set();
                            return;
                        }
                    }
                });

                var sendMessages = new List<string>();
                sendMessages.AddRange(messages);
                sendMessages.AddRange(routedMessages);
                //send messages to the client
                foreach (var message in sendMessages)
                {
                    serverWritter.WriteLine(message);
                    serverWritter.Flush();
                }
                
            },null);

            await client.Connect();
            foreach (var message in messages)
            {
                client.Send(message);
            }

            //now wait
            WaitHandle.WaitAll(new WaitHandle[]
            {
                serverDone,
                clientDone
            }, 20000);

            //check it all worked
            foreach (var message in messages)
            {
                Assert.Contains(message, clientRecievedMessages);
                Assert.Contains(message, serverRecievedMessages);
            }

            //check the routed messages worked
            foreach (var routedMessage in routedMessages)
            {
                Assert.Contains(routedMessage, clientRecievedRoutedMessages);
            }

            //server should have recieved an extra message
            Assert.Contains("yes please", serverRecievedMessages);

            //cleanup
            client.Disconnect();
            server.Stop();
        }
    }
}
