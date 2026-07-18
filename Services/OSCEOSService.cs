using FastOSC;
using System.Net;
using System.Text;
using XboxEOS.EventArgs;

namespace XboxEOS.Services;

internal class OSCEOSService : IOSCEOSService, IDisposable
{
    public event EventHandler<PanTiltChangedEventArgs> PanTiltChanged;
    public event EventHandler<OSCMessageSentEventArgs> OSCMessageSent;

    bool isConnected = false;

    //OSCReceiver? receiver = null;
    OSCSender? sender = null;

    public void Dispose()
    {
        //if (receiver != null)
        //{
        //    receiver.DisconnectAsync().ContinueWith((result) => Console.WriteLine("Received Disconnected"));
        //    receiver.OnPacketReceived -= HandlePacketReceivedAsync;
        //}

        if (sender != null)
        {
            sender.Disconnect();
        }
    }

    public void Connect()
    {
        if (isConnected) return;
        isConnected = true;

        //receiver = new OSCReceiver();
        //receiver.OnPacketReceived += HandlePacketReceivedAsync;
        //receiver.Connect(new IPEndPoint(IPAddress.Loopback, 8001));

        sender = new OSCSender();
        sender.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 8000));
    }

    public void Disconnect()
    {
        if (!isConnected) return;

        //receiver?.DisconnectAsync().Wait();
        //receiver?.OnPacketReceived -= HandlePacketReceivedAsync;
        //receiver = null;

        sender?.Disconnect();
        sender = null;

        isConnected = false;
    }

    private Task HandlePacketReceivedAsync(IOSCPacket packet)
    {
        //:-   /eos/out/pantilt = -270.000, 270.000, -135.000, 135.000, 0.000, 0.000
        Console.WriteLine("OSC Packet Received {0}", 0);
        return Task.CompletedTask;
    }

    public void SendNextButton() => SendButton(@"NEXT");

    public void SendPrevButton() => SendButton(@"LAST");

    public void SendRemDimButton()
    {
        SendButton(@"Rem_Dim");
        SendButton(@"Enter");
    }

    public void SendFanButton() => SendButton(@"Fan_");

    public void SendButton(string key)
    {
        if (!isConnected) return;

        var address = $"/eos/key/{key}";

        var message = new OSCMessage(address, 1);

        sender?.Send(message);
        OnOSCMessageSent(address, 1);

        Task.Delay(20);

        var message2 = new OSCMessage(address, 0);

        sender?.Send(message2);
        OnOSCMessageSent(address, 0);
    }

    public void SendPanWheel(bool isCoarse = false, float amount = 0.0f) => SendWheel(@"pan", isCoarse, amount);

    public void SendTiltWheel(bool isCoarse = false, float amount = 0.0f) => SendWheel(@"tilt", isCoarse, amount);

    public void SendZoomWheel(bool isCoarse = false, float amount = 0.0f) => SendWheel(@"Zoom", isCoarse, amount);

    public void SendEdgeWheel(bool isCoarse = false, float amount = 0.0f) => SendWheel(@"Edge", isCoarse, amount);

    public void SendIrisWheel(bool isCoarse = false, float amount = 0.0f) => SendWheel(@"Iris", isCoarse, amount);

    public void SendWheel(string wheelName, bool isCoarse = false, float amount = 0.0f)
    {
        if (!isConnected) return;

        StringBuilder address = new StringBuilder("/eos/wheel");

        if (isCoarse)
        {
            address.Append("/coarse");
        }
        else
        {
            address.Append("/fine");
        }

        address.AppendFormat("/{0}", wheelName);

        var message = new OSCMessage(address.ToString(), amount);

        sender?.Send(message);

        OnOSCMessageSent(address.ToString(), amount);
    }

    private void OnOSCMessageSent(string address, params object[] args)
    {
        OSCMessageSent?.Invoke(this, new OSCMessageSentEventArgs() { Address = address.ToString(), Data = args.Select(a=>a.ToString()).ToArray() });
    }
}
