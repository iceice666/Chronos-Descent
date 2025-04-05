using System;
using ChronosDescent.Scripts.Core.Entity;
using Godot;

namespace ChronosDescent.Scripts.ActionManager;

/// <summary>
///     Manages input from different sources (keyboard/mouse, controller, touchscreen)
///     and provides a unified interface for accessing input state
/// </summary>
[GlobalClass]
public partial class UserInputManager : Control, IActionManager
{
    // Input sources enum
    public enum InputSource
    {
        KeyboardMouse,
        Controller,
        VirtualJoystick
    }

    private int _controllerIndex;

    [Export] private VirtualJoystick _moveJoystick;
    [Export] private VirtualJoystick _normalAttackJoystick;
    [Export] private VirtualJoystick _specialAttackJoystick;
    [Export] private VirtualJoystick _ultimateJoystick;
    [Export] private VirtualJoystick _lifeSavingJoystick;
    [Export] private Button _interactButton;

    // References to UI components
    private Control _virtualInputContainer;

    // Current active input source
    public InputSource CurrentInputSource { get; private set; }


    // Input vectors for movement and aiming
    public Vector2 MoveDirection { get; set; } = Vector2.Zero;
    public Vector2 LookDirection { get; set; } = Vector2.Zero;


    public override void _Ready()
    {
        // Initialize the input source based on device capabilities
        if (DisplayServer.IsTouchscreenAvailable()) CurrentInputSource = InputSource.VirtualJoystick;
        else if (Input.GetConnectedJoypads().Count > 0) CurrentInputSource = InputSource.Controller;
        else CurrentInputSource = InputSource.KeyboardMouse;

        GD.Print($"Current Input Source: {CurrentInputSource}");

        // Get reference to virtual input container using the node path
        _virtualInputContainer = GetNode<Control>("../VirtualInput");

        // Setup virtual buttons
        SetupVirtualButtons();

        // Set initial visibility
        UpdateVirtualInputVisibility();
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
            SwitchInputSource(InputSource.Controller);
        // Check if using touch/virtual joystick
        else if (DisplayServer.IsTouchscreenAvailable() &&
                 (_moveJoystick.IsPressed || _normalAttackJoystick.IsPressed))
            SwitchInputSource(InputSource.VirtualJoystick);
        // Check if using keyboard/mouse
        else if (IsAnyKeyboardInputActive() ||
                 Input.IsMouseButtonPressed(MouseButton.Left) ||
                 Input.IsMouseButtonPressed(MouseButton.Right))
            SwitchInputSource(InputSource.KeyboardMouse);
    }

    /// <summary>
    ///     Checks if controller is active based on button presses or joystick movement
    /// </summary>
    private bool IsControllerActive()
    {
        return Input.IsJoyButtonPressed(_controllerIndex, JoyButton.A)
               || Input.IsJoyButtonPressed(_controllerIndex, JoyButton.B)
               || Input.IsJoyButtonPressed(_controllerIndex, JoyButton.X)
               || Input.IsJoyButtonPressed(_controllerIndex, JoyButton.Y);
    }

    /// <summary>
    ///     Switches the current input source and updates UI accordingly
    /// </summary>
    private void SwitchInputSource(InputSource newSource)
    {
        if (CurrentInputSource == newSource) return;

        GD.Print($"Input Source changed: {newSource}");

        CurrentInputSource = newSource;
        UpdateVirtualInputVisibility();
        GlobalEventBus.Instance.Publish(GlobalEventVariant.InputSourceChanged, newSource);
    }

    /// <summary>
    ///     Updates visibility of virtual joystick based on the current input source
    /// </summary>
    private void UpdateVirtualInputVisibility()
    {
        _virtualInputContainer.Visible = CurrentInputSource == InputSource.VirtualJoystick;
    }

    /// <summary>
    ///     Processes movement input from current input source
    /// </summary>
    private void ProcessMovementInput()
    {
        MoveDirection = CurrentInputSource == InputSource.VirtualJoystick
            ? _moveJoystick.Output
            : Input.GetVector(
                "move_left", "move_right", "move_up", "move_down"
            );
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
                var output = Input.GetVector(
                    "aim_left", "aim_right", "aim_up", "aim_down"
                );
                if (output != Vector2.Zero) LookDirection = output;
                break;
            case InputSource.VirtualJoystick:
                if (_normalAttackJoystick.IsPressed)
                {
                    LookDirection = _normalAttackJoystick.Output;
                }
                else if (_specialAttackJoystick.IsPressed)
                {
                    LookDirection = _specialAttackJoystick.Output;
                }
                else if (_ultimateJoystick.IsPressed)
                {
                    LookDirection = _ultimateJoystick.Output;
                }
                else if (_lifeSavingJoystick.IsPressed)
                {
                    LookDirection = _lifeSavingJoystick.Output;
                }
                else
                {
                    LookDirection = _normalAttackJoystick.Output;
                }

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
        _interactButton.ButtonDown -= OnInteractPressed;
        _interactButton.ButtonUp -= OnInteractReleased;
    }
}