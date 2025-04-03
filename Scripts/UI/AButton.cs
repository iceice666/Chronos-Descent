using System.Collections.Generic;
using Godot;

namespace ChronosDescent.Scripts.UI;

public partial class AButton : Button
{
    private readonly Dictionary<int, Key> _specialCodeSequence = new()
    {
        { -1, Key.Up }, { 0, Key.Up },
        { 1, Key.Down }, { 2, Key.Down },
        { 3, Key.Left }, { 4, Key.Right },
        { 5, Key.Left }, { 6, Key.Right },
        { 7, Key.Semicolon }, { 8, Key.Apostrophe }
    };

    private int _specialCodeProgress = -1;

    public override void _Ready()
    {
        Pressed += () => GlobalEventBus.Instance.Publish(GlobalEventVariant.MagicButtonTriggered);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey eventKey || !eventKey.IsPressed()) return;

        if (_specialCodeSequence.TryGetValue(_specialCodeProgress, out var key) && key == eventKey.Keycode)
            _specialCodeProgress++;
        else
            _specialCodeProgress = -1;


        if (_specialCodeProgress == 9) Visible = true;
    }
}