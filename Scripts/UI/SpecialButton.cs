using System;
using Godot;

namespace ChronosDescent.Scripts.UI;

[GlobalClass]
public partial class SpecialButton : Button
{
    public Action OnPressed;


    public void Init(Action cb)
    {
        OnPressed = cb;
        Pressed += OnPressed;
    }

    public override void _ExitTree()
    {
        Pressed -= OnPressed;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!HasFocus()) return;
        if (@event is not InputEventJoypadButton e) return;
        foreach (var ie in InputMap.ActionGetEvents("confirm"))
        {
            if (ie is not InputEventJoypadButton iej) continue;
            if (iej.ButtonIndex != e.ButtonIndex) continue;
            if (iej.IsReleased()) continue;

            OnPressed();
            break;
        }
    }
}