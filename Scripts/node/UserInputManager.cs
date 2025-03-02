// ReSharper disable MemberCanBePrivate.Global

using System;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace ChronosDescent.Scripts.node;

/// <summary>
/// A versatile input component that handles keyboard/mouse, controller, and virtual joystick inputs.
/// This improved version directly tracks vector inputs and provides a more flexible keybinding system.
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

    [Signal]
    public delegate void ActionMappingChangedEventHandler(string actionName);

    #endregion

    #region Enums

    public enum InputSource
    {
        KeyboardMouse,
        Controller,
        VirtualJoystick
    }

    public enum InputType
    {
        Button,
        Axis,
        Vector
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

    #endregion

    #region Private Variables

    private InputSource _currentInputSource;

    private VirtualJoystick _leftVirtualVirtualJoystick;
    private VirtualJoystick _rightVirtualVirtualJoystick;
    private readonly Dictionary<string, InputAction> _inputActions = new();
    private readonly Dictionary<string, InputType> _actionTypes = new();
    private readonly List<string> _registeredActions = [];
    private ConfigFile _configFile = new();
    private bool _waitingForRemap;
    private string _actionToRemap = "";
    private Vector2 _center;

    #endregion

    /// <summary>
    /// Represents an abstracted input action that can be triggered from any input source
    /// </summary>
    private class InputAction
    {
        public bool IsPressed { get; set; }
        public bool JustPressed { get; set; }
        public bool JustReleased { get; set; }
        public float AnalogValue { get; set; }
        public Vector2 Vector { get; set; } = Vector2.Zero;

        public void Reset()
        {
            JustPressed = false;
            JustReleased = false;
            // Don't reset IsPressed or AnalogValue as they should persist between frames
        }
    }

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
    /// Gets the analog value for an action ([0,1] for triggers, [-1,1] for axes)
    /// </summary>
    public float GetActionStrength(string action)
    {
        return _inputActions.TryGetValue(action, out var inputAction) ? inputAction.AnalogValue : 0f;
    }

    /// <summary>
    /// Gets a Vector2 value for a vector action
    /// </summary>
    public Vector2 GetVector(string vectorActionName)
    {
        return _inputActions.TryGetValue(vectorActionName, out var inputAction) ? inputAction.Vector : Vector2.Zero;
    }

    /// <summary>
    /// Gets the movement vector (convenience method)
    /// </summary>
    public Vector2 GetMovementVector()
    {
        return GetVector("movement");
    }

    /// <summary>
    /// Gets the look vector (convenience method)
    /// </summary>
    public Vector2 GetLookVector()
    {
        return GetVector("look");
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
        if (!_registeredActions.Contains(actionName))
        {
            GD.PrintErr($"Cannot remap unregistered action: {actionName}");
            return;
        }

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
    public static void StartVibration(float weakMagnitude = 0.5f, float strongMagnitude = 0.5f, float duration = 0.5f,
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
    public static string GetControllerName(int deviceId = 0)
    {
        return IsControllerConnected(deviceId) ? Input.GetJoyName(deviceId) : "Not Connected";
    }

    /// <summary>
    /// Get a list of all registered action names
    /// </summary>
    public IReadOnlyList<string> GetRegisteredActions()
    {
        return _registeredActions;
    }

    /// <summary>
    /// Get the input type of specific action
    /// </summary>
    public InputType GetActionType(string actionName)
    {
        return _actionTypes.GetValueOrDefault(actionName, InputType.Button);
    }

    #endregion

    #region Private Methods

    private void RegisterDefaultActions()
    {
        // Register vector actions directly
        RegisterVectorAction("movement");
        RegisterVectorAction("look");

        // Register axis actions
        RegisterAxisAction("left_trigger");
        RegisterAxisAction("right_trigger");

        // Register button actions
        RegisterButtonAction("attack");
        RegisterButtonAction("use_item_1");
        RegisterButtonAction("use_item_2");
        RegisterButtonAction("use_item_3");
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
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateControllerInput()
    {
        // Get the first connected controller
        if (Input.GetConnectedJoypads().Count == 0)
            return; // No controller connected

        int deviceId = Input.GetConnectedJoypads()[0];

        // Handle Vector2 inputs directly
        UpdateVectorFromController("movement", deviceId, JoyAxis.LeftX, JoyAxis.LeftY);
        UpdateVectorFromController("look", deviceId, JoyAxis.RightX, JoyAxis.RightY);

        // Handle trigger axes
        var leftTrigger = Input.GetJoyAxis(deviceId, JoyAxis.TriggerLeft);
        var rightTrigger = Input.GetJoyAxis(deviceId, JoyAxis.TriggerRight);

        UpdateActionFromInput("left_trigger", leftTrigger > JoystickDeadzone, false, false, leftTrigger);
        UpdateActionFromInput("right_trigger", rightTrigger > JoystickDeadzone, false, false, rightTrigger);

        // Update button actions
        foreach (var action in _registeredActions)
        {
            if (_actionTypes[action] != InputType.Button) continue;

            // Handle action through the input system if mapped
            if (InputMap.HasAction(action))
            {
                UpdateActionFromInputSystem(action);
            }
        }
    }

    private void UpdateActionFromInputSystem(string buttonName)
    {
        UpdateActionFromInput(buttonName, Input.IsActionPressed(buttonName), Input.IsActionJustPressed(buttonName),
            Input.IsActionJustReleased(buttonName), Input.GetActionStrength(buttonName));
    }

    private void UpdateKeyboardMouseInput()
    {
        // Handle movement directly as a vector
        var movementVector = new Vector2(
            Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"),
            Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up")
        ).LimitLength();

        UpdateActionVector("movement", movementVector);

        // Handle look using mouse position relative to center
        var mousePosition = GetViewport().GetMousePosition();
        var relativePosition = mousePosition - _center;
        var lookVector = relativePosition.Normalized();
        var lookStrength = relativePosition.Length() / 100.0f; // Adjust divisor based on your needs
        lookStrength = Mathf.Min(lookStrength, 1.0f);

        UpdateActionVector("look", lookVector * lookStrength);

        // Update all registered button actions
        foreach (var action in _registeredActions
                     .Where(action => _actionTypes[action] == InputType.Button)
                     .Where(action => InputMap.HasAction(action)))
        {
            UpdateActionFromInputSystem(action);
        }
    }

    private void UpdateVirtualJoystickInput()
    {
        // Check if virtual joystick nodes are available
        if (_leftVirtualVirtualJoystick == null || _rightVirtualVirtualJoystick == null)
            return;

        // Update vector actions directly
        UpdateActionVector("movement", _leftVirtualVirtualJoystick);
        UpdateActionVector("look", _rightVirtualVirtualJoystick);

        // Update all registered button actions
        foreach (var action in _registeredActions
                     .Where(action => _actionTypes[action] == InputType.Button)
                     .Where(action => InputMap.HasAction(action)))
        {
            UpdateActionFromInputSystem(action);
        }
    }

    // TODO: replace with custom joystick node
    private static Vector2 GetJoystickOutput(Control joystick)
    {
        // Try to get the output using different common property names
        if (joystick.HasMethod("GetOutput"))
            return (Vector2)joystick.Call("GetOutput");

        if ((bool)joystick.Get("Output"))
            return (Vector2)joystick.Get("Output");

        if ((bool)joystick.Get("Value"))
            return (Vector2)joystick.Get("Value");

        return Vector2.Zero;
    }

    private void UpdateVectorFromController(string actionName, int deviceId, JoyAxis xAxis, JoyAxis yAxis)
    {
        var x = Input.GetJoyAxis(deviceId, xAxis);
        var y = Input.GetJoyAxis(deviceId, yAxis);

        // Apply deadzone
        if (Mathf.Abs(x) < JoystickDeadzone) x = 0;
        if (Mathf.Abs(y) < JoystickDeadzone) y = 0;

        var vector = new Vector2(x, y).LimitLength();
        UpdateActionVector(actionName, vector);
    }

    private void UpdateActionVector(string actionName, Vector2 vector)
    {
        if (!_inputActions.TryGetValue(actionName, out var action))
            return;

        action.Vector = vector;
        action.IsPressed = vector.LengthSquared() > 0.01f;
        action.AnalogValue = vector.Length();
    }

    private void UpdateActionVector(string actionName, VirtualJoystick joystick)
    {
        if (!_inputActions.TryGetValue(actionName, out var action))
            return;

        var output = joystick.Output;
        action.Vector = output;
        action.IsPressed = joystick.IsPressed;
        action.AnalogValue = output.Length();
    }

    private void UpdateActionFromInput(
        string actionName,
        bool isPressed, bool justPressed, bool justReleased,
        float strength = 1.0f
    )
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
        // Check for virtual joystick input
        if (_leftVirtualVirtualJoystick != null && _rightVirtualVirtualJoystick != null)
        {
            var leftVirtualJoystickActive = _leftVirtualVirtualJoystick.IsPressed;
            var rightVirtualJoystickActive = _rightVirtualVirtualJoystick.IsPressed;

            if (!leftVirtualJoystickActive && !rightVirtualJoystickActive) return;

            SetInputSource(InputSource.VirtualJoystick);
        }

        // Check for keyboard/mouse input
        if (_registeredActions
            .Where(action => _actionTypes[action] == InputType.Button)
            .Any(action => Input.IsActionJustPressed(action)))
        {
            SetInputSource(InputSource.KeyboardMouse);
            return;
        }

        if (Input.IsMouseButtonPressed(MouseButton.Left) || Input.IsMouseButtonPressed(MouseButton.Right))
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
                if (!Input.IsJoyButtonPressed(deviceId, (JoyButton)i)) continue;
                SetInputSource(InputSource.Controller);
                return;
            }

            // Check joystick axes
            if (Mathf.Abs(Input.GetJoyAxis(deviceId, JoyAxis.LeftX)) > JoystickDeadzone ||
                Mathf.Abs(Input.GetJoyAxis(deviceId, JoyAxis.LeftY)) > JoystickDeadzone ||
                Mathf.Abs(Input.GetJoyAxis(deviceId, JoyAxis.RightX)) > JoystickDeadzone ||
                Mathf.Abs(Input.GetJoyAxis(deviceId, JoyAxis.RightY)) > JoystickDeadzone)
            {
                SetInputSource(InputSource.Controller);
            }
        }
    }

    private static bool IsJoystickActive(Control joystick)
    {
        // Try different methods to check if joystick is active
        if ((bool)joystick.Get("IsPressed"))
            return (bool)joystick.Get("IsPressed");

        if (joystick.HasMethod("IsPressed"))
            return (bool)joystick.Call("IsPressed");

        // Try checking if the output vector is non-zero
        var output = GetJoystickOutput(joystick);
        return output.LengthSquared() > 0.01f;
    }

    private void OnJoyConnectionChanged(long devId, bool connected)
    {
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
        EmitSignal(SignalName.ActionMappingChanged, _actionToRemap);
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

        // Save registered actions and their types
        _configFile.SetValue("actions", "registered", _registeredActions.ToArray());
        foreach (var action in _registeredActions)
        {
            _configFile.SetValue("action_types", action, (int)_actionTypes[action]);
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

        // Load registered actions and their types
        if (_configFile.HasSectionKey("actions", "registered"))
        {
            var savedActions = (Godot.Collections.Array<string>)_configFile.GetValue("actions", "registered");
            foreach (var action in savedActions)
            {
                if (_configFile.HasSectionKey("action_types", action))
                {
                    var actionType = (InputType)(int)_configFile.GetValue("action_types", action);
                    switch (actionType)
                    {
                        case InputType.Button:
                            RegisterButtonAction(action);
                            break;
                        case InputType.Axis:
                            RegisterAxisAction(action);
                            break;
                        case InputType.Vector:
                            RegisterVectorAction(action);
                            break;
                    }
                }
            }
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

    /// <summary>
    /// Registers a new button-type action
    /// </summary>
    private void RegisterButtonAction(string actionName)
    {
        if (_inputActions.ContainsKey(actionName)) return;

        _inputActions[actionName] = new InputAction();
        _actionTypes[actionName] = InputType.Button;
        _registeredActions.Add(actionName);

        // Ensure input action exists in the InputMap
        if (!InputMap.HasAction(actionName))
        {
            InputMap.AddAction(actionName);
        }
    }

    /// <summary>
    /// Registers a new axis-type action
    /// </summary>
    private void RegisterAxisAction(string actionName)
    {
        if (_inputActions.ContainsKey(actionName)) return;
        _inputActions[actionName] = new InputAction();
        _actionTypes[actionName] = InputType.Axis;
        _registeredActions.Add(actionName);
    }

    /// <summary>
    /// Registers a new vector-type action
    /// </summary>
    private void RegisterVectorAction(string actionName)
    {
        if (_inputActions.ContainsKey(actionName)) return;
        _inputActions[actionName] = new InputAction();
        _actionTypes[actionName] = InputType.Vector;
        _registeredActions.Add(actionName);
    }


    private void OnWindowSizeChanged()
    {
        _center = GetViewport().GetVisibleRect().Size / 2;
    }

    #endregion

    #region Lifecycle Methods

    public override void _Ready()
    {
        // Set initial input source
        _currentInputSource = DefaultInputSource;

        // Connect joystick signals
        Input.JoyConnectionChanged += OnJoyConnectionChanged;

        // Get virtual joystick nodes 
        _leftVirtualVirtualJoystick = GetNode<VirtualJoystick>("/root/Autoload/UI/LeftJoystick");
        _rightVirtualVirtualJoystick = GetNode<VirtualJoystick>("/root/Autoload/UI/RightJoystick");

        // Register default input actions
        RegisterDefaultActions();

        // Load any saved input mappings
        LoadInputMappings();

        // Print connected controllers
        foreach (var deviceId in Input.GetConnectedJoypads())
        {
            GD.Print($"Joystick connected: {Input.GetJoyName(deviceId)}");
        }

        // Get the initial viewport size
        _center = GetViewport().GetVisibleRect().Size / 2;
        GetWindow().SizeChanged += OnWindowSizeChanged;
    }

    public override void _Process(double delta)
    {
        // Auto-detect input source changes if enabled
        if (AutoSwitchInputDevice) DetectInputSourceChange();
    }

    public override void _PhysicsProcess(double delta)
    {
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
        GetWindow().SizeChanged -= OnWindowSizeChanged;

        // Save input mappings
        SaveInputMappings();
    }

    #endregion
}