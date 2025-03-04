using System;
using System.Collections.Generic;
using Godot;

namespace ChronosDescent.Scripts.node;

/// <summary>
/// Manages input from different sources (keyboard/mouse, controller, touchscreen)
/// and provides a unified interface for accessing input state
/// </summary>
[GlobalClass]
public partial class UserInputManager : Control
{
    [Signal]
    public delegate void InputSourceChangedEventHandler(InputSource newSource);

    // Input sources enum
    public enum InputSource
    {
        KeyboardMouse,
        Controller,
        VirtualJoystick
    }

    // Current active input source
    public InputSource CurrentInputSource { get; private set; }

    // Registered button actions
    private readonly Dictionary<string, ButtonAction> _buttonActions = new();

    // Joystick deadzone threshold
    [Export] public float JoystickDeadzone { get; set; } = 0.2f;

    // References to UI components
    private Control _virtualInputContainer;
    private VirtualJoystick _leftVirtualJoystick;
    private VirtualJoystick _rightVirtualJoystick;

    // Input vectors for movement and aiming
    public Vector2 MovementInput { get; private set; } = Vector2.Zero;
    public Vector2 AimInput { get; private set; } = Vector2.Zero;

    // Optimize with static readonly fields for common checks
    private static readonly Key[] CommonKeyboardKeys =
    [
        Key.W, Key.A, Key.S, Key.D,
        Key.Space, Key.Enter, Key.Escape, Key.Tab
    ];

    // Button action state tracker
    private sealed class ButtonAction(string name)
    {
        public readonly string Name = name;
        public bool IsPressed { get; private set; }
        public bool IsJustPressed { get; private set; }
        public bool IsJustReleased { get; private set; }

        public void Update(bool pressed)
        {
            IsJustPressed = pressed && !IsPressed;
            IsJustReleased = !pressed && IsPressed;
            IsPressed = pressed;
        }
    }

    public override void _Ready()
    {
        // Initialize the input source based on device capabilities
        CurrentInputSource = DisplayServer.IsTouchscreenAvailable()
            ? InputSource.VirtualJoystick
            : InputSource.KeyboardMouse;

        // Get reference to virtual input container using the node path
        _virtualInputContainer = GetNode<Control>("/root/Autoload/UI/VirtualInput");
        _leftVirtualJoystick = _virtualInputContainer.GetNode<VirtualJoystick>("LeftJoystick");
        _rightVirtualJoystick = _virtualInputContainer.GetNode<VirtualJoystick>("RightJoystick");

        // Set initial visibility
        UpdateVirtualInputVisibility();
    }

    public override void _Process(double delta)
    {
        DetectInputSource();
        ProcessMovementInput();
        ProcessAimInput();
        ProcessButtonActions();
    }

    /// <summary>
    /// Registers a button action to be tracked by the input system
    /// </summary>
    public void RegisterButtonAction(string actionName)
    {
        if (!_buttonActions.ContainsKey(actionName))
        {
            _buttonActions[actionName] = new ButtonAction(actionName);
        }
    }

    /// <summary>
    /// Unregisters a button action from the input system
    /// </summary>
    public void UnregisterButtonAction(string actionName)
    {
        _buttonActions.Remove(actionName);
    }

    /// <summary>
    /// Checks if a button is currently pressed
    /// </summary>
    public bool IsButtonPressed(string actionName)
    {
        return _buttonActions.TryGetValue(actionName, out var action) && action.IsPressed;
    }

    /// <summary>
    /// Checks if a button was just pressed this frame
    /// </summary>
    public bool IsButtonJustPressed(string actionName)
    {
        return _buttonActions.TryGetValue(actionName, out var action) && action.IsJustPressed;
    }

    /// <summary>
    /// Checks if a button was just released this frame
    /// </summary>
    public bool IsButtonJustReleased(string actionName)
    {
        return _buttonActions.TryGetValue(actionName, out var action) && action.IsJustReleased;
    }

    /// <summary>
    /// Detects which input source is currently being used
    /// </summary>
    private void DetectInputSource()
    {
        // Check if using controller (optimization: early returns)
        if (IsControllerActive())
        {
            SwitchInputSource(InputSource.Controller);
            return;
        }

        // Check if using touch/virtual joystick
        if (DisplayServer.IsTouchscreenAvailable() &&
            (_leftVirtualJoystick.IsPressed || _rightVirtualJoystick.IsPressed))
        {
            SwitchInputSource(InputSource.VirtualJoystick);
            return;
        }

        // Check if using keyboard/mouse
        if (IsAnyKeyboardInputActive() || Input.IsMouseButtonPressed(MouseButton.Left) ||
            Input.IsMouseButtonPressed(MouseButton.Right))
        {
            SwitchInputSource(InputSource.KeyboardMouse);
        }
    }

    /// <summary>
    /// Checks if controller is active based on button presses or joystick movement
    /// </summary>
    private bool IsControllerActive()
    {
        if (Input.GetConnectedJoypads().Count == 0)
            return false;

        // Check common controller buttons
        if (Input.IsJoyButtonPressed(0, JoyButton.A) ||
            Input.IsJoyButtonPressed(0, JoyButton.B) ||
            Input.IsJoyButtonPressed(0, JoyButton.X) ||
            Input.IsJoyButtonPressed(0, JoyButton.Y))
            return true;

        // Check joystick axes
        return Math.Abs(Input.GetJoyAxis(0, JoyAxis.LeftX)) > JoystickDeadzone ||
               Math.Abs(Input.GetJoyAxis(0, JoyAxis.LeftY)) > JoystickDeadzone ||
               Math.Abs(Input.GetJoyAxis(0, JoyAxis.RightX)) > JoystickDeadzone ||
               Math.Abs(Input.GetJoyAxis(0, JoyAxis.RightY)) > JoystickDeadzone;
    }

    /// <summary>
    /// Switches the current input source and updates UI accordingly
    /// </summary>
    private void SwitchInputSource(InputSource newSource)
    {
        if (CurrentInputSource == newSource) return;

        CurrentInputSource = newSource;
        UpdateVirtualInputVisibility();
        EmitSignal(SignalName.InputSourceChanged, (int)CurrentInputSource);
    }

    /// <summary>
    /// Updates visibility of virtual joystick based on the current input source
    /// </summary>
    private void UpdateVirtualInputVisibility()
    {
        _virtualInputContainer.Visible = CurrentInputSource == InputSource.VirtualJoystick;
    }

    /// <summary>
    /// Processes movement input from current input source
    /// </summary>
    private void ProcessMovementInput()
    {
        MovementInput = CurrentInputSource switch
        {
            InputSource.KeyboardMouse => GetKeyboardMovementInput(),
            InputSource.Controller => GetControllerMovementInput(),
            InputSource.VirtualJoystick => GetVirtualJoystickMovementInput(),
            _ => Vector2.Zero
        };
    }

    /// <summary>
    /// Gets keyboard movement input
    /// </summary>
    private Vector2 GetKeyboardMovementInput()
    {
        return new Vector2(
            Input.GetAxis("move_left", "move_right"),
            Input.GetAxis("move_up", "move_down")
        );
    }

    /// <summary>
    /// Gets controller movement input with deadzone handling
    /// </summary>
    private Vector2 GetControllerMovementInput()
    {
        var input = new Vector2(
            Input.GetJoyAxis(0, JoyAxis.LeftX),
            Input.GetJoyAxis(0, JoyAxis.LeftY)
        );

        return ApplyDeadzone(input);
    }

    /// <summary>
    /// Gets virtual joystick movement input with deadzone handling
    /// </summary>
    private Vector2 GetVirtualJoystickMovementInput()
    {
        if (!_leftVirtualJoystick.IsPressed)
            return Vector2.Zero;

        return ApplyDeadzone(_leftVirtualJoystick.Output);
    }

    /// <summary>
    /// Processes aim input from the current input source
    /// </summary>
    private void ProcessAimInput()
    {
        AimInput = CurrentInputSource switch
        {
            InputSource.KeyboardMouse => GetMouseAimInput(),
            InputSource.Controller => GetControllerAimInput(),
            InputSource.VirtualJoystick => GetVirtualJoystickAimInput(),
            _ => Vector2.Zero
        };
    }

    /// <summary>
    /// Gets mouse aim input
    /// </summary>
    private Vector2 GetMouseAimInput()
    {
        var screenCenter = GetViewport().GetVisibleRect().Size / 2;
        var input = GetViewport().GetMousePosition() - screenCenter;

        return input.Length() > 0 ? input.Normalized() : Vector2.Zero;
    }

    /// <summary>
    /// Gets controller aim input with deadzone handling
    /// </summary>
    private Vector2 GetControllerAimInput()
    {
        var input = new Vector2(
            Input.GetJoyAxis(0, JoyAxis.RightX),
            Input.GetJoyAxis(0, JoyAxis.RightY)
        );

        return ApplyDeadzone(input);
    }

    /// <summary>
    /// Gets virtual joystick aim input with deadzone handling
    /// </summary>
    private Vector2 GetVirtualJoystickAimInput()
    {
        if (!_rightVirtualJoystick.IsPressed)
            return Vector2.Zero;

        return ApplyDeadzone(_rightVirtualJoystick.Output);
    }

    /// <summary>
    /// Applies deadzone to an input vector
    /// </summary>
    private Vector2 ApplyDeadzone(Vector2 input)
    {
        var length = input.Length();

        if (length < JoystickDeadzone)
            return Vector2.Zero;

        // Smoothly scale input from deadzone to 1.0
        return input.Normalized() * ((length - JoystickDeadzone) / (1.0f - JoystickDeadzone));
    }

    /// <summary>
    /// Processes all registered button actions
    /// </summary>
    private void ProcessButtonActions()
    {
        foreach (var action in _buttonActions.Values)
        {
            action.Update(Input.IsActionPressed(action.Name));
        }
    }

    /// <summary>
    /// Checks if any relevant keyboard input is active
    /// </summary>
    private bool IsAnyKeyboardInputActive()
    {
        // Check common keys
        foreach (var key in CommonKeyboardKeys)
        {
            if (Input.IsKeyPressed(key))
                return true;
        }

        // Check number keys for items
        for (var i = (int)Key.Key1; i <= (int)Key.Key9; i++)
        {
            if (Input.IsKeyPressed((Key)i))
                return true;
        }

        return false;
    }
}