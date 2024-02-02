using System.IO;
using System.Net;
using System.Net.Sockets;
using ProtoBuf;

namespace MergeSharp.UDPConnectionManager;

public class UDPNode
{
    public int id { get; private set; }
    public IPAddress ip { get; private set; }
    public int port { get; private set; }
    public UdpClient udpSession { get; private set; }
    public bool isSelf;
    public bool connected { get; private set; }
    public bool receiveMessages { get; private set; }

    public UDPNode(string ip, int port, bool isSelf = false)
    {
        this.ip = IPAddress.Parse(ip);
        this.port = port;
        this.isSelf = isSelf;
        this.connected = false;
        this.receiveMessages = true;
        this.udpSession = new UdpClient();
    }

    public void ToggleReceiveMessages()
    {
        receiveMessages = !receiveMessages;
    }

    public void Send(NetworkProtocol msg)
    {
        using (var ms = new MemoryStream())
        {
            Serializer.SerializeWithLengthPrefix(ms, msg, PrefixStyle.Base128);
            byte[] data = ms.ToArray();
            udpSession.Send(data, data.Length, new IPEndPoint(ip, port));
        }
    }
}
