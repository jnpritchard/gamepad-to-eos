using SharpDX.DirectInput;
using System.Reflection.Metadata.Ecma335;

namespace XboxEOS.Services;

public sealed class InputService : IInputService
{
    private bool ContinueLoop { get; set; } = false;

    public event EventHandler<JoystickUpdate>? JoystickUpdated;

    public IList<DeviceInstance> GetInputDevices()
    {
        // Initialize DirectInput
        var directInput = new DirectInput();

        var devices = directInput.GetDevices(DeviceType.Gamepad | DeviceType.Joystick,
                    DeviceEnumerationFlags.AllDevices);

        foreach(var device in devices) {
            Console.WriteLine($"{device.ProductName} : {device.InstanceName}");
        }

        return devices;
    }

    public void GetInfo(Guid inputGuid)
    {
        // Initialize DirectInput
        var directInput = new DirectInput();

        // If Joystick not found, throws an error
        if (inputGuid == Guid.Empty)
        {
            throw new Exception("No joystick/Gamepad found.");
        }

        // Instantiate the joystick
        var joystick = new Joystick(directInput, inputGuid);

        Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", inputGuid);

        Console.WriteLine("Joystick/Gamepad Name: {0}", joystick.Information.ProductName);
        Console.WriteLine("Joystick/Gamepad Type: {0}", joystick.Information.Type);
        Console.WriteLine("Joystick/Gamepad SubType: {0}", joystick.Information.Subtype);
        Console.WriteLine("Joystick/Gamepad Axis Count: {0}", joystick.Capabilities.AxeCount);
        Console.WriteLine("Range: {0} - {1}", joystick.Properties.Range.Minimum, joystick.Properties.Range.Maximum);
        Console.WriteLine("Joystick/Gamepad Button Count: {0}", joystick.Capabilities.ButtonCount);
        Console.WriteLine("Joystick/Gamepad POV Count: {0}", joystick.Capabilities.PovCount);
        Console.WriteLine("Joystick/Gamepad Force Feedback: {0}", joystick.Capabilities.Flags.HasFlag(DeviceFlags.ForceFeedback) ? "Yes" : "No");

        // Query all suported ForceFeedback effects
        var allEffects = joystick.GetEffects();
        foreach (var effectInfo in allEffects)
            Console.WriteLine("Effect available {0}", effectInfo.Name);

    }


    public Task BeginAsync(Guid joystickGuid, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            // Initialize DirectInput
            var directInput = new DirectInput();

            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty)
            {
                throw new Exception("No joystick/Gamepad found.");
            }

            // Instantiate the joystick
            var joystick = new Joystick(directInput, joystickGuid);

            // Set BufferSize in order to use buffered data.
            joystick.Properties.BufferSize = 128;

            // Acquire the joystick
            joystick.Acquire();

            ContinueLoop = true;

            // Poll events from joystick
            while (ContinueLoop)
            {
                joystick.Poll();
                var datas = joystick.GetBufferedData();
                foreach (var state in datas)
                {
                    Console.WriteLine(state);
                    JoystickUpdated?.Invoke(this, state);                
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Cancellation requested. Exiting...");
                    break;
                }
            }
        });

    }

    public void Stop()
    {
        ContinueLoop = false;
    }
}