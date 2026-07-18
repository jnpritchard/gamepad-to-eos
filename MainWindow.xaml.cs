using SharpDX.DirectInput;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Windows;
using XboxEOS.Services;

namespace XboxEOS;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IInputService _inputService;
    private readonly IOSCEOSService _oSCEOSService;

    private readonly Timer? _timer = null;


    private float panDelta = 0f;
    private float tiltDelta = 0f;
    private float zoomDelta = 0f;
    private float edgeDelta = 0f;
    private float irisDelta = 0f;
    private double sensitivity = 0.2;

    private CancellationToken _cancellationToken = new();

    public MainWindow(IInputService inputService, IOSCEOSService oSCEOSService)
    {
        _inputService = inputService;
        _oSCEOSService = oSCEOSService;

        InitializeComponent();

        _timer = new Timer(timerCallback, null, 0, 200);

        var devices = _inputService.GetInputDevices();
        foreach (var device in devices)
        {
            listOfDevices.Items.Add(new ListItemDeviceViewModel { InstanceGuid = device.InstanceGuid, InstanceName = device.InstanceName });
        }

        _inputService.JoystickUpdated += this.OnJoystickUpdated;

        _oSCEOSService.OSCMessageSent += _oSCEOSService_OSCMessageSent;

        if (listOfDevices.Items.Count == 1)
        {
            listOfDevices.SelectedIndex = 0;
            StartMonitoring_Click(null, null);
        }


        Connect_Click(null, null);
    }

    private record ListItemDeviceViewModel()
    {
        public Guid InstanceGuid { get; init; }
        
        public required string InstanceName { get; init; }
    }

    private void StartMonitoring_Click(object sender, RoutedEventArgs e)
    {
        var selectedDevice = listOfDevices.SelectedItem as ListItemDeviceViewModel;
        if (selectedDevice == null)
        {
            MessageBox.Show("Please select a device from the list.");
            return;
        }

        _inputService.BeginAsync(selectedDevice.InstanceGuid, _cancellationToken);

        startMonitoringButton.IsEnabled = false;
        stopMonitoringButton.IsEnabled = true;
        listOfDevices.IsEnabled = false;
    }

    private void StopMonitoring_Click(object sender, RoutedEventArgs e)
    {
        _cancellationToken.ThrowIfCancellationRequested();
        _inputService.Stop();
        startMonitoringButton.IsEnabled = true;
        stopMonitoringButton.IsEnabled = false;
        listOfDevices.IsEnabled = true;
    }

    /// <summary>
    /// Event handler for joystick updates.
    /// </summary>    
    private void OnJoystickUpdated(object? sender, JoystickUpdate e)
    {
        // Update the UI with the joystick state
        Dispatcher.Invoke(() =>
        {
            joystickStateTextBlock.Text = $"Joystick State: {e}";

            switch (e.Offset)
            {
                case JoystickOffset.X:
                    xAxisIndicator.Value = e.Value;
                    var x = xAxisIndicator.Value - Int16.MaxValue;
                    if(Math.Abs(x) > 3600) // Null Zone
                    {
                        panDelta = (float)(x * this.sensitivity);
                    }
                    else
                    {
                        panDelta = 0f;
                    }

                    break;
                case JoystickOffset.Y:
                    yAxisIndicator.Value = e.Value;
                    var y = yAxisIndicator.Value - Int16.MaxValue;
                    if (Math.Abs(y) > 3600) // Null Zone
                    {
                        tiltDelta = (float)(y * this.sensitivity);
                    }
                    else
                    {
                        tiltDelta = 0f;
                    }
                    
                    break;
                case JoystickOffset.Z:
                    zAxisIndicator.Value = e.Value;
                    var z = zAxisIndicator.Value - Int16.MaxValue;
                    if(Math.Abs(z) > 3600)
                    {
                        zoomDelta = (float)(z * this.sensitivity)*-0.05f;
                    }
                    else
                    {
                        zoomDelta = 0f;
                    }
                    break;
                case JoystickOffset.RotationX:
                    xRotationAxisIndicator.Value = e.Value;
                    var xR = xRotationAxisIndicator.Value - Int16.MaxValue;
                    if (Math.Abs(xR) > 3600)
                    {
                        irisDelta = (float)(xR * this.sensitivity) * 0.01f;
                    }
                    else
                    {
                        irisDelta = 0f;
                    }
                    break;
                case JoystickOffset.RotationY:
                    yRotationAxisIndicator.Value = e.Value;
                    var yR = yRotationAxisIndicator.Value - Int16.MaxValue;
                    if (Math.Abs(yR) > 3600)
                    {
                        edgeDelta = (float)(yR * this.sensitivity) * 0.1f;
                    }
                    else
                    {
                        edgeDelta = 0f;
                    }
                    break;
                case JoystickOffset.Buttons0: // A
                    if (e.Value > 0)
                    {
                        button0Pressed();
                    }
                    else
                    {
                        button0Unpressed();
                    }
                    break;
                case JoystickOffset.Buttons1: // X
                    if (e.Value > 0)
                    {
                        button1Pressed();
                    }
                    else
                    {
                        button1Unpressed();
                    }
                    break;
                case JoystickOffset.Buttons2: // B
                    if (e.Value > 0)
                    {
                        button2Pressed();
                    }
                    else
                    {
                        button2Unpressed();
                    }
                    break;
                case JoystickOffset.Buttons3: // Y
                    if (e.Value > 0)
                    {
                        button3Pressed();
                    }
                    else
                    {
                        button3Unpressed();
                    }
                    break;
                case JoystickOffset.Buttons4: // Bumper R
                    if(e.Value > 0)
                    {
                        button4Pressed();
                    }
                    else
                    {
                        button4Unpressed();
                    }
                    break;
                case JoystickOffset.Buttons5: // Bumper L
                    if (e.Value > 0)
                    {
                        button5Pressed();
                    }
                    else
                    {
                        button5Unpressed();
                    }
                    break;
            }
        });
    }

    private void button0Pressed()
    {
        this._oSCEOSService.SendFanButton();
    }

    private void button0Unpressed()
    {
    }

    private void button1Pressed()
    {
        this._oSCEOSService.SendRemDimButton();
    }

    private void button1Unpressed()
    {
    }

    private void button2Pressed()
    {
        this._oSCEOSService.SendButton(@"Select_Last");
    }

    private void button2Unpressed()
    {
    }

    private void button3Pressed()
    {
        checkboxIsCoarse.IsChecked = !(checkboxIsCoarse.IsChecked ?? true);
    }

    private void button3Unpressed()
    {
    }

    private void button4Pressed()
    {

    }

    private void button4Unpressed()
    {
        this._oSCEOSService.SendPrevButton();
    }

    private void button5Pressed()
    {

    }

    private void button5Unpressed()
    {
        this._oSCEOSService.SendNextButton();
    }

    private void Button_Up_Click(object sender, RoutedEventArgs e)
    {
        this._oSCEOSService.SendTiltWheel(checkboxIsCoarse.IsChecked ?? true, 10.0f);
    }

    private void Button_Left_Click(object sender, RoutedEventArgs e)
    {
        this._oSCEOSService.SendPanWheel(checkboxIsCoarse.IsChecked ?? true, -10.0f);
    }

    private void Button_Right_Click(object sender, RoutedEventArgs e)
    {
        this._oSCEOSService.SendPanWheel(checkboxIsCoarse.IsChecked ?? true, 10.0f);
    }

    private void Button_Down_Click(object sender, RoutedEventArgs e)
    {
        this._oSCEOSService.SendTiltWheel(checkboxIsCoarse.IsChecked ?? true, -10.0f);
    }


    private void Button_Next_Click(object sender, RoutedEventArgs e)
    {
        this._oSCEOSService.SendNextButton();
    }

    private void Button_Prev_Click(object sender, RoutedEventArgs e)
    {
        this._oSCEOSService.SendPrevButton();
    }

    private void Connect_Click(object sender, RoutedEventArgs e)
    {
        connectButton.IsEnabled = false;
        this._oSCEOSService.Connect();
        disconnectButton.IsEnabled = true;
    }
    private void Disconnect_Click(object sender, RoutedEventArgs e)
    {
        this._oSCEOSService.Disconnect();
        connectButton.IsEnabled = true;
        disconnectButton.IsEnabled = false;
    }


    private void timerCallback(object? state)
    {
        bool isCourse = true;
        if (panDelta != 0f)
        {
            Dispatcher.Invoke(() => {
                isCourse = checkboxIsCoarse.IsChecked ?? true;
            });

            this._oSCEOSService.SendPanWheel(isCourse, panDelta);
        }

        if (tiltDelta != 0f)
        {
            Dispatcher.Invoke(() => {
                isCourse = checkboxIsCoarse.IsChecked ?? true;
            });

            this._oSCEOSService.SendTiltWheel(isCourse, tiltDelta);
        }

        if (zoomDelta != 0f)
        {
            Dispatcher.Invoke(() => {
                isCourse = checkboxIsCoarse.IsChecked ?? true;
            });

            this._oSCEOSService.SendZoomWheel(isCourse, zoomDelta);
        }

        if (edgeDelta != 0f)
        {
            Dispatcher.Invoke(() => {
                isCourse = checkboxIsCoarse.IsChecked ?? true;
            });

            this._oSCEOSService.SendEdgeWheel(isCourse, edgeDelta);
        }

        if (irisDelta != 0f)
        {
            Dispatcher.Invoke(() => {
                isCourse = checkboxIsCoarse.IsChecked ?? true;
            });

            this._oSCEOSService.SendIrisWheel(isCourse, irisDelta);
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        StopMonitoring_Click(null, null);
        Disconnect_Click(null, null);
    }

    private void sliderSensitivity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        this.sensitivity = 0.001 + (1 * (e.NewValue * e.NewValue));
        this.labelSensitivity?.Content = this.sensitivity;
    }

    private void _oSCEOSService_OSCMessageSent(object? sender, EventArgs.OSCMessageSentEventArgs e)
    {
        this.Dispatcher.Invoke(() => { 
            this.oscMessageTextBlock.Text = $"{e.Address} {string.Join(',', e.Data)}";
        });
    }

}