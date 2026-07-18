using XboxEOS.EventArgs;

namespace XboxEOS.Services;

public interface IOSCEOSService
{
    event EventHandler<PanTiltChangedEventArgs> PanTiltChanged;
    event EventHandler<OSCMessageSentEventArgs> OSCMessageSent;


    void Connect();
    void Disconnect();
    void SendButton(string key);
    void SendEdgeWheel(bool isCoarse = false, float amount = 0);
    void SendFanButton();
    void SendIrisWheel(bool isCoarse = false, float amount = 0);
    void SendNextButton();
    void SendPanWheel(bool isCoarse = false, float amount = 0);
    void SendPrevButton();
    void SendRemDimButton();
    void SendTiltWheel(bool isCoarse = false, float amount = 0);
    void SendZoomWheel(bool isCourse = false, float amount = 0);
}