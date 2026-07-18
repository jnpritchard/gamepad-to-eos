using SharpDX.DirectInput;

namespace XboxEOS.Services;

public interface IInputService
{
    event EventHandler<JoystickUpdate> JoystickUpdated;

    IList<DeviceInstance> GetInputDevices();
    void GetInfo(Guid inputGuid);
    Task BeginAsync(Guid inputGuid, CancellationToken cancellationToken);
    void Stop();
}