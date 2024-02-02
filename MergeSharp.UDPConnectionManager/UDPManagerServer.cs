using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace MergeSharp.UDPConnectionManager;

class UDPManagerServer
{
    private UDPNode self;
    private CancellationTokenSource cts;
    private udpConnectionManager connectionManager;
    UdpClient listener;

    public UDPManagerServer(UDPNode self, udpConnectionManager connectionManager)
    {
        this.self = self;
        this.connectionManager = connectionManager;
    }

    public void Run()
    {
        cts = new CancellationTokenSource();
        CancellationToken ct = cts.Token;
        listener = new UdpClient(self.port);

        Task.Run(async () =>
        {
        while (!ct.IsCancellationRequested)
            {
                try
                {
                    var receivedResult = await listener.ReceiveAsync();
                    if (self.receiveMessages)
                    {
                        using (var ms = new MemoryStream(receivedResult.Buffer))
                        {
                            var syncMsg = Serializer.DeserializeWithLengthPrefix<NetworkProtocol>(ms, PrefixStyle.Base128);
                            if (syncMsg != null)
                            {
                                connectionManager.ExecuteSyncMessage(syncMsg);
                            }
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    // Listener has been disposed, stopping the loop
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }, ct);
    }

    public void Stop()
    {
        cts.Cancel();
        listener.Close();
    }
}