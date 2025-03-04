using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ChronosDescent.Scripts.node;

/// <summary>
/// Manages input from different sources (keyboard/mouse, controller, touchscreen)
/// and provides a unified interface for accessing input state
/// </summary>
public partial class UserInputManager : Node
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
    public InputSource CurrentInputSource { get; private set; } = InputSource.KeyboardMouse;

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

    // Button action state tracker
    private class ButtonAction(string name)
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
        // Get reference to virtual input container
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
        if (_buttonActions.ContainsKey(actionName))
        {
            _buttonActions.Remove(actionName);
        }
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
        // Check if using controller
        if (Input.GetConnectedJoypads().Count > 0 && (
                Input.IsJoyButtonPressed(0, JoyButton.A) ||
                Input.IsJoyButtonPressed(0, JoyButton.B) ||
                Input.IsJoyButtonPressed(0, JoyButton.X) ||
                Input.IsJoyButtonPressed(0, JoyButton.Y) ||
                Math.Abs(Input.GetJoyAxis(0, JoyAxis.LeftX)) > JoystickDeadzone ||
                Math.Abs(Input.GetJoyAxis(0, JoyAxis.LeftY)) > JoystickDeadzone ||
                Math.Abs(Input.GetJoyAxis(0, JoyAxis.RightX)) > JoystickDeadzone ||
                Math.Abs(Input.GetJoyAxis(0, JoyAxis.RightY)) > JoystickDeadzone))
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
        MovementInput = Vector2.Zero;

        switch (CurrentInputSource)
        {
            case InputSource.KeyboardMouse:
                // Use WASD or arrow keys
                MovementInput = new Vector2(
                    Input.GetAxis("move_left", "move_right"),
                    Input.GetAxis("move_up", "move_down")
                );
                break;

            case InputSource.Controller:
                // Use left joystick
                MovementInput = new Vector2(
                    Input.GetJoyAxis(0, JoyAxis.LeftX),
                    Input.GetJoyAxis(0, JoyAxis.LeftY)
                );

                // Apply deadzone
                if (MovementInput.Length() < JoystickDeadzone)
                {
                    MovementInput = Vector2.Zero;
                }
                else
                {
                    MovementInput = MovementInput.Normalized() *
                                    ((MovementInput.Length() - JoystickDeadzone) / (1.0f - JoystickDeadzone));
                }

                break;

            case InputSource.VirtualJoystick:
                if (_leftVirtualJoystick.IsPressed)
                {
                    MovementInput = _leftVirtualJoystick.Output;

                    // Apply deadzone
                    if (MovementInput.Length() < JoystickDeadzone)
                    {
                        MovementInput = Vector2.Zero;
                    }
                    else
                    {
                        MovementInput = MovementInput.Normalized() *
                                        ((MovementInput.Length() - JoystickDeadzone) / (1.0f - JoystickDeadzone));
                    }
                }

                break;
        }
    }

    /// <summary>
    /// Processes aim input from current input source
    /// </summary>
    private void ProcessAimInput()
    {
        AimInput = Vector2.Zero;

        switch (CurrentInputSource)
        {
            case InputSource.KeyboardMouse:
                // Use mouse position relative to the screen center
                var screenCenter = GetViewport().GetVisibleRect().Size / 2;
                AimInput = GetViewport().GetMousePosition() - screenCenter;
                if (AimInput.Length() > 0)
                {
                    AimInput = AimInput.Normalized();
                }

                break;

            case InputSource.Controller:
                // Use right joystick
                AimInput = new Vector2(
                    Input.GetJoyAxis(0, JoyAxis.RightX),
                    Input.GetJoyAxis(0, JoyAxis.RightY)
                );

                // Apply deadzone
                if (AimInput.Length() < JoystickDeadzone)
                {
                    AimInput = Vector2.Zero;
                }
                else
                {
                    AimInput = AimInput.Normalized() *
                               ((AimInput.Length() - JoystickDeadzone) / (1.0f - JoystickDeadzone));
                }

                break;

            case InputSource.VirtualJoystick:
                // Using same joystick for both movement and aiming in this implementation
                AimInput = MovementInput;
                break;
        }
    }

    /// <summary>
    /// Processes all registered button actions using LINQ
    /// </summary>
    private void ProcessButtonActions()
    {
        _buttonActions.Values.ToList().ForEach(action =>
        {
            var isPressed = Input.IsActionPressed(action.Name);

            action.Update(isPressed);
        });
    }


    /// <summary>
    /// Resets the virtual joystick state
    /// </summary>
    public void ResetVirtualJoystick()
    {
        _leftVirtualJoystick?.Reset();
    }

    /// <summary>
    /// Checks if any relevant keyboard input is active
    /// </summary>
    private bool IsAnyKeyboardInputActive()
    {
        // Check common WASD keys
        if (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.A) ||
            Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.D))
            return true;

        // Check action keys
        if (Input.IsKeyPressed(Key.Space) || Input.IsKeyPressed(Key.Enter) ||
            Input.IsKeyPressed(Key.Escape) || Input.IsKeyPressed(Key.Tab))
            return true;

        // Check number keys for items
        for (var i = (int)Key.Key1; i <= (int)Key.Key9; i++)
        {
            if (Input.IsKeyPressed((Key)i))
                return true;
        }

        return false;
    }
}