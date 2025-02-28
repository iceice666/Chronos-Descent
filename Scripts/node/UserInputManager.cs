using Godot;
using System.Collections.Generic;

namespace ChronosDescent.Scripts.node;

/// <summary>
/// A versatile input component that handles keyboard/mouse, controller, and virtual joystick inputs.
/// Attach this to a player or input manager node and configure your desired input actions.
/// </summary>
[GlobalClass]
public partial class UserInputManager : Control
{
    #region Signals

    [Signal]
    public delegate void InputSourceChangedEventHandler(InputSource newSource);

    [Signal]
    public delegate void JoystickConnectedEventHandler(int deviceId, string deviceName);

    [Signal]
    public delegate void JoystickDisconnectedEventHandler(int deviceId);

    #endregion

    #region Enums

    public enum InputSource
    {
        KeyboardMouse,
        Controller,
        VirtualJoystick
    }

    #endregion

    #region Export Variables

    [ExportGroup("Input Settings")]
    [Export]
    public InputSource DefaultInputSource { get; set; } = InputSource.KeyboardMouse;

    [Export] public bool AutoSwitchInputDevice { get; set; } = true;

    [Export] public float JoystickDeadzone { get; set; } = 0.2f;

    [Export] public string SaveFilePath { get; set; } = "user://input_settings.cfg";

    [ExportGroup("Virtual Joystick References")]
    [Export]
    public NodePath LeftVirtualJoystickPath { get; set; }

    [Export] public NodePath RightVirtualJoystickPath { get; set; }

    #endregion

    #region Private Variables

    private InputSource _currentInputSource;
    private Control _leftVirtualJoystick;
    private Control _rightVirtualJoystick;
    private readonly Dictionary<string, InputAction> _inputActions = new();
    private ConfigFile _configFile = new();
    private bool _waitingForRemap;
    private string _actionToRemap = "";

    #endregion

    /// <summary>
    /// Represents an abstracted input action that can be triggered from any input source
    /// </summary>
    private class InputAction
    {
        public string Name { get; set; }
        public bool IsPressed { get; set; }
        public bool JustPressed { get; set; }
        public bool JustReleased { get; set; }
        public float AnalogValue { get; set; }
        public Vector2 Vector { get; set; }

        public InputAction(string name)
        {
            Name = name;
            Reset();
        }

        public void Reset()
        {
            JustPressed = false;
            JustReleased = false;
            // Don't reset IsPressed or AnalogValue as they should persist between frames
        }
    }

    #region Lifecycle Methods

    public override void _Ready()
    {
        // Set initial input source
        _currentInputSource = DefaultInputSource;

        // Connect joystick signals
        Input.JoyConnectionChanged += OnJoyConnectionChanged;

        // Get virtual joystick nodes if specified
        if (!string.IsNullOrEmpty(LeftVirtualJoystickPath))
            _leftVirtualJoystick = GetNode<Control>(LeftVirtualJoystickPath);

        if (!string.IsNullOrEmpty(RightVirtualJoystickPath))
            _rightVirtualJoystick = GetNode<Control>(RightVirtualJoystickPath);

        // Register default input actions
        RegisterDefaultActions();

        // Load any saved input mappings
        LoadInputMappings();

        // Print connected controllers
        foreach (var deviceId in Input.GetConnectedJoypads())
        {
            GD.Print($"Joystick connected: {Input.GetJoyName(deviceId)}");
        }
    }

    public override void _Process(double delta)
    {
        // Auto-detect input source changes if enabled
        if (AutoSwitchInputDevice)
            DetectInputSourceChange();

        // Update all registered input actions
        UpdateInputActions();
    }

    public override void _Input(InputEvent @event)
    {
        // Handle input remapping if active
        if (_waitingForRemap)
        {
            HandleRemapping(@event);
        }
    }

    public override void _ExitTree()
    {
        // Clean up signals
        Input.JoyConnectionChanged -= OnJoyConnectionChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Checks if an action is currently pressed
    /// </summary>
    public bool IsActionPressed(string action)
    {
        return _inputActions.TryGetValue(action, out var inputAction) && inputAction.IsPressed;
    }

    /// <summary>
    /// Checks if an action was just pressed this frame
    /// </summary>
    public bool IsActionJustPressed(string action)
    {
        return _inputActions.TryGetValue(action, out var inputAction) && inputAction.JustPressed;
    }

    /// <summary>
    /// Checks if an action was just released this frame
    /// </summary>
    public bool IsActionJustReleased(string action)
    {
        return _inputActions.TryGetValue(action, out var inputAction) && inputAction.JustReleased;
    }

    /// <summary>
    /// Gets the analog value for an action (0-1 for triggers, -1 to 1 for axes)
    /// </summary>
    public float GetActionStrength(string action)
    {
        return _inputActions.TryGetValue(action, out var inputAction) ? inputAction.AnalogValue : 0f;
    }

    /// <summary>
    /// Gets a Vector2 value for directional inputs
    /// </summary>
    public Vector2 GetVector(string actionLeft, string actionRight, string actionUp, string actionDown)
    {
        return new Vector2(
            GetActionStrength(actionRight) - GetActionStrength(actionLeft),
            GetActionStrength(actionDown) - GetActionStrength(actionUp)
        );
    }

    /// <summary>
    /// Gets a Vector2 value for a pre-configured vector action
    /// </summary>
    public Vector2 GetVector(string vectorActionName)
    {
        if (_inputActions.TryGetValue(vectorActionName, out var inputAction))
            return inputAction.Vector;
        return Vector2.Zero;
    }

    /// <summary>
    /// Manually set the active input source
    /// </summary>
    public void SetInputSource(InputSource source)
    {
        if (_currentInputSource == source) return;

        _currentInputSource = source;
        EmitSignal(SignalName.InputSourceChanged, (int)source);
        GD.Print($"Input source changed to: {source}");
    }

    /// <summary>
    /// Start the process of remapping an input action
    /// </summary>
    public void StartRemapping(string actionName)
    {
        _waitingForRemap = true;
        _actionToRemap = actionName;
        GD.Print($"Waiting for input to remap action: {actionName}");
    }

    /// <summary>
    /// Cancel an ongoing remapping process
    /// </summary>
    public void CancelRemapping()
    {
        _waitingForRemap = false;
        _actionToRemap = "";
    }

    /// <summary>
    /// Make the controller vibrate
    /// </summary>
    public static void Vibrate(float weakMagnitude = 0.5f, float strongMagnitude = 0.5f, float duration = 0.5f,
        int deviceId = 0)
    {
        if (Input.GetConnectedJoypads().Contains(deviceId))
        {
            Input.StartJoyVibration(deviceId, weakMagnitude, strongMagnitude, duration);
        }
    }

    /// <summary>
    /// Stop controller vibration
    /// </summary>
    public static void StopVibration(int deviceId = 0)
    {
        Input.StopJoyVibration(deviceId);
    }

    /// <summary>
    /// Get the current input source
    /// </summary>
    public InputSource GetCurrentInputSource()
    {
        return _currentInputSource;
    }

    /// <summary>
    /// Checks if a controller is connected
    /// </summary>
    public static bool IsControllerConnected(int deviceId = 0)
    {
        return Input.GetConnectedJoypads().Contains(deviceId);
    }

    /// <summary>
    /// Get a list of all connected controllers
    /// </summary>
    public static Godot.Collections.Array<int> GetConnectedControllers()
    {
        return Input.GetConnectedJoypads();
    }

    /// <summary>
    /// Get the name of a connected controller
    /// </summary>
    public string GetControllerName(int deviceId = 0)
    {
        return IsControllerConnected(deviceId) ? Input.GetJoyName(deviceId) : "Not Connected";
    }

    #endregion

    #region Private Methods

    private void RegisterDefaultActions()
    {
        // Movement actions
        _inputActions["move_left"] = new InputAction("move_left");
        _inputActions["move_right"] = new InputAction("move_right");
        _inputActions["move_up"] = new InputAction("move_up");
        _inputActions["move_down"] = new InputAction("move_down");

        // Direction vector (combines movement actions)
        _inputActions["movement"] = new InputAction("movement");

        // Action buttons
        _inputActions["jump"] = new InputAction("jump");
        _inputActions["attack"] = new InputAction("attack");
        _inputActions["interact"] = new InputAction("interact");
        _inputActions["dodge"] = new InputAction("dodge");

        // Camera/Look actions
        _inputActions["look_left"] = new InputAction("look_left");
        _inputActions["look_right"] = new InputAction("look_right");
        _inputActions["look_up"] = new InputAction("look_up");
        _inputActions["look_down"] = new InputAction("look_down");

        // Look vector (combines look actions)
        _inputActions["look"] = new InputAction("look");

        // Triggers
        _inputActions["left_trigger"] = new InputAction("left_trigger");
        _inputActions["right_trigger"] = new InputAction("right_trigger");

        // Add any game-specific actions here
    }

    private void UpdateInputActions()
    {
        // Reset just pressed/released states
        foreach (var action in _inputActions.Values)
        {
            action.Reset();
        }

        // Update actions based on current input source
        switch (_currentInputSource)
        {
            case InputSource.KeyboardMouse:
                UpdateKeyboardMouseInput();
                break;
            case InputSource.Controller:
                UpdateControllerInput();
                break;
            case InputSource.VirtualJoystick:
                UpdateVirtualJoystickInput();
                break;
        }

        // Update derived vector inputs
        UpdateVectorInputs();
    }

    private void UpdateKeyboardMouseInput()
    {
        // Update basic movement actions
        UpdateActionFromInput("move_left", Input.IsActionPressed("move_left"), Input.IsActionJustPressed("move_left"),
            Input.IsActionJustReleased("move_left"), Input.GetActionStrength("move_left"));
        UpdateActionFromInput("move_right", Input.IsActionPressed("move_right"),
            Input.IsActionJustPressed("move_right"), Input.IsActionJustReleased("move_right"),
            Input.GetActionStrength("move_right"));
        UpdateActionFromInput("move_up", Input.IsActionPressed("move_up"), Input.IsActionJustPressed("move_up"),
            Input.IsActionJustReleased("move_up"), Input.GetActionStrength("move_up"));
        UpdateActionFromInput("move_down", Input.IsActionPressed("move_down"), Input.IsActionJustPressed("move_down"),
            Input.IsActionJustReleased("move_down"), Input.GetActionStrength("move_down"));

        // Update action buttons
        UpdateActionFromInput("jump", Input.IsActionPressed("jump"), Input.IsActionJustPressed("jump"),
            Input.IsActionJustReleased("jump"));
        UpdateActionFromInput("attack", Input.IsActionPressed("attack"), Input.IsActionJustPressed("attack"),
            Input.IsActionJustReleased("attack"));
        UpdateActionFromInput("interact", Input.IsActionPressed("interact"), Input.IsActionJustPressed("interact"),
            Input.IsActionJustReleased("interact"));
        UpdateActionFromInput("dodge", Input.IsActionPressed("dodge"), Input.IsActionJustPressed("dodge"),
            Input.IsActionJustReleased("dodge"));

        // Mouse look - get mouse motion for this frame
        // Note: This requires handling _Input() for mouse motion events,
        // or using the InputEventMouseMotion in _Input if you need full mouse handling
    }

    private void UpdateControllerInput()
    {
        // Get the first connected controller
        int deviceId;
        if (Input.GetConnectedJoypads().Count > 0)
            deviceId = Input.GetConnectedJoypads()[0];
        else
            return; // No controller connected

        // Left stick (Movement)
        var leftX = Input.GetJoyAxis(deviceId, JoyAxis.LeftX);
        var leftY = Input.GetJoyAxis(deviceId, JoyAxis.LeftY);

        // Apply deadzone
        if (Mathf.Abs(leftX) < JoystickDeadzone) leftX = 0;
        if (Mathf.Abs(leftY) < JoystickDeadzone) leftY = 0;

        // Update movement actions based on left stick
        UpdateActionFromInput("move_left", leftX < -JoystickDeadzone, false, false, Mathf.Abs(Mathf.Min(0, leftX)));
        UpdateActionFromInput("move_right", leftX > JoystickDeadzone, false, false, Mathf.Max(0, leftX));
        UpdateActionFromInput("move_up", leftY < -JoystickDeadzone, false, false, Mathf.Abs(Mathf.Min(0, leftY)));
        UpdateActionFromInput("move_down", leftY > JoystickDeadzone, false, false, Mathf.Max(0, leftY));

        // Right stick (Look)
        var rightX = Input.GetJoyAxis(deviceId, JoyAxis.RightX);
        var rightY = Input.GetJoyAxis(deviceId, JoyAxis.RightY);

        // Apply deadzone
        if (Mathf.Abs(rightX) < JoystickDeadzone) rightX = 0;
        if (Mathf.Abs(rightY) < JoystickDeadzone) rightY = 0;

        // Update look actions based on right stick
        UpdateActionFromInput("look_left", rightX < -JoystickDeadzone, false, false, Mathf.Abs(Mathf.Min(0, rightX)));
        UpdateActionFromInput("look_right", rightX > JoystickDeadzone, false, false, Mathf.Max(0, rightX));
        UpdateActionFromInput("look_up", rightY < -JoystickDeadzone, false, false, Mathf.Abs(Mathf.Min(0, rightY)));
        UpdateActionFromInput("look_down", rightY > JoystickDeadzone, false, false, Mathf.Max(0, rightY));

        // Triggers
        var leftTrigger = Input.GetJoyAxis(deviceId, JoyAxis.TriggerLeft);
        var rightTrigger = Input.GetJoyAxis(deviceId, JoyAxis.TriggerRight);

        UpdateActionFromInput("left_trigger", leftTrigger > JoystickDeadzone, false, false, leftTrigger);
        UpdateActionFromInput("right_trigger", rightTrigger > JoystickDeadzone, false, false, rightTrigger);

        // Face buttons - adapt these based on your game's needs
        var jumpPressed = Input.IsJoyButtonPressed(deviceId, JoyButton.A);
        var jumpJustPressed = Input.IsJoyButtonPressed(deviceId, JoyButton.A) && !_inputActions["jump"].IsPressed;
        var jumpJustReleased = !Input.IsJoyButtonPressed(deviceId, JoyButton.A) && _inputActions["jump"].IsPressed;
        UpdateActionFromInput("jump", jumpPressed, jumpJustPressed, jumpJustReleased);

        var attackPressed = Input.IsJoyButtonPressed(deviceId, JoyButton.X);
        var attackJustPressed = Input.IsJoyButtonPressed(deviceId, JoyButton.X) && !_inputActions["attack"].IsPressed;
        var attackJustReleased = !Input.IsJoyButtonPressed(deviceId, JoyButton.X) && _inputActions["attack"].IsPressed;
        UpdateActionFromInput("attack", attackPressed, attackJustPressed, attackJustReleased);

        var interactPressed = Input.IsJoyButtonPressed(deviceId, JoyButton.B);
        var interactJustPressed =
            Input.IsJoyButtonPressed(deviceId, JoyButton.B) && !_inputActions["interact"].IsPressed;
        var interactJustReleased =
            !Input.IsJoyButtonPressed(deviceId, JoyButton.B) && _inputActions["interact"].IsPressed;
        UpdateActionFromInput("interact", interactPressed, interactJustPressed, interactJustReleased);

        var dodgePressed = Input.IsJoyButtonPressed(deviceId, JoyButton.Y);
        var dodgeJustPressed = Input.IsJoyButtonPressed(deviceId, JoyButton.Y) && !_inputActions["dodge"].IsPressed;
        var dodgeJustReleased = !Input.IsJoyButtonPressed(deviceId, JoyButton.Y) && _inputActions["dodge"].IsPressed;
        UpdateActionFromInput("dodge", dodgePressed, dodgeJustPressed, dodgeJustReleased);
    }

    private void UpdateVirtualJoystickInput()
    {
        // Check if virtual joystick nodes are available
        if (_leftVirtualJoystick == null || _rightVirtualJoystick == null)
            return;

        // Retrieve output values from virtual joystick nodes
        // This assumes your virtual joystick implements a GetOutput() method
        // Adjust based on your actual implementation
        var leftJoystickOutput = Vector2.Zero;
        var rightJoystickOutput = Vector2.Zero;

        // Try to get the output using different common property names
        // This makes it work with different virtual joystick implementations
        if (_leftVirtualJoystick.HasMethod("GetOutput"))
            leftJoystickOutput = (Vector2)_leftVirtualJoystick.Call("GetOutput");
        else if ((bool)_leftVirtualJoystick.Get("Output"))
            leftJoystickOutput = (Vector2)_leftVirtualJoystick.Get("Output");
        else if ((bool)_leftVirtualJoystick.Get("Value"))
            leftJoystickOutput = (Vector2)_leftVirtualJoystick.Get("Value");

        if (_rightVirtualJoystick.HasMethod("GetOutput"))
            rightJoystickOutput = (Vector2)_rightVirtualJoystick.Call("GetOutput");
        else if ((bool)_rightVirtualJoystick.Get("Output"))
            rightJoystickOutput = (Vector2)_rightVirtualJoystick.Get("Output");
        else if ((bool)_rightVirtualJoystick.Get("Value"))
            rightJoystickOutput = (Vector2)_rightVirtualJoystick.Get("Value");

        // Update movement actions based on left virtual joystick
        UpdateActionFromInput("move_left", leftJoystickOutput.X < -JoystickDeadzone, false, false,
            Mathf.Abs(Mathf.Min(0, leftJoystickOutput.X)));
        UpdateActionFromInput("move_right", leftJoystickOutput.X > JoystickDeadzone, false, false,
            Mathf.Max(0, leftJoystickOutput.X));
        UpdateActionFromInput("move_up", leftJoystickOutput.Y < -JoystickDeadzone, false, false,
            Mathf.Abs(Mathf.Min(0, leftJoystickOutput.Y)));
        UpdateActionFromInput("move_down", leftJoystickOutput.Y > JoystickDeadzone, false, false,
            Mathf.Max(0, leftJoystickOutput.Y));

        // Update look actions based on right virtual joystick
        UpdateActionFromInput("look_left", rightJoystickOutput.X < -JoystickDeadzone, false, false,
            Mathf.Abs(Mathf.Min(0, rightJoystickOutput.X)));
        UpdateActionFromInput("look_right", rightJoystickOutput.X > JoystickDeadzone, false, false,
            Mathf.Max(0, rightJoystickOutput.X));
        UpdateActionFromInput("look_up", rightJoystickOutput.Y < -JoystickDeadzone, false, false,
            Mathf.Abs(Mathf.Min(0, rightJoystickOutput.Y)));
        UpdateActionFromInput("look_down", rightJoystickOutput.Y > JoystickDeadzone, false, false,
            Mathf.Max(0, rightJoystickOutput.Y));

        // Virtual buttons should be handled through the regular input system
        // or by specific UI button nodes that trigger actions
        UpdateActionFromInput("jump", Input.IsActionPressed("jump"), Input.IsActionJustPressed("jump"),
            Input.IsActionJustReleased("jump"));
        UpdateActionFromInput("attack", Input.IsActionPressed("attack"), Input.IsActionJustPressed("attack"),
            Input.IsActionJustReleased("attack"));
        UpdateActionFromInput("interact", Input.IsActionPressed("interact"), Input.IsActionJustPressed("interact"),
            Input.IsActionJustReleased("interact"));
        UpdateActionFromInput("dodge", Input.IsActionPressed("dodge"), Input.IsActionJustPressed("dodge"),
            Input.IsActionJustReleased("dodge"));
    }

    private void UpdateVectorInputs()
    {
        // Update the movement vector
        _inputActions["movement"].Vector = new Vector2(
            GetActionStrength("move_right") - GetActionStrength("move_left"),
            GetActionStrength("move_down") - GetActionStrength("move_up")
        ).LimitLength(); // Normalize to prevent diagonal movement being faster

        // Update the look vector
        _inputActions["look"].Vector = new Vector2(
            GetActionStrength("look_right") - GetActionStrength("look_left"),
            GetActionStrength("look_down") - GetActionStrength("look_up")
        ).LimitLength();
    }

    private void UpdateActionFromInput(string actionName, bool isPressed, bool justPressed, bool justReleased,
        float strength = 1.0f)
    {
        if (!_inputActions.TryGetValue(actionName, out var action))
            return;

        // Update button states
        action.IsPressed = isPressed;
        action.JustPressed = justPressed;
        action.JustReleased = justReleased;

        // Update analog value (only if pressed, otherwise it's 0)
        action.AnalogValue = isPressed ? strength : 0f;
    }

    private void DetectInputSourceChange()
    {
        // Check for keyboard/mouse input
        if (Input.IsActionJustPressed("move_left") ||
            Input.IsActionJustPressed("move_right") ||
            Input.IsActionJustPressed("move_up") ||
            Input.IsActionJustPressed("move_down") ||
            Input.IsActionJustPressed("jump") ||
            Input.IsActionJustPressed("attack") ||
            Input.IsActionJustPressed("interact") ||
            Input.IsMouseButtonPressed(MouseButton.Left) ||
            Input.IsMouseButtonPressed(MouseButton.Right))
        {
            SetInputSource(InputSource.KeyboardMouse);
            return;
        }

        // Check for controller input
        if (Input.GetConnectedJoypads().Count > 0)
        {
            var deviceId = Input.GetConnectedJoypads()[0];

            // Check any joystick button or axis
            for (var i = 0; i < (int)JoyButton.Max; i++)
            {
                if (Input.IsJoyButtonPressed(deviceId, (JoyButton)i))
                {
                    SetInputSource(InputSource.Controller);
                    return;
                }
            }

            // Check joystick axes
            if (Mathf.Abs(Input.GetJoyAxis(deviceId, JoyAxis.LeftX)) > JoystickDeadzone ||
                Mathf.Abs(Input.GetJoyAxis(deviceId, JoyAxis.LeftY)) > JoystickDeadzone ||
                Mathf.Abs(Input.GetJoyAxis(deviceId, JoyAxis.RightX)) > JoystickDeadzone ||
                Mathf.Abs(Input.GetJoyAxis(deviceId, JoyAxis.RightY)) > JoystickDeadzone)
            {
                SetInputSource(InputSource.Controller);
                return;
            }
        }

        // Check for virtual joystick input
        if (_leftVirtualJoystick != null && _rightVirtualJoystick != null)
        {
            // This would depend on your virtual joystick implementation
            // Here's a generic check that assumes the joystick node has an "IsPressed" property
            var leftVirtualJoystickActive = false;
            var rightVirtualJoystickActive = false;

            if ((bool)_leftVirtualJoystick.Get("IsPressed"))
                leftVirtualJoystickActive = (bool)_leftVirtualJoystick.Get("IsPressed");
            else if (_leftVirtualJoystick.HasMethod("IsPressed"))
                leftVirtualJoystickActive = (bool)_leftVirtualJoystick.Call("IsPressed");

            if ((bool)_rightVirtualJoystick.Get("IsPressed"))
                rightVirtualJoystickActive = (bool)_rightVirtualJoystick.Get("IsPressed");
            else if (_rightVirtualJoystick.HasMethod("IsPressed"))
                rightVirtualJoystickActive = (bool)_rightVirtualJoystick.Call("IsPressed");

            if (leftVirtualJoystickActive || rightVirtualJoystickActive)
            {
                SetInputSource(InputSource.VirtualJoystick);
            }
        }
    }

    private void OnJoyConnectionChanged(long devId, bool connected)
    {
        // Need to manually cast here due to:
        // https://github.com/godotengine/godot/issues/65749
        var deviceId = (int)devId;
        if (connected)
        {
            var joyName = Input.GetJoyName(deviceId);
            GD.Print($"Joystick {deviceId} connected: {joyName}");
            EmitSignal(SignalName.JoystickConnected, deviceId, joyName);

            // Automatically switch to controller input if a controller is connected
            if (AutoSwitchInputDevice)
                SetInputSource(InputSource.Controller);
        }
        else
        {
            GD.Print($"Joystick {deviceId} disconnected");
            EmitSignal(SignalName.JoystickDisconnected, deviceId);

            // If the current input source is controller and no controllers are connected,
            // fall back to keyboard/mouse
            if (_currentInputSource == InputSource.Controller && Input.GetConnectedJoypads().Count == 0)
                SetInputSource(InputSource.KeyboardMouse);
        }
    }

    private void HandleRemapping(InputEvent @event)
    {
        if (string.IsNullOrEmpty(_actionToRemap))
            return;

        var validRemapEvent = false;

        switch (@event)
        {
            // Handle keyboard remapping
            case InputEventKey { Pressed: true } keyEvent:
                RemapAction(_actionToRemap, keyEvent);
                validRemapEvent = true;
                break;
            // Handle mouse button remapping
            case InputEventMouseButton { Pressed: true } mouseEvent:
                RemapAction(_actionToRemap, mouseEvent);
                validRemapEvent = true;
                break;
            // Handle joystick button remapping
            case InputEventJoypadButton { Pressed: true } joyButtonEvent:
                RemapAction(_actionToRemap, joyButtonEvent);
                validRemapEvent = true;
                break;
            // Handle joystick axis remapping
            case InputEventJoypadMotion joyMotionEvent when Mathf.Abs(joyMotionEvent.AxisValue) > 0.5f:
                // For axes, make sure we're getting a significant motion
                RemapAction(_actionToRemap, joyMotionEvent);
                validRemapEvent = true;
                break;
        }

        if (!validRemapEvent) return;

        SaveInputMappings();
        _waitingForRemap = false;
        _actionToRemap = "";
    }

    private static void RemapAction(string actionName, InputEvent newEvent)
    {
        if (!InputMap.HasAction(actionName))
        {
            GD.Print($"Creating new action: {actionName}");
            InputMap.AddAction(actionName);
        }

        switch (newEvent)
        {
            // Determine the type of event and clear only that type
            case InputEventKey:
            {
                // Clear all keyboard events for this action
                foreach (var oldEvent in InputMap.ActionGetEvents(actionName))
                {
                    if (oldEvent is InputEventKey)
                        InputMap.ActionEraseEvent(actionName, oldEvent);
                }

                break;
            }
            case InputEventMouseButton:
            {
                // Clear all mouse button events for this action
                foreach (var oldEvent in InputMap.ActionGetEvents(actionName))
                {
                    if (oldEvent is InputEventMouseButton)
                        InputMap.ActionEraseEvent(actionName, oldEvent);
                }

                break;
            }
            case InputEventJoypadButton:
            {
                // Clear all joystick button events for this action
                foreach (var oldEvent in InputMap.ActionGetEvents(actionName))
                {
                    if (oldEvent is InputEventJoypadButton)
                        InputMap.ActionEraseEvent(actionName, oldEvent);
                }

                break;
            }
            case InputEventJoypadMotion:
            {
                // Clear all joystick motion events for this action
                foreach (var oldEvent in InputMap.ActionGetEvents(actionName))
                {
                    if (oldEvent is InputEventJoypadMotion)
                        InputMap.ActionEraseEvent(actionName, oldEvent);
                }

                break;
            }
        }

        // Add the new event
        InputMap.ActionAddEvent(actionName, newEvent);
        GD.Print($"Remapped action '{actionName}' to {newEvent.GetType().Name}");
    }

    private void SaveInputMappings()
    {
        _configFile = new ConfigFile();

        // Save all input mappings
        foreach (string actionName in InputMap.GetActions())
        {
            var events = InputMap.ActionGetEvents(actionName);

            for (var i = 0; i < events.Count; i++)
            {
                var evt = events[i];

                if (evt is InputEventKey keyEvent)
                {
                    _configFile.SetValue("keyboard", $"{actionName}_{i}", keyEvent.Keycode.ToString());
                }
                else if (evt is InputEventMouseButton mouseEvent)
                {
                    _configFile.SetValue("mouse", $"{actionName}_{i}", mouseEvent.ButtonIndex.ToString());
                }
                else if (evt is InputEventJoypadButton joyButtonEvent)
                {
                    _configFile.SetValue("joystick_buttons", $"{actionName}_{i}",
                        joyButtonEvent.ButtonIndex.ToString());
                }
                else if (evt is InputEventJoypadMotion joyMotionEvent)
                {
                    _configFile.SetValue("joystick_axes", $"{actionName}_{i}",
                        new Vector2((long)joyMotionEvent.Axis, joyMotionEvent.AxisValue));
                }
            }
        }

        // Save current input source
        _configFile.SetValue("settings", "input_source", (int)_currentInputSource);
        _configFile.SetValue("settings", "deadzone", JoystickDeadzone);

        // Save to file
        var err = _configFile.Save(SaveFilePath);
        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to save input mappings: {err}");
        }
        else
        {
            GD.Print("Input mappings saved successfully");
        }
    }

    private void LoadInputMappings()
    {
        var err = _configFile.Load(SaveFilePath);
        if (err != Error.Ok)
        {
            GD.Print("No saved input mappings found, using defaults");
            return;
        }

        // Load input source setting
        if (_configFile.HasSectionKey("settings", "input_source"))
        {
            var sourceIndex = (int)_configFile.GetValue("settings", "input_source");
            _currentInputSource = (InputSource)sourceIndex;
        }

        // Load deadzone setting
        if (_configFile.HasSectionKey("settings", "deadzone"))
        {
            JoystickDeadzone = (float)_configFile.GetValue("settings", "deadzone");
        }

        // Load keyboard mappings
        var keyboardKeys = _configFile.GetSectionKeys("keyboard");
        foreach (var key in keyboardKeys)
        {
            var parts = key.Split('_');
            var actionName = parts[0];

            if (!InputMap.HasAction(actionName))
                InputMap.AddAction(actionName);

            var keycode = (Key)(int)_configFile.GetValue("keyboard", key);

            var newEvent = new InputEventKey();
            newEvent.Keycode = keycode;
            newEvent.Pressed = true;

            InputMap.ActionAddEvent(actionName, newEvent);
        }

        // Load mouse mappings
        var mouseKeys = _configFile.GetSectionKeys("mouse");
        foreach (var key in mouseKeys)
        {
            var parts = key.Split('_');
            var actionName = parts[0];

            if (!InputMap.HasAction(actionName))
                InputMap.AddAction(actionName);

            var buttonIndex = (MouseButton)(int)_configFile.GetValue("mouse", key);

            var newEvent = new InputEventMouseButton();
            newEvent.ButtonIndex = buttonIndex;
            newEvent.Pressed = true;

            InputMap.ActionAddEvent(actionName, newEvent);
        }

        // Load joystick button mappings
        var joystickButtonKeys = _configFile.GetSectionKeys("joystick_buttons");
        foreach (var key in joystickButtonKeys)
        {
            var parts = key.Split('_');
            var actionName = parts[0];

            if (!InputMap.HasAction(actionName))
                InputMap.AddAction(actionName);

            var buttonIndex = (JoyButton)(int)_configFile.GetValue("joystick_buttons", key);

            var newEvent = new InputEventJoypadButton();
            newEvent.ButtonIndex = buttonIndex;
            newEvent.Pressed = true;

            InputMap.ActionAddEvent(actionName, newEvent);
        }

        // Load joystick axis mappings
        var joystickAxisKeys = _configFile.GetSectionKeys("joystick_axes");
        foreach (var key in joystickAxisKeys)
        {
            var parts = key.Split('_');
            var actionName = parts[0];

            if (!InputMap.HasAction(actionName))
                InputMap.AddAction(actionName);

            var axisInfo = (Vector2)_configFile.GetValue("joystick_axes", key);

            var newEvent = new InputEventJoypadMotion();
            newEvent.Axis = (JoyAxis)(int)axisInfo.X;
            newEvent.AxisValue = axisInfo.Y;

            InputMap.ActionAddEvent(actionName, newEvent);
        }

        GD.Print("Input mappings loaded successfully");
    }

    #endregion
}