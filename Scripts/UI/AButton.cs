using Godot;

namespace ChronosDescent.Scripts.UI;

public partial class AButton : Button
{
    public override void _Ready()
    {
        Pressed += () => GlobalEventBus.Instance.Publish(GlobalEventVariant.MagicButtonTriggered);
    }
}