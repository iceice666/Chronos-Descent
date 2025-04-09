using System;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Entities;
using Godot;

namespace ChronosDescent.Scripts.ActionManager;

/// <summary>
///     Manages input from different sources (keyboard/mouse, controller, touchscreen)
///     and provides a unified interface for accessing input state
/// </summary>
[GlobalClass]
public partial class UserInputManager : Control, IActionManager
{
    public static UserInputManager Instance { get; private set; }

    // Input sources enum
    public enum InputSource
    {
        KeyboardMouse,
        Controller,
        VirtualJoystick
    }

    private int _controllerIndex;

    private VirtualJoystick _moveJoystick;
    private VirtualJoystick _normalAttackJoystick;
    private VirtualJoystick _specialAttackJoystick;
    private VirtualJoystick _ultimateJoystick;
    private VirtualJoystick _lifeSavingJoystick;
    private Button _interactButton;

    // References to UI components
    private Control _virtualInputContainer;

    // Current active input source
    private InputSource _currentInputSource;

    public InputSource CurrentInputSource
    {
        get => _currentInputSource;
        private set
        {
            if (_currentInputSource == value) return;

            GD.Print($"Input Source changed: {value}");
            if (Player.Instance != null) Player.Instance.StateLabel.Text = value.ToString();

            _virtualInputContainer.Visible = value == InputSource.VirtualJoystick;
            GlobalEventBus.Instance.Publish(GlobalEventVariant.InputSourceChanged, value);
            _currentInputSource = value;
        }
    }


    // Input vectors for movement and aiming
    public Vector2 MoveDirection { get; set; } = Vector2.Zero;
    public Vector2 LookDirection { get; set; } = Vector2.Zero;


    public override void _Ready()
    {
        Instance = this;

        // Get reference to virtual input container
        _virtualInputContainer = GetNode<Control>("../VirtualInput");
        _moveJoystick = _virtualInputContainer.GetNode<VirtualJoystick>("MoveJoystick");
        _normalAttackJoystick = _virtualInputContainer.GetNode<VirtualJoystick>("NormalJoystick");
        _specialAttackJoystick = _virtualInputContainer.GetNode<VirtualJoystick>("SpecialJoystick");
        _ultimateJoystick = _virtualInputContainer.GetNode<VirtualJoystick>("UltimateJoystick");
        _lifeSavingJoystick = _virtualInputContainer.GetNode<VirtualJoystick>("LifeSavingJoystick");
        _interactButton = _virtualInputContainer.GetNode<Button>("InteractButtonNode/InteractButton");


        // Initialize the input source based on device capabilities
        if (DisplayServer.IsTouchscreenAvailable())
        {
            CurrentInputSource = InputSource.VirtualJoystick;
        }
        else if (Input.GetConnectedJoypads().Count > 0)
        {
            CurrentInputSource = InputSource.Controller;

            // Set the controller index to the first connected controller
            var connectedJoypads = Input.GetConnectedJoypads();
            if (connectedJoypads.Count > 0)
            {
                _controllerIndex = connectedJoypads[0];
                GD.Print($"Using controller index: {_controllerIndex}");
            }

            // Initialize look direction to prevent it from being zero
            // This ensures we have a valid initial direction for aiming
            LookDirection = new Vector2(1, 0);
        }
        else
        {
            CurrentInputSource = InputSource.KeyboardMouse;
        }


        // Setup virtual buttons
        SetupVirtualButtons();

        // Set initial visibility
        _virtualInputContainer.Visible = _currentInputSource == InputSource.VirtualJoystick;

        // Connect to joypad connection events
        Input.JoyConnectionChanged += OnJoyConnectionChanged;
    }

    private void OnJoyConnectionChanged(long device, bool connected)
    {
        GD.Print($"Controller {device} {(connected ? "connected" : "disconnected")}");

        // Update controller index when a new controller is connected
        if (connected)
        {
            _controllerIndex = (int)device;
            CurrentInputSource = InputSource.Controller;
        }
        // If the current controller was disconnected, find another one
        else if (device == _controllerIndex)
        {
            var connectedJoypads = Input.GetConnectedJoypads();
            if (connectedJoypads.Count > 0)
            {
                _controllerIndex = connectedJoypads[0];
            }
            else
            {
                // No controllers left, switch to keyboard
                CurrentInputSource = InputSource.KeyboardMouse;
            }
        }
    }

    public override void _Process(double delta)
    {
        DetectInputSource();
        ProcessMovementInput();
        ProcessAimInput();
    }

    /// <summary>
    ///     Detects which input source is currently being used
    /// </summary>
    private void DetectInputSource()
    {
        // Check if using controller (optimization: early returns)
        if (IsControllerActive())
            CurrentInputSource = InputSource.Controller;
        // Check if using touch/virtual joystick
        else if (DisplayServer.IsTouchscreenAvailable() &&
                 (_moveJoystick.IsPressed || _normalAttackJoystick.IsPressed))
            CurrentInputSource = InputSource.VirtualJoystick;
        // Check if using keyboard
        else if (IsAnyKeyboardInputActive())
            CurrentInputSource = InputSource.KeyboardMouse;
    }

    /// <summary>
    ///     Checks if controller is active based on button presses or joystick movement
    /// </summary>
    private bool IsControllerActive()
    {
        // Check for any controller button press
        for (var button = 0; button <= (int)JoyButton.Max; button++)
        {
            if (Input.IsJoyButtonPressed(_controllerIndex, (JoyButton)button))
            {
                return true;
            }
        }

        // Check for any joystick movement
        for (var axis = 0; axis <= 5; axis++) // Check all common axes (left stick, right stick, triggers)
        {
            var value = Input.GetJoyAxis(_controllerIndex, (JoyAxis)axis);
            if (Mathf.Abs(value) > 0.25f) // Higher threshold to prevent false detection
            {
                return true;
            }
        }

        return false;
    }




    /// <summary>
    ///     Processes movement input from current input source
    /// </summary>
    private void ProcessMovementInput()
    {
        if (CurrentInputSource == InputSource.VirtualJoystick)
        {
            MoveDirection = _moveJoystick.Output;
            return;
        }

        var value = Input.GetVector(
            "move_left", "move_right", "move_up", "move_down"
        );

        MoveDirection = value;

        if (CurrentInputSource == InputSource.Controller && value != Vector2.Zero)
        {
            LookDirection = value;
        }
    }

    /// <summary>
    ///     Processes aim input from the current input source
    /// </summary>
    private void ProcessAimInput()
    {
        switch (CurrentInputSource)
        {
            case InputSource.KeyboardMouse:
            {
                var viewport = GetViewport();
                LookDirection = (viewport.GetMousePosition() - viewport.GetVisibleRect().Size / 2).LimitLength();
                break;
            }
            case InputSource.Controller:
                break;
            case InputSource.VirtualJoystick:
                var newValue = Vector2.Zero;

                if (_normalAttackJoystick.IsPressed)
                {
                    newValue = _normalAttackJoystick.Output;
                }
                else if (_specialAttackJoystick.IsPressed)
                {
                    newValue = _specialAttackJoystick.Output;
                }
                else if (_ultimateJoystick.IsPressed)
                {
                    newValue = _ultimateJoystick.Output;
                }
                else if (_lifeSavingJoystick.IsPressed)
                {
                    newValue = _lifeSavingJoystick.Output;
                }

                if (newValue != Vector2.Zero) LookDirection = newValue;

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    /// <summary>
    ///     Checks if any relevant keyboard input is active
    /// </summary>
    private static bool IsAnyKeyboardInputActive()
    {
        return Input.IsKeyPressed(Key.Space) || Input.IsKeyPressed(Key.Escape);
    }

    private void OnInteractPressed()
    {
        Input.ActionPress("interact");
    }

    private void OnInteractReleased()
    {
        Input.ActionRelease("interact");
    }

    private void SetupVirtualButtons()
    {
        _interactButton.ButtonDown += OnInteractPressed;
        _interactButton.ButtonUp += OnInteractReleased;
    }

    public override void _ExitTree()
    {
        // Disconnect button events
        _interactButton.ButtonDown -= OnInteractPressed;
        _interactButton.ButtonUp -= OnInteractReleased;

        // Disconnect joypad connection events
        Input.JoyConnectionChanged -= OnJoyConnectionChanged;
    }
}